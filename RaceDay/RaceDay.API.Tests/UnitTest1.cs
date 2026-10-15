using Xunit;
using RaceDay.contracts.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Tests
{
    public class UnitTest1
    {
        // ==========================================
        // EVENT DTO TESTS
        // ==========================================

        [Fact]
        public void EventDto_ShouldStoreEventDetails()
        {
            var eventDto = new EventDto
            {
                EventId = 1,
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg"
            };

            Assert.Equal(1, eventDto.EventId);
            Assert.Equal("Johannesburg City Race", eventDto.EventName);
            Assert.Equal(new DateTime(2026, 11, 15), eventDto.EventDate);
            Assert.Equal("Johannesburg", eventDto.Location);
        }

        [Fact]
        public void CreateEventDto_ShouldAcceptValidEventDetails()
        {
            var eventDto = new CreateEventDto
            {
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Description = "A city running event",
                Distance = 5.00m,
                EventTypeId = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void CreateEventDto_ShouldRejectEventNameShorterThanThreeCharacters()
        {
            var eventDto = new CreateEventDto
            {
                EventName = "AB",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Distance = 5.00m,
                EventTypeId = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateEventDto_ShouldRejectDistanceBelowMinimum()
        {
            var eventDto = new CreateEventDto
            {
                EventName = "Johannesburg City Race",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Johannesburg",
                Distance = 0.00m,
                EventTypeId = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(eventDto);

            var isValid = Validator.TryValidateObject(
                eventDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        // ==========================================
        // CATEGORY DTO TESTS
        // ==========================================

        [Fact]
        public void CategoryDto_ShouldStoreCategoryDetails()
        {
            var categoryDto = new CategoryDto
            {
                CategoryId = 1,
                EventId = 1,
                CategoryName = "Junior 5K",
                CategoryType = "Running",
                MinimumAge = 10,
                MaximumAge = 17,
                CategoryDistance = 5.00m
            };

            Assert.Equal(1, categoryDto.CategoryId);
            Assert.Equal(1, categoryDto.EventId);
            Assert.Equal("Junior 5K", categoryDto.CategoryName);
            Assert.Equal("Running", categoryDto.CategoryType);
            Assert.Equal(10, categoryDto.MinimumAge);
            Assert.Equal(17, categoryDto.MaximumAge);
            Assert.Equal(5.00m, categoryDto.CategoryDistance);
        }

        [Fact]
        public void CreateCategoryDto_ShouldAcceptValidCategoryDetails()
        {
            var categoryDto = new CreateCategoryDto
            {
                EventId = 1,
                CategoryName = "Junior 5K",
                CategoryType = "Running",
                MinimumAge = 10,
                MaximumAge = 17,
                CategoryDistance = 5.00m
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(categoryDto);

            var isValid = Validator.TryValidateObject(
                categoryDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void CreateCategoryDto_ShouldRejectInvalidEventId()
        {
            var categoryDto = new CreateCategoryDto
            {
                EventId = 0,
                CategoryName = "Junior 5K",
                CategoryType = "Running",
                MinimumAge = 10,
                MaximumAge = 17,
                CategoryDistance = 5.00m
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(categoryDto);

            var isValid = Validator.TryValidateObject(
                categoryDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateCategoryDto_ShouldRejectCategoryNameShorterThanTwoCharacters()
        {
            var categoryDto = new CreateCategoryDto
            {
                EventId = 1,
                CategoryName = "A",
                CategoryType = "Running",
                MinimumAge = 10,
                MaximumAge = 17,
                CategoryDistance = 5.00m
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(categoryDto);

            var isValid = Validator.TryValidateObject(
                categoryDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateCategoryDto_ShouldRejectDistanceBelowMinimum()
        {
            var categoryDto = new CreateCategoryDto
            {
                EventId = 1,
                CategoryName = "Junior 5K",
                CategoryType = "Running",
                MinimumAge = 10,
                MaximumAge = 17,
                CategoryDistance = 0.00m
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(categoryDto);

            var isValid = Validator.TryValidateObject(
                categoryDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }
        [Fact]
        public void CreateEnrolmentDto_ShouldAcceptValidValues()
        {
            // Arrange
            var enrolmentDto = new CreateEnrolmentDto
            {
                EventId = 1,
                CategoryId = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(enrolmentDto);

            // Act
            var isValid = Validator.TryValidateObject(
                enrolmentDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void CreateEnrolmentDto_ShouldRejectInvalidEventId()
        {
            // Arrange
            var enrolmentDto = new CreateEnrolmentDto
            {
                EventId = 0,
                CategoryId = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(enrolmentDto);

            // Act
            var isValid = Validator.TryValidateObject(
                enrolmentDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateEnrolmentDto_ShouldRejectInvalidCategoryId()
        {
            // Arrange
            var enrolmentDto = new CreateEnrolmentDto
            {
                EventId = 1,
                CategoryId = 0
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(enrolmentDto);

            // Act
            var isValid = Validator.TryValidateObject(
                enrolmentDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }
        [Fact]
        public void CreateResultDto_ShouldAcceptValidValues()
        {
            // Arrange
            var resultDto = new CreateResultDto
            {
                EnrolmentId = 1,
                FinishTime = new TimeSpan(0, 32, 45),
                FinishPosition = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(resultDto);

            // Act
            var isValid = Validator.TryValidateObject(
                resultDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void CreateResultDto_ShouldRejectInvalidEnrolmentId()
        {
            // Arrange
            var resultDto = new CreateResultDto
            {
                EnrolmentId = 0,
                FinishTime = new TimeSpan(0, 32, 45),
                FinishPosition = 1
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(resultDto);

            // Act
            var isValid = Validator.TryValidateObject(
                resultDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void CreateResultDto_ShouldRejectInvalidFinishPosition()
        {
            // Arrange
            var resultDto = new CreateResultDto
            {
                EnrolmentId = 1,
                FinishTime = new TimeSpan(0, 32, 45),
                FinishPosition = 0
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(resultDto);

            // Act
            var isValid = Validator.TryValidateObject(
                resultDto,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(validationResults);
        }
    }

}