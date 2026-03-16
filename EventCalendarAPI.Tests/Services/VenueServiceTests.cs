using EventCalendarAPI.DTOs.Request;
using EventCalendarAPI.Exceptions;
using EventCalendarAPI.Interfaces;
using EventCalendarAPI.Models;
using EventCalendarAPI.Services;
using Moq;

namespace EventCalendarAPI.Tests.Services
{
    public class VenueServiceTests
    {
        private readonly Mock<IVenueRepository> _venueRepoMock = new();
        private readonly VenueService _sut;

        public VenueServiceTests()
        {
            _sut = new VenueService(_venueRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _venueRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsDto()
        {
            var venue = new Venue { Id = 1, Name = "Arena", Address = "123 St", City = "NYC", State = "NY", Country = "US", Capacity = 500 };
            _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

            var result = await _sut.GetByIdAsync(1);

            Assert.Equal("Arena", result.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaged()
        {
            var venues = new List<Venue> { new Venue { Id = 1, Name = "Arena", Address = "123 St", City = "NYC", State = "NY", Country = "US", Capacity = 500 } };
            _venueRepoMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(new PagedResult<Venue> { Items = venues, TotalCount = 1 });

            var result = await _sut.GetAllAsync(1, 10);

            Assert.Single(result.Items);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsDto()
        {
            _venueRepoMock.Setup(r => r.AddAsync(It.IsAny<Venue>())).ReturnsAsync((Venue v) => v);

            var request = new CreateVenueRequestDto
            {
                Name = "Stadium",
                Address = "456 Ave",
                City = "LA",
                State = "CA",
                Country = "US",
                Capacity = 1000
            };

            var result = await _sut.CreateAsync(request);

            Assert.Equal("Stadium", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            _venueRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venue?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _sut.UpdateAsync(99, new UpdateVenueRequestDto()));
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_SoftDeletes()
        {
            var venue = new Venue { Id = 1, Name = "Arena", Address = "123 St", City = "NYC", State = "NY", Country = "US", Capacity = 500, IsActive = true };
            _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);
            _venueRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Venue>())).ReturnsAsync((Venue v) => v);

            await _sut.DeleteAsync(1);

            Assert.False(venue.IsActive);
        }
    }
}
