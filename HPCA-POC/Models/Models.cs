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

    public class ResultModel
    {
        public string Data { get; set; }
    }
}