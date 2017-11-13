using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HPCA_POC.Models
{
    public class UserModel
    {
        
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Grant_Type
        {
            get
            {
                return "password";
            }
             
        }           
    }

    public class JWTToken
    {
        public string Issuer { get; set; }
        public string GivenName { get; set; }
        public string FirstName { get; set; }

    }

    public class Token
    {
        public string Token_Type { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public string RedemptionCode { get; set; }
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Grant_Type
        {
            get
            {
                return "password";
            }

        }
    }

    public class Response
    {
        public string Data { get; set; }

        public int StatusCode { get; set; }
    }

    public class ResultModel
    {
        public string Data { get; set; }

        public string ResultUserData { get; set; }
    }
}