using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace VictorMVC.Models
{
    public class Article: OrderedTable
    {
        [Required]
        [DisplayName("Article Name")]
        [MaxLength(63, ErrorMessage = "Max Length of file name ")]
        public string Title { get; set; }    //Internal Name for article

        [Range(1,12)]
        [Required]
        [DisplayName("Columns: Desktop")]
        public int Cols_lg { get; set; }    //Bootstrap columns to occupy on large screens

        [Range(1,12)]
        [Required]
        [DisplayName("Columns: Tablet")]
        public int Cols_md { get; set; }    //Bootstrap columns to occupy on medium screens

        [Required]
        [DisplayName("Article Content")]
        [AllowHtml]
        public string Content { get; set; } //Html Content of article
    }
}