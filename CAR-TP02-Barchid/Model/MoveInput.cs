using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Model
{
    public class MoveInput
    {
        [Required]
        public string OldPath { get; set; }

        [Required]
        public string TargetPath { get; set; }
    }
}
