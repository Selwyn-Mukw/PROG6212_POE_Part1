CREATE DATABASE RaceDay;
USE RaceDay;
CREATE TABLE [USER]
(
    UserID INT IDENTITY(1,1),
    FirstName VARCHAR(50),
    LastName VARCHAR(50),
    Email VARCHAR(100),
    PhoneNumber VARCHAR(20),
    PasswordHash VARCHAR(255),
    Role VARCHAR(20),
    ProfilePictureURL VARCHAR(500),
    CreatedAt DATETIME2
);

CREATE TABLE [EVENT TYPE]
(
    EventTypeID INT IDENTITY(1,1),
    TypeName VARCHAR(50)
);

CREATE TABLE [EVENT]
(
    EventID INT IDENTITY(1,1),
    OrganiserID INT,
    EventTypeID INT,
    EventName VARCHAR(100),
    Description VARCHAR(500),
    EventDate DATETIME2,
    Location VARCHAR(150),
    Distance DECIMAL(6,2),
    BannerImageURL VARCHAR(500)
);

CREATE TABLE [CATEGORY]
(
    CategoryID INT IDENTITY(1,1),
    EventID INT,
    CategoryName VARCHAR(100),
    CategoryType VARCHAR(50),
    MinimumAge INT,
    MaximumAge INT,
    CategoryDistance DECIMAL(6,2)
);