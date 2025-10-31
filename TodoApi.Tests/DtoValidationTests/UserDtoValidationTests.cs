using System.ComponentModel.DataAnnotations;
using TodoApi.DTOs.Users;
using Xunit;

namespace TodoApi.Tests
{
    public class UserDtoValidationTests
    {
        [Fact]
        public void Validate_ShouldFail_WhenRequiredFieldsAreMissingInUserCreateDto()
        {
            var dto = new UserCreateDto();

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(dto.UserName)));
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(dto.Email)));
        }
    }
}