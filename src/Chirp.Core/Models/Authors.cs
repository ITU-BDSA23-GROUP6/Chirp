using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Models
{
    [Table("Authors")]
    public class Author : IdentityUser
    {
        [Display(Name = "Author ID")]
        public override string Id { get; set; }

        [StringLength(20, MinimumLength = 5)]
        [Display(Name = "Username")]
        public required override string UserName { get; set; }

        [EmailAddress]
        public required override string Email { get; set; }
        public ICollection<Cheep> Cheeps { get; set; } = null!;
        public ISet<Author> Followers { get; set; } = null!;
        public ISet<Author> Following { get; set; } = null!;
    }
}