using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
