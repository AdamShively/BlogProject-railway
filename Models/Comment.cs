#nullable disable

using BlogProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; } //Foreign key relates comment to its parent post, Foreign key (GUID) translates to primary key in BlogUser model.
        public string BlogAuthorId { get; set; }
        public string BlogModeratorId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "Comment")]
        public string Body { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Updated")]
        public DateTime? Updated { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Moderated")]
        public DateTime? Moderated { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Deleted")]
        public DateTime? Deleted { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at between {2} and {1} characters.", MinimumLength = 2)]
        [Display(Name = "Moderated Comment")]
        public string ModeratedBody { get; set; }

        public ModerationReason ModerationReason { get; set; } 

        //Navigation properties.
        public virtual Post Post { get; set; }
        public virtual BlogUser BlogAuthor { get; set; }
        public virtual BlogUser BlogModerator { get; set; }
    }
}
