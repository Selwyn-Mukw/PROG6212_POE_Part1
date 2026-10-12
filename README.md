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
