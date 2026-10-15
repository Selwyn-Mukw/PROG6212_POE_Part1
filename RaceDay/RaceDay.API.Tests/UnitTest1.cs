using Xunit;
using RaceDay.contracts.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void EventDto_ShouldStoreEventDetails()
        {
            // Arrange
            var eventDto = new EventDto
            {
                EventId = 1,
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg"
            };

            // Assert
            Assert.Equal(1, eventDto.EventId);
            Assert.Equal("Johannesburg City Race", eventDto.EventName);
            Assert.Equal(new DateTime(2026, 11, 15), eventDto.EventDate);
            Assert.Equal("Johannesburg", eventDto.Location);
        }

        [Fact]
        public void CreateEventDto_ShouldAcceptValidEventDetails()
        {
            // Arrange
            var eventDto = new CreateEventDto
            {
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Description = "A city running event",
                Distance = 5.00m,
                EventTypeId = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void CreateEventDto_ShouldRejectEventNameShorterThanThreeCharacters()
        {
            // Arrange
            var eventDto = new CreateEventDto
            {
                EventName = "AB",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Distance = 5.00m,
                EventTypeId = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateEventDto_ShouldRejectDistanceBelowMinimum()
        {
            // Arrange
            var eventDto = new CreateEventDto
            {
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Distance = 0.00m,
                EventTypeId = 1
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }
    }
}