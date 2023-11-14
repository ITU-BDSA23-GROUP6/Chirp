using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Models
{
    [Table("Cheeps")]
    public class Cheep
    {
        [Display(Name = "Cheep ID")]
        [Key]
        public int CheepId { get; set; }

        [Required]
        public Author Author { get; set; }

        [Required]
        [StringLength(160, MinimumLength = 1)]
        public string Text { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime TimeStamp { get; set; }
    }
}
