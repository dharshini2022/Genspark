# LIBRARY MANAGEMENT SYSTEM : Requirements Analysis
---
# Architecture
- 3T Architecture
![3T](/Assets/3T.png)
- Repository Pattern
```
DB -> Models / DTOs -> Reops (DB interaction) -> Services
```
- Dependency Inversion : Interface based designs
```
Interface -> Service / Interface -> Repository (this creation an abstraction)
```
- Scoped based injection
```
var services = new ServiceCollection();

services.AddScoped<LibraryDbContext>();

services.AddScoped<IMemberRepository, MemberRepository>();
services.AddScoped<IBorrowingRepository, BorrowingRepository>();

services.AddScoped<IMemberService, MemberService>();
services.AddScoped<IBookService, BookService>();

 services.AddScoped<MemberMenu>();
 services.AddScoped<AdminMenu>();
services.AddScoped<AuthMenu>();

services.AddScoped<MemberModules.DashboardManagement>();
services.AddScoped<MemberModules.ProfileManagement>();
```
---
# Tech Stack:
- Backend : .NET
- DB : Postgresql
- Connectivity : EFCore
---

# C# Concepts:
- Classes and objects
- Interfaces based design
- Dependency Injection (Constructor injections)
- LINQ (Where(), SingleOrDefault() , Any(), Select(), Count(), GroupBy(), OrderBy())
---
# Database Concepts:
- Code First Approach (Models ->DbContext -> Tables)
- Primary key and Foreign Key Constraints
- Transactions for related operations like borrow / return
- Stored procedures for Joins based operations
- EFCore and FluentAPI 

# Separation of Concerns:
1. UI Layer : Main execution
- Program.cs -> Auth Menu
- On sucessful long -> Member Menu. or Admin Menu (based on role)
- On Selection of an option like (Book Management) -> Module (for Service Selection)

2. BLL (Business Layer Logic)
- Interface -> Service

3. DAL (Data Access Layer)
- IRepo -> AbstractRepository (for basic CRUD)
- Interface -> Repository (for model tailored operations)

4. Models 
- Models / DTOs based on operations
---

# ENTITIES:
- Member : manage members
- Books : mange books
- BookCopies: manage individula copies of book (copyId)
- BookCategories: for unique categories (as per normalisation)
- Borrow : handle book borrow and return
- Fine : handle fine collected for late submission / damages
- DamageLog : handle book damages and lost log
---
# DTOs (For Joins based result)
- MemberPendingFines
- MostBorrowedBook
- OverallBorrowHisotry
- OverallDueBook
---
# ROLES:
- User 
- Admin
---
# Functionality:
# USER
1. **Dashboard:**
- View Most Borrowed Books : overall just like popular books
- Overdue Books : books borrowed by user that has crossed the due date for submission
- Pending Fines : Fines not yet paid by the user

2. **Profile Management**
- Register
- Log In
- Update : password, contact details (update membership status)
- Deactivate account
- View Profile

3. **Book Management:**
- View Available books
- Search by Title
- Search by Author
- Filter by Category

4. **Borrow / Return**
- Borrow new book
- return book
- View Borrow History

5. **Fine Management**
- View Fine History
- Pay Fine

6. **Damage Log**
- View Damage Log 
---

# ADMIN
1. **Dashboard Management**
- Overall Borrowed Books (Active borrows)
- OVerall Over due books
- Members with Pending Fines

2. **Member Management**
- Add new user
- Deactive a user
- Search By Id
- Search By Email
- Search by name

3. **Book Management**
- Add new book
- Update Book
- Search Book by title
- Search book by author
- Search book by category
- Add new book copy

4. **Borrow / Return**
- View Active borrow
- Approve Return
- Borrow History of a Member

5. **Fine Management**
- View Overall Active Fines
- View Fine History of member

6. **Damage Log Mangement:**
- View All Damage Log
- Filter Damage Log by Member
- Filter Damage Log of a book copy

---
# Borrowing Rules: (Executed using Transaction in BorrowingService)
- GetMemberActive
- GetMemberStatus
- CheckBorrowLimit
- CheckTotalFine
- CheckBorrowingStatus of same book by user
---
# Return Rules: (Executed using Transaction in BorrowingService)
- GetReturnDate
- CalculateLateFine
- Make Book Copy Available
- Save Return date in BorrowingStatus
- Make the status of the copy and Book Available
---
# RELATIONSHIP:
1. BookCategory to Book => One to Many 
2. Member to Fine => One to Many
3. Book to BookCopies => One to Many
4. Member to Borrowing => One to Many 
5. BookCopies to Borrowing => One to Many 
6. Borrow to Fine => One to One 
7. Book Copies to Damage Log => one to many
8. member to Damage Log => one to many
---
# Borrow Relationship Order:
Book -> BookCopies -> Borrowing -> Member
one to many -> one to many -> many to 1
---
# ER Diagram
![ER Diagram](/Submissions/ER%20Diagram.jpeg)

