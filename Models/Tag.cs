#nullable disable

using System.ComponentModel.DataAnnotations;

namespace BlogProject.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int PostId { get; set; } //Foreign key relates tag to its parent post, Foreign key (GUID) translates to primary key in BlogUser model.
        public string BlogAuthorId { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        public string Text { get; set; }

        //Navigation properties.
        public virtual Post Post { get; set; }
        public virtual BlogUser BlogAuthor { get; set; }
    }
}
