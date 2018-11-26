using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace VictorMVC.Models
{
    [Bind(Exclude="Salt")]
    public class Login
    {
        [Key]
        [Required]
        public string Username { get; set; }
        [Required]
        [ScaffoldColumn(false)]
        public string Salt { get; set; }
        [Required]
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public DateTime TokenExpire { get; set; }
    }
}