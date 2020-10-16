using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp01.Helper
{
    public static class ServiceDataHelper
    {
        static HttpClient _client = new HttpClient();

         static string DataServiceUrl 
        {
            get { 
                return "https://webapiappnet01.azurewebsites.net/api/customers"; 
            } 
             
        }
        
        public static async Task<string> GetCustomers()
        {
            string ret;


            ret = await _client.GetAsync(DataServiceUrl).Result.Content.ReadAsStringAsync();

            return ret;
        }
    }
}