using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ProductManagementFunction
{
    public static class NewImageFunction
    {
        [FunctionName("NewImageUploaded")]
        [return: Table("catalog",Connection = "CnxProductStorage")]
        public static ProductEntity NewImageUploaded(
            [BlobTrigger("productimages/{name}", Connection = "CnxProductStorage")]Stream newProductBlob, 
            string name,
            [Queue("invalidimages", Connection = "CnxProductStorage")] out string invalidImageMessage, 
            ILogger log)
        {
            log.LogInformation($"Analyzing blob [{name}]   Size: {newProductBlob.Length} Bytes");
            invalidImageMessage = null;

            var visionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(System.Environment.GetEnvironmentVariable("csVisionKey")))
            {
                Endpoint = System.Environment.GetEnvironmentVariable("csVisionEndpoint") // AppSetting are automatically mapped on EnvironmentVariable
            };

            var features = new VisualFeatureTypes[] {
                    VisualFeatureTypes.Adult,
                    VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Color,
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces,
                    VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Tags
                };

            var imgAnalysis =  visionClient.AnalyzeImageInStreamAsync(newProductBlob, features).GetAwaiter().GetResult();

            if (imgAnalysis.Adult.IsAdultContent || imgAnalysis.Adult.IsRacyContent ) // Image is potentially adult content -> reject it
            {
                invalidImageMessage = $"ERROR: [{name}] detected as adult content:  adultscore={imgAnalysis.Adult.AdultScore}     racyscore={imgAnalysis.Adult.RacyScore}";
                log.LogInformation(invalidImageMessage);
                return null; // NO product to add in catalog table
            }
            else
            {
                log.LogInformation($"[{name }] is : {imgAnalysis.Description.Captions.FirstOrDefault().Text}");
                Random rnd = new Random((int)DateTime.Now.Ticks);
                var pid = rnd.Next(1000,int.MaxValue);
                var p = new ProductEntity()
                {
                    PartitionKey = pid.ToString(),
                    RowKey = pid.ToString(),
                    CatalogItemsServiceUrl = System.Environment.GetEnvironmentVariable("baseCatalogServiceUrl"),
                    Id = pid,
                    OwnerId = 1,
                    ProductAllergyInfo = "None",
                    ProductId = pid,
                    ProductName = imgAnalysis.Description.Captions.FirstOrDefault().Text,
                    ProductPictureUrl = GetSasUrlForBlob(
                        System.Environment.GetEnvironmentVariable("CnxProductStorage"),
                        "productimages", // TODO : should be parameters
                        System.Environment.GetEnvironmentVariable("baseImageUrl") , 
                        name)
                };

                return p;
            }

        }

        static private string GetSasUrlForBlob(string storageCnxString, string containerName,string baseUrl , string blobname)
        {
            var conBuilder = new DbConnectionStringBuilder();
            conBuilder.ConnectionString = storageCnxString;
            var credential = new StorageSharedKeyCredential(conBuilder["AccountName"] as string, conBuilder["AccountKey"] as string);

            var sas = new BlobSasBuilder
            {
                BlobName = blobname,
                BlobContainerName = containerName,
                StartsOn = DateTime.Now.AddDays(-1), // validity start yesterday (avoid protential timezone trouble :D )
                ExpiresOn = DateTime.Now.AddYears(9) // 9year SAS lifetime
            };
            sas.SetPermissions(BlobAccountSasPermissions.Read);// SAS token only for READ blob


            UriBuilder sasUri = new UriBuilder()
            {
                Scheme = "https",
                Host = $"{conBuilder["AccountName"]}.blob.core.windows.net",
                Path = $"{containerName}/{blobname}",
                Query = sas.ToSasQueryParameters(credential).ToString()
            };

            return sasUri.Uri.ToString();
        }


        static string GetTextFromOcrResult(OcrResult imgOcr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var r in imgOcr.Regions)
            {
                foreach (var l in r.Lines)
                {
                    foreach (var w in l.Words)
                    {
                        sb.Append(w.Text);
                        sb.Append(" ");
                    }
                }
            }
            return sb.ToString();
        }



        static string FormatListOf<T>(IEnumerable<T> listOf, Func<T, string> extractor)
        {
            bool notFirst = false;
            StringBuilder sb = new StringBuilder();
            foreach (var obj in listOf)
            {
                if (notFirst)
                    sb.Append(" , ");
                sb.Append(extractor(obj));
                notFirst = true;
            }
            return sb.ToString();
        }



        static string FormatListOfString(IEnumerable<string> listOfString)
        {
            bool notFirst = false;
            StringBuilder sb = new StringBuilder();
            foreach (var s in listOfString)
            {
                if (notFirst)
                    sb.Append(",");
                sb.Append(s);
                notFirst = true;
            }
            return sb.ToString();
        }

        static void PrintAnalysisResult(ImageAnalysis imgAn)
        {

            Console.WriteLine("Vision Analysis result : ");

            Console.WriteLine($"-- Description : ");
            Console.WriteLine($"     |- Captions : {FormatListOf(imgAn.Description.Captions, (ImageCaption c) => c.Text)}");
            Console.WriteLine($"     |- Tags : {FormatListOfString(imgAn.Description.Tags)}");
            Console.WriteLine($"-- Categories : {FormatListOf(imgAn.Categories, (Category c) => $"{c.Name} [{c.Score}]")}");
            Console.WriteLine($"-- Tags : {FormatListOf(imgAn.Tags, (ImageTag t) => $"{t.Name} [{t.Confidence}]")}");
            Console.WriteLine($"-- Adult content : {imgAn.Adult.IsAdultContent}");
            Console.WriteLine($"     |- Adult score : {imgAn.Adult.AdultScore}");
            //Console.WriteLine($"-- Gore content : {imgAn.Adult.IsGoryContent}"); // only in v6 preview sdk
            //Console.WriteLine($"     |- Gore score : {imgAn.Adult.GoreScore}"); // only in v6 preview sdk
            Console.WriteLine($"-- Racy content : {imgAn.Adult.IsRacyContent}");
            Console.WriteLine($"     |- Racy score : {imgAn.Adult.RacyScore}");


        }
    }
}
