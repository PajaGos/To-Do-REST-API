using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.Category
{
    public class CategoryCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }
        
        [Required]
        public int UserId { get; set; } // the owner of the category
    }
}