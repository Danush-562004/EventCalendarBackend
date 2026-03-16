using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _sut = new CategoryService(_categoryRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _categoryRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsDto()
        {
            var category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c", IsActive = true };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            var result = await _sut.GetByIdAsync(1);

            Assert.Equal("Work", result.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var categories = new List<Category> { new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c" } };
            _categoryRepoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new PagedResult<Category> { Items = categories, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateAsync_WhenNameExists_ThrowsDuplicateEntityException()
        {
            _categoryRepoMock.Setup(r => r.NameExistsAsync("Work")).ReturnsAsync(true);

            await Assert.ThrowsAsync<DuplicateEntityException>(() =>
                _sut.CreateAsync(new CreateCategoryRequestDto { Name = "Work", ColorCode = "#e74c3c" }));
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsDto()
        {
            _categoryRepoMock.Setup(r => r.NameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            var result = await _sut.CreateAsync(new CreateCategoryRequestDto { Name = "Sports", ColorCode = "#3498db" });

            Assert.Equal("Sports", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _categoryRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _sut.UpdateAsync(99, new UpdateCategoryRequestDto()));
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_SoftDeletes()
        {
            var category = new Category { Id = 1, Name = "Work", ColorCode = "#e74c3c", IsActive = true };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
            _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            await _sut.DeleteAsync(1);

            Assert.False(category.IsActive);
        }
    }
}
