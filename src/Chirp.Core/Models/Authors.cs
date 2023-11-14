using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Models
{
    [Table("Authors")]
    public class Author : IdentityUser
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Author ID")]

        public override string Id { get; set; }   // [TODO]: Make this 'override' the attribute from 'IdentityUser'

        [StringLength(20, MinimumLength = 5)]
        [Display(Name = "Username")]
        public required override string UserName { get; set; }

        [EmailAddress]
        public required override string Email { get; set; }
        public ICollection<Cheep> Cheeps { get; set; } = null!;
    }
}