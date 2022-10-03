using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogProject.Models
{
    public class BlogUser : IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Blog Image")]
        public byte[]? ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string? ContentType { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }

        [NotMapped]
        public string? FullName 
        { get 
            { 
                return $"{FirstName} {LastName}"; 
            } 
        }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        public virtual ICollection<Blog>? Blogs { get; set; } = new HashSet<Blog>();
        public virtual ICollection<Post>? Posts { get; set; } = new HashSet<Post>();
    }
}
