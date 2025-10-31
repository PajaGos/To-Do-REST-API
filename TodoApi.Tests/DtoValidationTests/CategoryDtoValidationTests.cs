using System.ComponentModel.DataAnnotations;
using TodoApi.DTOs.Category;
using TodoApi.DTOs.Tasks;
using Xunit;

namespace TodoApi.Tests
{
    public class CategoryDtoValidationTests
    {
        [Fact]
        public void Validate_ShouldFail_WhenRequiredFieldsAreMissingInTaskItemCreateDto()
        {
            var dto = new CategoryCreateDto();

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(dto.Name)));
        }
    }
}