using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HPCA_POC.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HPCA_POC.Controllers
{
    public class IndexController : Controller
    {
        string NetworkKey = "oAGpVeNR64waw77EDZ9zd05HgQZjhl6A.=.gKkA1zn4c1yDvv6dnbyUxQQlkm7sXc7y7O3pvwQ/f+7qz/tzipWQHqn9TBvrSXTqCXbG7PsBmBgMsl+DwRHftg==.=.";
        string DataView = "~/Views/Home/Result.cshtml";
        string DevURI = "https://www.moonshrub.com/hpca/";
        //Hellllloooooooooooo......
        // GET: Index
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListOfUsers(UserModel model)
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
            var x509 = new X509Certificate2("E://HPCA-POC/publicrsa.cer");
            ResultModel objModel = new ResultModel();
            LoginModel loginModel = new LoginModel();
            loginModel.UserName = model.Email;
            loginModel.Password = model.Password;
            var response = Authenticate(loginModel);
            objModel.Data = response.Data;
            if(response.StatusCode == 200)
            {
                Token objToken = JsonConvert.DeserializeObject<Token>(response.Data);
                var objPrinicipal = Validate(objToken.Access_Token);
                if (objPrinicipal != null)
                {
                    objModel.ResultUserData = objPrinicipal.FindFirst(ClaimTypes.Upn).Value + " " + objPrinicipal.FindFirst(ClaimTypes.GivenName).Value + " " + objPrinicipal.FindFirst(ClaimTypes.Surname).Value;
                }
            }

            return View(DataView, objModel);
        }

        public UserModel ParseToken(ClaimsPrincipal objPrinicipal)
        {
            UserModel objUserModel = new UserModel();
            return objUserModel;
        }

        public ClaimsPrincipal Validate(string jwtToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var x509 = new X509Certificate2("E://HPCA-POC/publicrsa.cer");
            var validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                IssuerSigningKey = new X509SecurityKey(x509),
                ValidAudiences = new List<string> { "/PLATFORMSERVICES/DEMO" },
                ValidIssuer = "http://auth-demo.systemc.com"
            };
            SecurityToken token = null;
            ClaimsPrincipal principal = jwtHandler.ValidateToken(jwtToken, validationParameters, out token);
            return principal;
        }

        public Response Authenticate(LoginModel model)
        {
            Response objRes = new Models.Response();
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
                    objRes.Data = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    objRes.Data = response.ReasonPhrase;

                }
                objRes.StatusCode = (int)response.StatusCode;
                return objRes;
            }
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
                    data = "User " + response.ReasonPhrase + (response.StatusCode.ToString() == "OK" ? " User already exists but has been enrolled into the network" : "");
                }
                else
                {
                    data = response.ReasonPhrase;
                }
                return data;
            }
        }
        public string ListOfUsersAddedtoNetwork()
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
                    data = response.Content.ReadAsStringAsync().Result;
                }
            }
            return data;
        }
        public ActionResult ResetPassword(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            LoginModel loginModel = new LoginModel();
            loginModel.UserName = model.Email;
            var data = ResetPasswordConfirmation(model);
            objModel.Data = data;
            return View(DataView, objModel);
        }
        public string ResetPasswordConfirmation(UserModel model)
        {
            string data = "";
            using (var client = new HttpClient())
            {
                {
                    var postData = new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(model.Email) } };
                    HttpContent content = new FormUrlEncodedContent(postData);
                    client.BaseAddress = new Uri(DevURI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "users/" + model.Email + "/password-reset-email")
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(model.Email),
                                                        Encoding.UTF8,
                                                        "application/json")//CONTENT-TYPE header
                    };
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        data = "User Password Reset  " + response.ReasonPhrase + (response.StatusCode.ToString() == " OK" ? "Not Registered user." : "");
                    }
                    else
                    {
                        data = response.ReasonPhrase;
                    }
                }
                return data;
            }
        }
    }
}