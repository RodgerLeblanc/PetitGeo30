using Newtonsoft.Json;
using PetitGeo30.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PetitGeo30.Services.Cloudant
{
    public class DocumentService
    {
        private string _baseCloudantUrl = "https://rodgerleblanc.cloudant.com/petitgeo30/";
        private string _documentName = "geocacheinfos";

        public async Task<GeoCacheModel> GetDocument()
        {
            GeoCacheModel geoCacheInfos = null;

            using (HttpClient client = GetHttpClient())
            {
                var url = _baseCloudantUrl + _documentName;
                var response = await client.GetAsync(url);

                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    var responseAsString = await response.Content.ReadAsStringAsync();
                    geoCacheInfos = JsonConvert.DeserializeObject<GeoCacheModel>(responseAsString);
                }
            }

            return geoCacheInfos;
        }

        private HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            var authData = string.Format($"{ApiKeys.CloudantUsername}:{ApiKeys.CloudantPassword}");
            var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
            return client;
        }
    }
}
