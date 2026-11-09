# PROG6212_POE_Part1,2 & 3

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

The RaceDay API uses Microsoft SQL Server and a database named RaceDay.

Open the solution in Visual Studio.
Open RaceDay.Api/appsettings.json.
Check the RaceDayConnection connection string and update it to match your local SQL Server instance.
Ensure that the RaceDay database is available.
Check that the JWT configuration contains the required Key, Issuer, and Audience settings. Use suitable local development values and do not commit real secrets.
Restore the NuGet packages and build the solution.
Run the API project and confirm that it starts successfully

The solution contains four projects:

-RaceDay.Api --- API application, controllers, data context,
models and migrations.
-RaceDay.contracts --- DTOs used to define API request and
response contracts.
-RaceDay.API.Tests --- automated tests, including DTO validation
and authentication/authorization integration tests.
-RaceDay.PasswordTool --- password-related utility project.

User Roles and Permissions
Organiser
The Organiser is responsible for managing race events and related information.

Organiser functionality includes:
Creating race events.
Updating and deleting events they are authorised to manage.
Managing event categories.
Managing enrolment information where authorised.
Recording and updating race results.
Accessing endpoints restricted to the Organiser role.

Organiser-only endpoints require appropriate authentication and authorisation.

Participant
The Participant interacts with race events and accesses information relevant to their participation.
Participant functionality includes:
Viewing available events and event categories.
Accessing participant enrolment functionality.
Viewing race results where permitted.
Accessing endpoints restricted to the Participant role.

Participant permissions are enforced through authentication and role-based authorisation where configured.

Important: Your actual controller attributes determine which endpoints each role can access. Check them before claiming that a particular operation is available to a role.

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

Youtube link : https://youtu.be/I0hsUAs0uP8

PART 3 – ASP.NET CORE MVC APPLICATION

11. Part 3 Overview

Part 3 extends the RaceDay system by introducing an ASP.NET Core MVC application.

The MVC application communicates with the RaceDay API and provides separate interfaces for Organisers and Participants.

The application uses session management to maintain authenticated user information and JWT tokens when communicating with the API.

12. Organiser Interface

The Organiser dashboard provides access to event administration functionality.

Implemented functionality includes:

Organiser dashboard.

Event creation and management.

Event type selection.

Event category management.

Participant enrolment management.

Race result management.

Event banner uploads.

Organisers can capture finishing times and positions for enrolled participants.

13. Participant Interface

The Participant dashboard provides access to race participation functionality.

Implemented functionality includes:

Participant dashboard.

Browse upcoming events.

Search events by name and location.

View event details.

View available race categories.

Enrol in events.

View personal enrolments.

Cancel enrolments.

View personal race results.

Update profile information.

Upload profile pictures.

Participants can view their recorded finishing times and positions once results have been captured by an Organiser.

14. Azure Blob Storage Integration

Azure Blob Storage is used to store image files uploaded through the RaceDay application.

The integration supports:

Participant profile pictures.

Event banner images.

The upload process follows this architecture:

The user selects an image through the MVC interface.

The MVC application sends the image to the RaceDay API.

The API processes the uploaded file.

The API uploads the image to Azure Blob Storage.

The associated image reference is stored in the database.

The MVC application retrieves and displays the image.

The MVC application communicates with Azure through the API rather than directly accessing Azure Blob Storage.

Azure connection strings and other sensitive configuration values must be stored securely using local development secrets or suitable environment configuration.

They must not be committed to GitHub.

PART 4 – DOCKER CONTAINERISATION

15. Docker Overview

Docker is used to containerise the RaceDay MVC application.

The application is built into a Docker image and executed inside a Docker container.

The MVC application communicates with the RaceDay API running on the host machine during the local Docker demonstration.

Docker Configuration

The project contains a Dockerfile located at:

RaceDay.MVC/Dockerfile

The Docker image is named:

raceday-mvc:part3

The MVC application runs inside the container on port 8080.

The API runs on the host machine on port 5000.

16. Running RaceDay with Docker

Prerequisites

Before running the application, ensure that the following are installed:

Docker Desktop.

.NET 8 SDK.

Microsoft SQL Server.

Visual Studio.

Step 1 – Start the RaceDay API

Open PowerShell in the RaceDay solution directory.

Run:

dotnet run --project RaceDay.Api --urls http://0.0.0.0:5000

Keep the API running while using the Docker container.

Step 2 – Build the Docker Image

Open another PowerShell window in the RaceDay solution directory.

Run:

docker build -f RaceDay.MVC/Dockerfile -t raceday-mvc:part3 .

This builds the MVC application into a Docker image.

Step 3 – Start the Docker Container

Run:

docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc:part3

The environment variable configures the MVC application to communicate with the API running on the Windows host.

Step 4 – Verify the Container

Run:

docker ps

The RaceDay MVC container should appear with an Up status.

To view the container logs:

docker logs raceday-mvc

Step 5 – Access the Application

Open a browser and navigate to:

http://localhost:8080

The RaceDay login page should appear.

Users can log in using their Organiser or Participant accounts.

Docker Management Commands

Stop the container:

docker stop raceday-mvc

Remove the container:

docker rm raceday-mvc

View available images:

docker images

View running containers:

docker ps

PART 5 – CI/CD AND GITHUB

17. GitHub Actions

GitHub Actions is used for automated repository checks.

The existing Part 1 workflow is located at:

.github/workflows/part1-ci.yml

The workflow checks that the required Part 1 documentation files are present in the repository.

The workflow runs automatically when changes are pushed to GitHub, according to its configured triggers.

Additional build and test automation should be verified against the final workflow configuration.

18. Version Control

Git and GitHub are used to manage the development of the RaceDay application.

The repository contains the project source code, database documentation, API implementation, MVC application, automated tests and Docker configuration.

Meaningful commits are used to track development progress and changes.

PART 6 – PROJECT VIDEOS AND EVIDENCE

19. Video Demonstrations
    https://youtu.be/gMWKY2o-7w8

20. CI/CD Evidence
    <img width="1880" height="877" alt="image" src="https://github.com/user-attachments/assets/0ea83279-7ced-4eef-86e8-353bf0a56ff7" />
    <img width="1881" height="727" alt="Screenshot 2026-11-09 132436" src="https://github.com/user-attachments/assets/3f8d2b0d-4fb3-47cc-ac11-bc5fb9637b90" />
    <img width="1865" height="772" alt="image" src="https://github.com/user-attachments/assets/10f31d90-2e8f-4444-b505-2f29c56d5ea0" />





