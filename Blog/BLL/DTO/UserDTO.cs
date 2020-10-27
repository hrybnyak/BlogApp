using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTO
{
    public class UserDto
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [RegularExpression(@"^.*(?=.{6,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(^[a-zA-Z0-9@\$=!:.#%]+$)")]
        public string Password { get; set; }
        public IEnumerable<BlogDto> Blogs { get; set; }
        public IEnumerable<CommentDto> Comments { get; set; }
    }
}
