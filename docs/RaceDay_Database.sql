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

CREATE TABLE [ENROLMENT]
(
    EnrolmentID INT IDENTITY(1,1),
    ParticipantID INT,
    EventID INT,
    CategoryID INT,
    EnrolmentDate DATETIME2,
    EnrolmentStatus VARCHAR(30)
);

CREATE TABLE [RESULT]
(
    ResultID INT IDENTITY(1,1),
    EnrolmentID INT,
    FinishTime TIME,
    FinishPosition INT,
    ResultStatus VARCHAR(30)
);


ALTER TABLE [USER]
ADD CONSTRAINT PK_USER
PRIMARY KEY (UserID);

ALTER TABLE [EVENT TYPE]
ADD CONSTRAINT PK_EVENT_TYPE
PRIMARY KEY (EventTypeID);

ALTER TABLE [EVENT]
ADD CONSTRAINT PK_EVENT
PRIMARY KEY (EventID);

ALTER TABLE [CATEGORY]
ADD CONSTRAINT PK_CATEGORY
PRIMARY KEY (CategoryID);

ALTER TABLE [ENROLMENT]
ADD CONSTRAINT PK_ENROLMENT
PRIMARY KEY (EnrolmentID);

ALTER TABLE [RESULT]
ADD CONSTRAINT PK_RESULT
PRIMARY KEY (ResultID);


ALTER TABLE [EVENT]
ADD CONSTRAINT FK_EVENT_USER
FOREIGN KEY (OrganiserID)
REFERENCES [USER](UserID);

ALTER TABLE [EVENT]
ADD CONSTRAINT FK_EVENT_EVENT_TYPE
FOREIGN KEY (EventTypeID)
REFERENCES [EVENT TYPE](EventTypeID);

ALTER TABLE [CATEGORY]
ADD CONSTRAINT FK_CATEGORY_EVENT
FOREIGN KEY (EventID)
REFERENCES [EVENT](EventID);

ALTER TABLE [ENROLMENT]
ADD CONSTRAINT FK_ENROLMENT_USER
FOREIGN KEY (ParticipantID)
REFERENCES [USER](UserID);

ALTER TABLE [ENROLMENT]
ADD CONSTRAINT FK_ENROLMENT_EVENT
FOREIGN KEY (EventID)
REFERENCES [EVENT](EventID);

ALTER TABLE [ENROLMENT]
ADD CONSTRAINT FK_ENROLMENT_CATEGORY
FOREIGN KEY (CategoryID)
REFERENCES [CATEGORY](CategoryID);

ALTER TABLE [RESULT]
ADD CONSTRAINT FK_RESULT_ENROLMENT
FOREIGN KEY (EnrolmentID)
REFERENCES [ENROLMENT](EnrolmentID);


ALTER TABLE [USER]
ALTER COLUMN FirstName VARCHAR(50) NOT NULL;

ALTER TABLE [USER]
ALTER COLUMN LastName VARCHAR(50) NOT NULL;

ALTER TABLE [USER]
ALTER COLUMN Email VARCHAR(100) NOT NULL;

ALTER TABLE [USER]
ALTER COLUMN PasswordHash VARCHAR(255) NOT NULL;

ALTER TABLE [USER]
ALTER COLUMN Role VARCHAR(20) NOT NULL;

ALTER TABLE [USER]
ALTER COLUMN CreatedAt DATETIME2 NOT NULL;


ALTER TABLE [USER]
ADD CONSTRAINT UQ_USER_Email UNIQUE (Email);


ALTER TABLE [USER]
ADD CONSTRAINT DF_USER_CreatedAt
DEFAULT SYSDATETIME() FOR CreatedAt;


ALTER TABLE [EVENT TYPE]
ALTER COLUMN TypeName VARCHAR(50) NOT NULL;

ALTER TABLE [EVENT TYPE]
ADD CONSTRAINT UQ_EVENT_TYPE_Name UNIQUE (TypeName);


ALTER TABLE [EVENT]
ALTER COLUMN OrganiserID INT NOT NULL;

ALTER TABLE [EVENT]
ALTER COLUMN EventTypeID INT NOT NULL;

ALTER TABLE [EVENT]
ALTER COLUMN EventName VARCHAR(100) NOT NULL;

ALTER TABLE [EVENT]
ALTER COLUMN EventDate DATETIME2 NOT NULL;

ALTER TABLE [EVENT]
ALTER COLUMN Location VARCHAR(150) NOT NULL;

ALTER TABLE [EVENT]
ALTER COLUMN Distance DECIMAL(6,2) NOT NULL;


ALTER TABLE [CATEGORY]
ALTER COLUMN EventID INT NOT NULL;

ALTER TABLE [CATEGORY]
ALTER COLUMN CategoryName VARCHAR(100) NOT NULL;

ALTER TABLE [CATEGORY]
ALTER COLUMN CategoryType VARCHAR(50) NOT NULL;

ALTER TABLE [CATEGORY]
ALTER COLUMN MinimumAge INT NOT NULL;

ALTER TABLE [CATEGORY]
ALTER COLUMN MaximumAge INT NOT NULL;

ALTER TABLE [CATEGORY]
ALTER COLUMN CategoryDistance DECIMAL(6,2) NOT NULL;

ALTER TABLE [ENROLMENT]
ALTER COLUMN ParticipantID INT NOT NULL;

ALTER TABLE [ENROLMENT]
ALTER COLUMN EventID INT NOT NULL;

ALTER TABLE [ENROLMENT]
ALTER COLUMN CategoryID INT NOT NULL;

ALTER TABLE [ENROLMENT]
ALTER COLUMN EnrolmentDate DATETIME2 NOT NULL;

ALTER TABLE [ENROLMENT]
ALTER COLUMN EnrolmentStatus VARCHAR(30) NOT NULL;

