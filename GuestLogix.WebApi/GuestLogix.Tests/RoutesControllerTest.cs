using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.SelfHost;
using GuestLogix.Business.Models.Routes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GuestLogix.Tests
{
    [TestClass]
    public class RoutesControllerTest 
    {
       
        string baseURL = "http://localhost:58925/api/routes";


        [TestMethod]
        public void Routes_Get()
        {
            using (var client = new HttpClient())
            {

                var message = new HttpRequestMessage(HttpMethod.Get, this.baseURL+ "?origin=yyz&destination=yvr");
                message.Headers.Add("Accept", "application/json");
           

                var response = client.SendAsync(message, HttpCompletionOption.ResponseContentRead).Result.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject<Routes>(response);
               
                ValidateResult(result);

            }
        }
        private void ValidateResult(Routes result)
        {
            Assert.IsNull(result, "No Path");
            Assert.AreEqual(HttpStatusCode.OK, result.Path);
            
        }
    }
}
