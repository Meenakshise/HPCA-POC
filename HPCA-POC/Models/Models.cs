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
    }

    public class ResultModel
    {
        public string Data { get; set; }
    }
}