ALTER TABLE [RESULT]
ALTER COLUMN EnrolmentID INT NOT NULL;

ALTER TABLE [RESULT]
ALTER COLUMN FinishTime TIME NOT NULL;

ALTER TABLE [RESULT]
ALTER COLUMN FinishPosition INT NOT NULL;

ALTER TABLE [RESULT]
ALTER COLUMN ResultStatus VARCHAR(30) NOT NULL;


ALTER TABLE [EVENT]
ADD CONSTRAINT UQ_EVENT_Name UNIQUE (EventName);


ALTER TABLE [CATEGORY]
ADD CONSTRAINT UQ_CATEGORY_Event_Name UNIQUE (EventID, CategoryName);


ALTER TABLE [ENROLMENT]
ADD CONSTRAINT DF_ENROLMENT_Date
DEFAULT SYSDATETIME() FOR EnrolmentDate;


ALTER TABLE [ENROLMENT]
ADD CONSTRAINT DF_ENROLMENT_Status
DEFAULT 'Pending' FOR EnrolmentStatus;


ALTER TABLE [RESULT]
ADD CONSTRAINT DF_RESULT_Status
DEFAULT 'Pending' FOR ResultStatus;


ALTER TABLE [ENROLMENT]
ADD CONSTRAINT UQ_ENROLMENT_Participant_Event_Category
UNIQUE (ParticipantID, EventID, CategoryID);

--Create sample Data
INSERT INTO [USER]
    (FirstName, LastName, Email, PhoneNumber, PasswordHash, Role, ProfilePictureURL)
VALUES
    ('Selwyn', 'Mukwevho', 'thabo.mokoena@raceday.co.za', '0821234567',
     'HASH_Selwyn_2026', 'Organiser', 'https://raceday.co.za/profiles/thabo.jpg'),

    ('Mbali', 'Moyakhe', 'lerato.nkosi@raceday.co.za', '0832345678',
     'HASH_Mbali_2026', 'Organiser', 'https://raceday.co.za/profiles/lerato.jpg'),

    ('Canaan', 'Dlamini', 'sipho.dlamini@gmail.com', '0843456789',
     'HASH_Canaan_2026', 'Participant', 'https://raceday.co.za/profiles/sipho.jpg'),

    ('Orinea', 'Naidoo', 'ayesha.naidoo@gmail.com', '0854567890',
     'HASH_Orinea_2026', 'Participant', 'https://raceday.co.za/profiles/ayesha.jpg');

INSERT INTO [EVENT TYPE]
    (TypeName)
VALUES
    ('Road Race'),
    ('Trail Run'),
    ('Fun Run');

INSERT INTO [EVENT]
    (OrganiserID, EventTypeID, EventName, Description, EventDate, Location, Distance, BannerImageURL)
VALUES
    (1, 1, 'Johannesburg City Funky Race',
     'A competitive road race through Johannesburg.',
     '2026-10-18 07:00:00',
     'Johannesburg, Gauteng',
     10.00,
     'https://raceday.co.za/banners/jhb-road-race.jpg'),

    (2, 2, 'Magaliesberg Trailblaze Challenge',
     'A challenging trail running event through the Magaliesberg area.',
     '2026-11-08 06:30:00',
     'Magaliesberg, Gauteng',
     21.10,
     'https://raceday.co.za/banners/magaliesberg-trail.jpg'),

    (1, 3, 'Pretoria Community 5k Fun Run',
     'A family-friendly fun run open to participants of different ages.',
     '2026-11-22 08:00:00',
     'Pretoria, Gauteng',
     5.00,
     'https://raceday.co.za/banners/pretoria-fun-run.jpg');


     INSERT INTO [CATEGORY]
    (EventID, CategoryName, CategoryType, MinimumAge, MaximumAge, CategoryDistance)
VALUES
    (1, '10km Open', 'Open', 18, 99, 10.00),
    (1, '10km Junior', 'Junior', 14, 17, 10.00),
    (2, '21km Trail Open', 'Open', 18, 99, 21.10),
    (2, '21km Trail Junior', 'Junior', 16, 17, 21.10),
    (3, '5km Family Fun', 'Family', 10, 99, 5.00),
    (3, '5km Junior Fun', 'Junior', 10, 17, 5.00);


INSERT INTO [ENROLMENT]
    (ParticipantID, EventID, CategoryID, EnrolmentDate, EnrolmentStatus)
VALUES
    (3, 1, 1, '2026-09-01 10:15:00', 'Confirmed'),
    (4, 1, 1, '2026-09-01 11:30:00', 'Confirmed'),
    (3, 2, 3, '2026-09-02 09:20:00', 'Confirmed'),
    (4, 3, 5, '2026-09-02 14:45:00', 'Pending');


INSERT INTO [RESULT]
    (EnrolmentID, FinishTime, FinishPosition, ResultStatus)
VALUES
    (1, '01:02:35', 14, 'Completed'),
    (2, '01:08:42', 27, 'Completed'),
    (3, '02:31:18', 19, 'Completed');


SELECT * FROM [USER];

SELECT * FROM [EVENT TYPE];

SELECT * FROM [EVENT];

SELECT * FROM [CATEGORY];

SELECT * FROM [ENROLMENT];

SELECT * FROM [RESULT];

--Demonstrate execution and understanding of joining
SELECT
    U.FirstName + ' ' + U.LastName AS Participant,
    E.EventName,
    C.CategoryName,
    EN.EnrolmentStatus
FROM [ENROLMENT] EN
INNER JOIN [USER] U
    ON EN.ParticipantID = U.UserID
INNER JOIN [EVENT] E
    ON EN.EventID = E.EventID
INNER JOIN [CATEGORY] C
    ON EN.CategoryID = C.CategoryID;