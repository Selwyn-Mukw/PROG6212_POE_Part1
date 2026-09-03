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