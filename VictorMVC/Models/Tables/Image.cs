using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    public class Image: Table
    {
        public int Article_ID { get; set; }

        [MinLength(5, ErrorMessage = "File Name must be at least one charachter in length and include the file extension")]
        [MaxLength(63, ErrorMessage = "Max Length of file name ")]
        public string FileName { get; set; }
    }
}