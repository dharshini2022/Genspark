# Library Management System

Console based Library Management System with 2 roles (Member, Admin)

---
# Submissions Scripts are inside the Submission Folder
---

# Tech Stack

* Backend : .NET Framework
* Architecture : 3T Architecture
* Pattern : Repository Pattern
* Database : Postgresql
* Connectivity : EFCore Fluent API

---

# To Run:

* cd LibrarySystem.UI
* dotnet restore 
* dotnet build
* dotnet run

---
# Work Flow

This project follows Clean Architecture with:

* UI (Program.cs -> Menu -> Module)
* BLL (Interface -> Service)
* DAL (Interface -> Repository)
* Models (DTOs / Models)
* DbContext
* Database

---

# Project Structure

```bash
LibrarySystem/
│
├── UI/                 # Execution 
│   ├── Program.cs      #Execution starts here
│   ├── Menu/           #Role based Feature options display
│   ├── Modules/        #Module based functionality Menu
│   ├── Session/        #Session Manager to store user details globally once logged in (like token)
│
│
├── Models/           #DB tables as Models
│   ├── DTOs/         #Model objects for table joins based input / output
│   ├── Book.cs
│   ├── BookCategory.cs
│   ├── BookCopy.cs
│   ├── Borrowing.cs
│   ├── DamageLog.cs
│   ├── Fine.cs
│   ├── Member.cs
│
├── DAL/            #Data Access Layer (Repos + DbContext)
│   └── Context/    #DbContext to connect with the DB (DB created using Code First Approach)
│   └── Data/       #Seed Data to add sample data to the project
│   └── Migrations/ #Model, DbContext to DB migration files (Table creation)
│   └── Interfaces/     #Interfaces of Repositories (To create Dependency Inversion)
│   └── Repositories/   #Model to DB interaction methods (CRUD)
│
├── BLL/            #Business Logic Layer
│   └── Intergaces/    #Interfaces of Services (Dependency Inversion)
│   └── Services/       #Business Logic of functionalities
│   
├── Exceptions/     #Custom Exceptions Created as per this project
│
├── README.md
└── .gitignore
```

---
