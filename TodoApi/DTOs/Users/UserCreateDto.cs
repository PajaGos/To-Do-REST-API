using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.Users
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}