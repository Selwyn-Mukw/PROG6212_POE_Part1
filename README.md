# PROG6212_POE_Part1

RaceDay event management system
## Project Description
RaceDay is an event management system designed to help organiser's manage race events and allow participants to interact with and participate in those events.

The system is intended to provide a structured way of managing event
information, participants and the underlying database.

User Roles
## Organiser
The Organiser is responsible for managing racing events. Organiser functionality includes creating and managing event information and maintaining the information required for participants.

## Participant
The Participant interacts with racing events and can access relevant event information and participate in available events.

## Part 1
Part 1 contains the initial system design and database documentation.(All the planning and preparation is done here.

### ERD
The Entity Relationship Diagram (ERD) shows the database entities and the relationships between them.
The ERD is available in:
`docs/RaceDay_ERD.pdf`
### API Endpoint Plan
The API Endpoint Plan documents the planned API endpoints and their functionality.
The API Endpoint Plan is available in:
`docs/RaceDay_API_Endpoint_Plan.pdf`

### SQL Database Script
The SQL database script contains the database creation and setup statements for the RaceDay system.
The SQL script is available in:
`docs/RaceDay_Database.sql`

## Repository Structure


PROG6212_POE_Part1/
│
├── .github/
│   └── workflows/
│       └── part1-ci.yml
│
├── docs/
│   ├── RaceDay_ERD.pdf
│   ├── RaceDay_API_Endpoint_Plan.pdf
│   ├── RaceDay_Database.sql
│   └── API Endpoint plan.docx
│
└── README.md
'
## Database Setup

The RaceDay database can be created using Microsoft SQL Server Management Studio (SSMS).

1. Open SQL Server Management Studio.
2. Open the `RaceDay_Database.sql` script.
3. Execute the script.
4. The script will create the required database objects and data for the
   RaceDay system.
   
## Youtube link
https://youtu.be/HcojE-xmNg8
## CI/CD
GitHub Actions is used to automatically check that the required Part 1 files are present in the repository.

The workflow checks that:

- The `docs` folder exists.
- The RaceDay ERD PDF exists.
- The RaceDay API Endpoint Plan PDF exists.
- The RaceDay SQL database script exists.
- The `README.md` file exists.
below is an image of the CI Flow 
The workflow runs automatically when changes are pushed to the repository.
<img width="1906" height="816" alt="Screenshot 2026-09-03 212731" src="https://github.com/user-attachments/assets/8a6edb1c-aa0e-414f-b028-2a00d39c6649" />

Part2 
RaceDay is an ASP.NET Core Web API project developed for the PROG6212
Programming 2B Portfolio of Evidence (PoE), Part 2. It provides API
functionality for user authentication, events, categories, enrolments
and results.

Technology Stack

-ASP.NET Core Web API
-C# and .NET 8
-SQL Server
-Entity Framework Core
-JWT bearer authentication
-xUnit automated tests
-Swagger / OpenAPI
-Solution Structure

The solution contains four projects:

-RaceDay.Api --- API application, controllers, data context,
models and migrations.
-RaceDay.contracts --- DTOs used to define API request and
response contracts.
-RaceDay.API.Tests --- automated tests, including DTO validation
and authentication/authorization integration tests.
-RaceDay.PasswordTool --- password-related utility project.

Database Setup

The application is configured to use a SQL Server database named
RaceDay. The development connection used for this project has been
Jivo with Windows authentication.

1.Open the solution in Visual Studio.
2.Open RaceDay.Api/appsettings.json.
3.Check the ConnectionStrings section and update RaceDayConnection
to match your SQL Server instance and environment.
4.Ensure the RaceDay database and its required tables are present
before running database-dependent endpoints.
5.The project contains EF Core migrations. Review the migrations and
the current database schema before applying migrations in a new
environment.

Running the API
1.Clone the repository and open RaceDay/RaceDay.slnx in Visual
Studio.
2.Confirm the connection string and JWT configuration are set for your
local environment.
3.Set RaceDay.Api as the startup project.
4.Build the solution.
5.Run the API using the configured HTTPS launch profile.
6.Open the Swagger URL shown in the application output or browser
launch.

Swagger provides an interactive way to inspect the available endpoints
and send requests. Some endpoints require a valid JWT bearer token.


Controller                          Purpose
AuthController                    Registration and login

ProfileController                 Authenticated user's profile

EventsController                  Event operations

CategoriesController              Event category operations

EnrolmentsController              Participant enrolments and
organiser enrolment management

ResultsController                 Race results

For the authoritative route templates and request DTOs, inspect the
controller attributes and DTOs in the repository and use Swagger at
runtime. Endpoint availability and access requirements should be
verified against the Part 1 endpoint plan and Part 2 assignment guide.

Automated Tests

The test project is RaceDay.API.Tests.

Run all tests from the solution directory:

dotnet test
The latest local test run confirmed during development reported 25
passed, 0 failed, 0 skipped.

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/47e56fa6-4525-4c6c-af7a-e7e8c386bc0e" />

