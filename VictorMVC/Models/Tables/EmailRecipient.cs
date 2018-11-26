using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    public class EmailRecipient: UnorderedTable
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}