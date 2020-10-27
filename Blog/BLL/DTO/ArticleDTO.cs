using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTO
{
    public class ArticleDto
    {
        public int? Id { get; set; }

        [MaxLength(500)]
        [Required]
        public string Name { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime LastUpdate { get; set; }
        [Required]
        public int? BlogId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorUsername { get; set; }
        public ICollection<CommentDto> Comments { get; set; }
        public ICollection<TegDto> Tegs { get; set; }
    }
}