# Library Management System

Console based Library Management System with 2 roles (Member, Admin)

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

# Output
---
# Auth Menu
![Auth Menu](/Assets/AuthMenu.png)
* On executing this project, the auth menu is loaded initially.
* After login, the member details are stored in **Global Session Manager**, to use it in further menus
* Auth Menu has:


**1. Register (by default it is member)**
![Register](/Assets/Register.png)
Member Register.


**2. Login**
 (Role: Member or Admin is automatically identified wrt Role property in the member model)
![Auth Menu](/Assets/AuthMenu.png)

* On successful login, the user is directed to Member Menu (if Role is member) or Admin Menu (if role is Admin).
---

# MEMBER MENU
Menu only for users with Role : Member
![Member Menu](/Assets/image-1.png)

---
# A. Dashboard Reports:
**Dashboard Functions are executed using Stored Procedures**
**A.1 Most Borrowed Book**
![MostBorrowedBook](/Assets/MostBorrowedBooks.png)
This features highly borrowed book with respect to borrow count (like popular books).

**A.2 OverDue Books**
![OverDueBooks](/Assets/OverDueBooks.png)
This result is with respect to the member. It shows the list of books that is borrowed by the member -> the due date has passed -> member is yet to return it.

**A.3 Pending Fines**
![PendingFines](/Assets/Register.png)
Total Pending Fine of the member is shown. For more details, member can check the Fine Management section.

---

# B. Profile Management:
**B.1 = View Profile**
![Profile](/Assets/ProfileManagement.png)
This shows the Profile details of the logged in member.

**B.2 = Update Profile**
![UpdateProfile](/Assets/UpdateProfile.png)
Members can update their profile and Membership Status with basic static validations.

**B.3 DeActivate Profile:**

if given yes, account will be deactivated (isActvie = false)
---

# C. Book Management:
**C.1 View All Available books**
![ViewAvailableBooks](/Assets/AvlBooks.png)
Members can view all the books whose Status is Available (not borrowed or lost)

**C.2 Search by Title**
![SearchByTitle](/Assets/SearchByTitle.png)
Get book details with its Title

**C.3 Search By Author**
![SearchByAuthor](/Assets/SearchByAuthor.png)
Get all books of an author

---

# D.Borrow / Return Books
**D.1 Borrow New Book**
![Borrow](/Assets/Borrow.png)
Member can borrow book by entering the book name.
* Member validated
* Borrowing Limit Checked
* Fine Limit Checked
* Book Validated
* Any Book Copy of the book, Status == Available checked
* Duplicate Borrowing of same book checked (if user has already borrowed different copy of the book)
* Borrowing done

**D.2 Return Book**
![ReturnBook](/Assets/Return.png)
* Book Title got from member and book validated
* check if a borrow exisits with memberID and any copy of the book (bookCopyId)
* Check if borrow status is ACTIVE
* Return done (Change status of borrow to RETURN)
* **Note** this request will be added to **Admin**, only when they approve the status of the book, Book Copy will be made **AVAILABLE**. This is done to verify **DAMAGE or LOST**

**D.3 Borrow History**
![BorrowHisotry](/Assets/BorrowHistory.png)
Members can view their full borrow history

---

# E. Fine Management
E.1 View Fine History
![FineHistory](/Assets/FineHistory.png)
Members can view their full Fine History
E.2 Pay Fine
![PayFine](/Assets/PayFine.png)
Members can pay fine with respect to FineID. 
**Future Scope**: Adding transactional checks 

---

# F. Damage Logs
F.1 View Damage Log
![DamageLog](/Assets/DamageLog.png)
* Members can view Damage Log History with the description added by the admin.
* This is created if the Admin marks the book copy status as **DAMAGED or LOST** at the **APPROVE RETURN** step.

---
# ADMIN MEU
It consitis the operations perfomed by admin
**Admin Menu**
![AdminMenu](/Assets/AdminMenu.png)
If the Role  == "Admin", then automatically the user will be navigated to the Admin Menu

---

# A. DashBoard Reports
**A.1 Overal Borrowed Books**
![Borrowhistory](/Assets/OverallBorrowHistory.png)
This is overall Borrow History (irrespective of the member)

**A.2 OverDue Books:**
![OverDue](/Assets/OverallOverDue.png)
It shows the Borrow details of overdue books

**A.3 Members With Pending Fines**
![Fines](/Assets/OverallPendingFine.png)
It shows the Member Details ALong with fines

**A.4 Most Borrowed Books**
This shows popular books just like how it worked in the member side
---

# B. Member Management
**B.1 View All Members:**
![AllMembers](/Assets/OverallMember.png)
Member Details can be viewed but Member Password is not shown

**B.2 Search Member by Id**
![Search](/Assets/SearchMember1.png)
Similary B.3 Search my Name and B.4 Email works

**B.5 Deactivate Member**
![Deactivate](/Assets/MemberDeactivate.png)
The admin can Deactivate a member. (This is not hard delete. This is Soft Delete) The Member has a property isActive which is set to false of deactivation

# C. Book Management
**C.1 Add Book**
![AddBook](/Assets/AddBook.png)
Primary Key BookID is auto - generated as it is of type SERIAL

**C.2 Update Book**
![UpdateBook](/Assets/UpdateBook.png)
The Admin can update the essential details of the book with Book ID

**C.3 View All Books**
This is similary to View All Books in Member Menu

**C.4 Search by Title**
This is similary to View All Books in Member Menu

**C.5 Search by Author**
This is similary to View All Books in Member Menu

**C.6 Search by Category**
This is similary to View All Books in Member Menu

**C.7 Add Book copy**
![AddBookCpy](/Assets/AddBookCopy.png)
* Book Validation
* Book Copy added

---

# D. Borrow Management
**D.1 View Active Borrows**
![ActiveBorrows](/Assets/ActiveBorrow.png)
Books that are currently in Borrow status

**D.2 Approve Return**
![ApproveReturn](/Assets/ApproveReturn.png)
After the member returns the book, admin can verify it.

**case 1:** If book is damaged or lost
* Admin Marks the borrow status to **DAMAGE or LOST**
* Book Copy become **DAMAGE or UNAVAILABLE**
* Added to DamageLog with description

**case2:** No damage
* borrow status remains as **RETURNED**
* Book Copy status becomes **AVAILABLE**

**D.3 View Borrow History by Member**
![BorrowHisotry](/Assets/BorrowHistoryByMember.png)

---

# E. Fine Management
**E.1 Overall Active Fines**
![Fines](/Assets/OverallActiveFines.png)
Get the active Fine details (unpaid) of all members

**E.2 Fine History Of Member**
![FineHistory](/Assets/FineHistoryByMember.png)
Get the Fine History (member specific) both paid and unpaid

# F. DamageLog
**F.1 View Overall Damage Log**
![DamageLog](/Assets/OverallDamageLog.png)

**F.2 Damalog of a Member**
![FilterByMember](/Assets/DamageLogByMember.png)

**F.3 DamageLog of a Book Copy**
![FilterByCopy](/Assets/DamageLogOfBookCpy.png)





