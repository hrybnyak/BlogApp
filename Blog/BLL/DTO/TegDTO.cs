﻿using System.ComponentModel.DataAnnotations;

namespace BLL.DTO
{
    public class TegDto
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

    }
}