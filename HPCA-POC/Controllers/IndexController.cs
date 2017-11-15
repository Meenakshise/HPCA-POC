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
using Newtonsoft.Json.Linq;

namespace HPCA_POC.Controllers
{
    public class IndexController : Controller
    {
        string NetworkKey = "oAGpVeNR64waw77EDZ9zd05HgQZjhl6A.=.gKkA1zn4c1yDvv6dnbyUxQQlkm7sXc7y7O3pvwQ/f+7qz/tzipWQHqn9TBvrSXTqCXbG7PsBmBgMsl+DwRHftg==.=.";
        string DataView = "~/Views/Home/Result.cshtml";
        string RefreshTokenView = "~/Views/Home/RefreshTokenView.cshtml";
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
            ResultModel objModel = new ResultModel();
            LoginModel loginModel = new LoginModel();
            loginModel.UserName = model.Email;
            loginModel.Password = model.Password;
            var response = Authenticate(loginModel);
            objModel.Data = response.Data;
            if(response.StatusCode == 200)
            {
                Token objToken = JsonConvert.DeserializeObject<Token>(response.Data);
                objModel.refreshToken = objToken.Refresh_Token;
                var objPrinicipal = Validate(objToken.Access_Token);
                if (objPrinicipal != null)
                {
                    objModel.ResultUserData = objPrinicipal.FindFirst(ClaimTypes.Upn).Value + " " + objPrinicipal.FindFirst(ClaimTypes.GivenName).Value + " " + objPrinicipal.FindFirst(ClaimTypes.Surname).Value;
                    DateTime startTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(objPrinicipal.FindFirst(JwtRegisteredClaimNames.Nbf).Value));
                    DateTime expiryTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(objPrinicipal.FindFirst(JwtRegisteredClaimNames.Exp).Value));
                    objModel.OldStartTime = startTime;
                    objModel.OldExpTime = expiryTime;
                }
            }

            return View(DataView, objModel);
        }
        public ActionResult RefreshToken(ResultModel resultModel)
        {
            ResultModel objModel = new ResultModel();
            UserModel model = new UserModel();
            var Json = JsonConvert.DeserializeObject<Token>(resultModel.Data);
            var token = Json.Access_Token;
            var refTok = Json.Refresh_Token;
            var response = AuthenticateAfterLogin(refTok);
            if (response.StatusCode == 200)
            {
                Token objToken = JsonConvert.DeserializeObject<Token>(response.Data);
                objModel.refreshToken = objToken.Refresh_Token;
                var objPrinicipal = Validate(objToken.Access_Token);
                if (objPrinicipal != null)
                {
                    DateTime startTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(objPrinicipal.FindFirst(JwtRegisteredClaimNames.Nbf).Value));
                    DateTime expiryTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(objPrinicipal.FindFirst(JwtRegisteredClaimNames.Exp).Value));
                    objModel.NewStartTime = startTime;
                    objModel.RefreshedExpTime = expiryTime;
                }
            }

            return View(RefreshTokenView, objModel);
        }

        public Response AuthenticateAfterLogin(string refToken)
        {
            Response objRes = new Models.Response();
            UserModel usrModel = new UserModel();
            dynamic refresh = new JObject();
            refresh.refresh_token = refToken;
            refresh.grant_type = usrModel.Grant_Type_Ref;

            using (var client = new HttpClient())
            {
                var postData = new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(refresh) } };
                HttpContent content = new FormUrlEncodedContent(postData);
                client.BaseAddress = new Uri(DevURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(refresh),
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
        public ActionResult ReSendEmail(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            var response = ResendEmailtoUser(model);
            objModel.Data = response.Data;
            return View(DataView, objModel);
        }

        public ActionResult UserStatus(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            var response = CurrentStatusOfUser(model);
            objModel.Data = response.Data;
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
            string path = Server.MapPath("/");
            var x509 = new X509Certificate2(path + "publicrsa.cer");
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

        public Response CurrentStatusOfUser(UserModel model)
        {
            int StatusCode = 0;
            Response objRes = new Models.Response();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(DevURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("NetworkKey", NetworkKey);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "users/" + model.Email);

                HttpResponseMessage response = client.SendAsync(request).Result;
                StatusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    Token objToken = JsonConvert.DeserializeObject<Token>(response.Content.ReadAsStringAsync().Result);
                    if (objToken != null && !string.IsNullOrEmpty(objToken.RedemptionCode))
                        objRes.Data = "User registered in HPCA and an Email sent but the user is yet to set the password";
                    else if (objToken != null && string.IsNullOrEmpty(objToken.RedemptionCode))
                        objRes.Data = "User has set the password and user is ready to login";
                }
                else
                {
                    objRes.Data = response.ReasonPhrase;
                }
                objRes.StatusCode = (int)response.StatusCode;
                return objRes;
            }
        }
        public Response ResendEmailtoUser(UserModel model)
        {
            Response objRes = new Models.Response();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(DevURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "users/" + model.Email + "/confirmation-email");
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    objRes.Data = "Confirmation email resent to the user";
                }
                else
                {
                    objRes.Data = response.ReasonPhrase;
                }
                objRes.StatusCode = (int)response.StatusCode;
                return objRes;
            }

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
                var u =  JsonConvert.SerializeObject(model);
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
        public ActionResult ForgotPassword(UserModel model)
        {
            ResultModel objModel = new ResultModel();
            LoginModel loginModel = new LoginModel();
            loginModel.UserName = model.Email;
            var data = ForgotPasswordConfirmation(model);
            objModel.Data = data;
            return View(DataView, objModel);
        }
        public string ForgotPasswordConfirmation(UserModel model)
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