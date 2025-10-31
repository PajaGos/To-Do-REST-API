using System.ComponentModel.DataAnnotations;
using TodoApi.DTOs.Tasks;
using TodoApi.DTOs.Users;
using Xunit;

namespace TodoApi.Tests
{
    public class TaskItemDtoValidationTests
    {
        [Fact]
        public void Validate_ShouldFail_WhenRequiredFieldsAreMissingInTaskItemCreateDto()
        {
            var dto = new TaskItemCreateDto();

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(dto.Title)));
        }
    }
}