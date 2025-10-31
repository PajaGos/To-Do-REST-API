using TodoApi.DTOs.Category;
using TodoApi.Models;

namespace TodoApi.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(this Category category)
        {
            return new CategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
            };
        }

        public static Category ToEntity(this CategoryCreateDto dto)
        {
            return new Category()
            {
                Name = dto.Name,
                UserId = dto.UserId
            };
        }

        public static void UpdateFromDto(this Category category, CategoryUpdateDto dto)
        {
            if (dto.Name != null)
            {
                category.Name = dto.Name;
            }
        }
    }
}