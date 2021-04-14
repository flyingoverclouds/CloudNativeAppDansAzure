using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                    ProductPictureUrl = System.Environment.GetEnvironmentVariable("baseImageUrl") + name
                };

                return p;
            }

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
        /*

                public static void Run2(
                        [BlobTrigger("new/{name}", Connection = "CnxStorageAccountDemo")] Stream myBlob,  // the blob that trigger the function executation
                        string name,    // name of the blob (like found in container in the storage account
                        [Queue("readyfortranslation", Connection = "CnxStorageAccountDemo")] out string messageToTranslate, // OUTput paremeters used bye function runtime to push a new message in the queue named 'readyfortranslation', using the same storage account connexion as the blobtrigger
                        ILogger log // logger interface 
                )
                {
                    log.LogInformation($"OCRing blob [{name}]   Size: {myBlob.Length} Bytes");

                    var visionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(System.Environment.GetEnvironmentVariable("csVisionKey")))
                    {
                        Endpoint = System.Environment.GetEnvironmentVariable("csVisionEndpoint") // AppSetting are automatically mapped on EnvironmentVariable
                    };

                    var imgOcr = visionClient.RecognizePrintedTextInStreamAsync(true, myBlob).GetAwaiter().GetResult();

                    if (imgOcr != null)
                    {
                        string textToTranslate = GetTextFromOcrResult(imgOcr);
                        messageToTranslate = $"{name}##{textToTranslate}";
                        // HACK : the previous line use a simple formatting to send the blob name AND the ocr text to the next function.
                        // In real life, we must save the original text+langguage in a DB, push only the name of the blob 
                        // eg : we can use a CosmosDB database with json document linkied to each scanned document. this json doc will be enriching by each function (OCr, then translation, then feature extraction, ...)

                        log.LogInformation("MESSAGE pushed to queue : " + messageToTranslate);
                    }
                    else
                    {
                        messageToTranslate = string.Empty;
                        log.LogError($"ERROR WHILE OCRING BLOB {name}!");
                    }
                }
        */
    }
}
