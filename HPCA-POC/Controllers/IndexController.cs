using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HPCA_POC.Models;
using Newtonsoft.Json;

namespace HPCA_POC.Controllers
{
    public class IndexController : Controller
    {
        string NetworkKey = "oAGpVeNR64waw77EDZ9zd05HgQZjhl6A.=.gKkA1zn4c1yDvv6dnbyUxQQlkm7sXc7y7O3pvwQ/f+7qz/tzipWQHqn9TBvrSXTqCXbG7PsBmBgMsl+DwRHftg==.=.";
        string DataView = "~/Views/Home/Result.cshtml";
        string DevURI = "https://www.moonshrub.com/hpca/";
        
        // GET: Index
        public ActionResult Index()
        {
            return View();
        }

        public  ActionResult ListOfUsers(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            var data = ListOfUsersAddedtoNetwork();
            objModel.Data = data;
            return View(DataView, objModel);
        }

        public ActionResult CreateUser(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            var data = CreateAndPostUsertoNetwork(model);
            objModel.Data = data;
            return View(DataView, objModel);
        }

        public ActionResult Login(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            LoginModel loginModel = new LoginModel();
            loginModel.UserName = model.Email;
            loginModel.Password = model.Password;
            var data = Authenticate(loginModel);
            objModel.Data = data;
            return View(DataView, objModel);
        }

        public void Validate(string jwtToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

        }

        public string Authenticate(LoginModel model)
        {
            string data = "";
            using (var client = new HttpClient())
            {
                var postData = new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(model) } };
                HttpContent content = new FormUrlEncodedContent(postData);
                client.BaseAddress = new Uri(DevURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(model),
                                                    Encoding.UTF8,
                                                    "application/json")//CONTENT-TYPE header
                };
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    data = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    data = response.ReasonPhrase;
                }
                return data;
            }
            return data;
        }

        public string CreateAndPostUsertoNetwork(UserModel model)
        {
            string data = "";
            using (var client = new HttpClient())
            {
                var postData = new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(model) } };
                HttpContent content = new FormUrlEncodedContent(postData);
                client.BaseAddress = new Uri(DevURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "users")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(model),
                                                    Encoding.UTF8,
                                                    "application/json")//CONTENT-TYPE header
                };
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    data = "User " + response.ReasonPhrase + ( response.StatusCode.ToString() == "OK" ? " User already exists but has been enrolled into the network" : "");
                }
                else
                {
                    data = response.ReasonPhrase;
                }
                return data;
            }           
        }
        public  string ListOfUsersAddedtoNetwork()
        {
            string data = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(DevURI);
                
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                HttpResponseMessage response = client.GetAsync("users").Result;
                if (response.IsSuccessStatusCode)
                {
                      data =   response.Content.ReadAsStringAsync().Result;
                }               
            }
            return data;
        }
    }
}