using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    //Form Model for sending email messages. This should not be attached to any context.
    public class EmailForm
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Message { get; set; }
    }
}