using TodoApi.DTOs.Users;
using TodoApi.Models;

namespace TodoApi.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Email = user.Email,
                UserName = user.UserName,
            };
        }

        public static User ToEntity(this UserCreateDto dto)
        {
            return new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
            };
        }

        public static void UpdateFromDto(this User user, UserUpdateDto dto)
        {
            if (dto.Email != null)
            {
                user.Email = dto.Email;
            }
            if (dto.UserName != null)
            {
                user.UserName = dto.UserName;
            }
        }
    }
}