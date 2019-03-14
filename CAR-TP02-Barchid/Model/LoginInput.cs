using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Model
{
    public class LoginInput
    {
        [Required]
        public string User { get; set; }

        [Required]
        public string Pass { get; set; }
    }
}
