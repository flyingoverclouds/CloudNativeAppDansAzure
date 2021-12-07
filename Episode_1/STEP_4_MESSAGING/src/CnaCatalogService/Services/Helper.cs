using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnaCatalogService.Services
{
    public class Helper
    {
        public static async Task<Stream> DownloadPictureFromUrlAsync(string productPictureUrl, HttpClient httpClient)
        {

            Stream stream = null;
           
            try
            {
                var response = await httpClient.GetAsync(productPictureUrl);

                if (response.IsSuccessStatusCode)
                {
                    stream = await response.Content.ReadAsStreamAsync();

                }
            }
            catch (HttpRequestException)
            {
                // swallow the error 
                // TODO : Log the error Application Insight
            }

            return stream;
        }
    }
}
