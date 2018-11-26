using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    public class Demo: OrderedTable
    {

        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "wav,mp3", ErrorMessage = "Uploads must end in file extensions .wav or .mp3")]
        [MinLength(5, ErrorMessage = "File Name must be at least one charachter in length and include the file extension")]
        [MaxLength(63, ErrorMessage = "Max Length of file name ")]
        [DisplayName("File")]
        [Editable(true)]
        public string File { get; set; }

        [Required()]
        [DataType(DataType.Text)]
        [MaxLength(63, ErrorMessage = "Max Length of Title is 63 charachters")]
        [DisplayName("Title")]
        [Editable(true)]
        public string Title { get; set; }
    }
}