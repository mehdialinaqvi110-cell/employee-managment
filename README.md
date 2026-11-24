# Employee Management System

A full-stack Employee Management System built using **Vue.js 3, C#, ASP.NET Core Web API, Entity Framework Core, and MySQL**. The application provides a dashboard to manage employee records, search and filter employees, and generate department-wise reports.

## Features

- **Dashboard:** View employee statistics and key information.
- **Employee Management:** Add, view, update, and delete employee records.
- **Search:** Find employees using the available search functionality.
- **Department Filter:** Filter employee records by department.
- **Pagination:** Navigate through employee records efficiently.
- **Department-wise Reports:** View employee information organized by department.
- **Form Validation:** Validate employee details before saving.
- **Email Validation:** Prevent duplicate email addresses and validate employee data.
- **Confirmation Modal:** Confirm before deleting an employee.
- **Success and Error Messages:** Display feedback for user actions.

## Technology Stack

| Layer | Technologies |
|---|---|
| Frontend | Vue.js 3, JavaScript, HTML, CSS |
| Backend | C#, ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | MySQL |
| Database Migrations | EF Core Migrations |
| Version Control | Git and GitHub |

## Project Structure

```text
employee-managment/
├── EmployeeManagement.Api/
│   ├── Controllers/
│   │   └── EmployeeController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/
│   ├── Models/
│   │   └── Employee.cs
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── EmployeeManagement.Api.csproj
│
├── employee-frontend/
│   ├── public/
│   ├── src/
│   │   ├── assets/
│   │   ├── components/
│   │   ├── services/
│   │   ├── App.vue
│   │   └── main.js
│   ├── package.json
│   └── vite.config.js
│
└── .gitignore
```

## Prerequisites

Install the following before running the project:

- Node.js and npm
- .NET SDK compatible with the backend project
- MySQL Server
- Git (optional, for cloning the repository)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/mehdialinaqvi110-cell/employee-managment.git
cd employee-managment
```

### 2. Configure the MySQL database

Create the database in MySQL:

```sql
CREATE DATABASE employee_management;
```

Configure the database connection string in the backend's local development configuration.

For local development, create or update:

`EmployeeManagement.Api/appsettings.Development.json`

Use the connection string name `DefaultConnection` and supply your own MySQL credentials.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=employee_management;User=root;Password=YOUR_LOCAL_PASSWORD;"
  }
}
```

Replace `YOUR_LOCAL_PASSWORD` with your local MySQL password. **Never commit real credentials to GitHub.**

### 3. Run the backend

Open a terminal and navigate to the backend directory:

```bash
cd EmployeeManagement.Api
dotnet restore
dotnet run
```

The API runs at the URL configured by the ASP.NET Core launch profile. The development URL used during this project was:

```text
http://localhost:5164
```

The employee API endpoint used by the frontend is:

```text
http://localhost:5164/api/Employee
```

If your application starts on a different port, update the frontend API configuration accordingly.

### 4. Run the frontend

Open a second terminal from the repository root:

```bash
cd employee-frontend
npm install
npm run dev
```

Open the local URL printed by Vite. During development, the frontend was available at:

```text
http://localhost:5173
```

Ensure the backend is running and the frontend API configuration points to the correct backend URL.

## Employee Data

The application manages employee records with fields including:

- Full Name
- Email
- Department
- Salary

The backend handles employee operations and communicates with MySQL through Entity Framework Core.

## API

The employee API is available at:

```text
/api/Employee
```

It supports the CRUD operations implemented by the backend:

- **GET** — Retrieve employee records.
- **POST** — Create an employee.
- **PUT** — Update an employee.
- **DELETE** — Delete an employee.

Refer to `EmployeeController.cs` for the implemented routes and request formats.

## Future Enhancements

- User authentication and role-based access control.
- Employee profile images and additional employee details.
- Export reports to Excel or PDF.
- Dashboard charts and advanced analytics.
- Automated unit and integration testing.
- Deployment to a cloud hosting platform.

## Author

**Mehdi Ali**

GitHub: [mehdialinaqvi110-cell](https://github.com/mehdialinaqvi110-cell)

## License

This project is available for educational and portfolio purposes. Add a license file if you intend to distribute it under specific terms.
