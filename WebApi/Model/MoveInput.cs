using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Model
{
    /// <summary>
    /// Model class used in the body of a MOVE request.
    /// </summary>
    public class MoveInput
    {
        [Required]
        public string OldPath { get; set; }

        [Required]
        public string TargetPath { get; set; }
    }
}
