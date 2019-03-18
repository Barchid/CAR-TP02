using System.ComponentModel.DataAnnotations;

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
