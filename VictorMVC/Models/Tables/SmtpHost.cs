using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    public class SmtpHost : UnorderedTable
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        public bool AuthenticationRequired { get; set; }
        public bool UseSSL { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}