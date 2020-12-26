using System;
using System.ComponentModel.DataAnnotations;

namespace BookLib.Models
{
    public class AuthorForUpdateDto
    {
        [Required(ErrorMessage = "Name cannot be empty")]
        [MaxLength(20, ErrorMessage = "Length of name cannot be more than 20")]
        public string Name { get; set; }

        [Range(12, 120, ErrorMessage = "Age shoule be 12 to 120")]
        public int Age { get; set; }

        [Required(ErrorMessage = "BirthPlace is mandatory")]
        [MaxLength(40)]
        public string BirthPlace { get; set; }

        [EmailAddress(ErrorMessage = "Email format is not correct")]
        public string Email { get; set; }
    }
}
