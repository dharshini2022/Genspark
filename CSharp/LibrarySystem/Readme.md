**LIBRARY MANAGEMENT SYSTEM**
**To Run**
Move to UI folder and execute
cd LibrarySystem.UI
dotnet run

**Auth Menu**
On executing this project, the auth menu is loaded initially.
![Auth Menu](image.png)
After login, the member details are stored in global Session Manager, to use it in further menus
Auth Menu has:
1) Register (by default it is member)
![Register](image-20.png)
Member Register.
2) Login (Role: Member or Admin is automatically identified wrt Role property in the member model)
![Auth Menu](image.png)

On successful login, the user is directed to Member Menu (if Role is member) or Admin Menu (if role is Admin).

**MEMBER MENU**
Menu only for users with Role : Member
![Member Menu](image-1.png)

**A. Dashboard Reports:**
**Dashboard Functions are executed using Stored Procedures**
A.1 Most Borrowed Book
![MostBorrowedBook](image-1.png)
This features highly borrowed book with respect to borrow count (like popular books).
A.2 OverDue Books
![OverDueBooks](image-2.png)
This result is with respect to the member. It shows the list of books that is borrowed by the member -> the due date has passed -> member is yet to return it.
A.3 Pending Fines
![PendingFines](image-19.png)
Total Pending Fine of the member is shown. For more details, member can check the Fine Management section.

**B. Profile Management:**
B.1 = View Profile
![Profile](image-3.png)
This shows the Profile details of the logged in member.
B.2 = Update Profile
![UpdateProfile](image-4.png)
Members can update their profile and Membership Status with basic static validations.
B.3 DeActivate Profile:

if given yes, account will be deactivated (isActvie = false)

**C. Book Management:**
C.1 View All Available books 
![ViewAvailableBooks](image-5.png)
Members can view all the books whose Status is Available (not borrowed or lost)
C.2 Search by Title
![SearchByTitle](image-6.png)
Get book details with its Title
C.3 Search By Author
![SearchByAuthor](image-7.png)
Get all books of an author

**D.Borrow / Return Books**
D.1 Borrow New Book
![Borrow](image-8.png)
Member can borrow book by entering the book name.
* Member validated
* Borrowing Limit Checked
* Fine Limit Checked
* Book Validated
* Any Book Copy of the book, Status == Available checked
* Duplicate Borrowing of same book checked (if user has already borrowed different copy of the book)
* Borrowing done
D.2 Return Book
![ReturnBook](image-9.png)
* Book Title got from member and book validated
* check if a borrow exisits with memberID and any copy of the book (bookCopyId)
* Check if borrow status is ACTIVE
* Return done (Change status of borrow to RETURN)
* **Note** this request will be added to **Admin**, only when they approve the status of the book, Book Copy will be made **AVAILABLE**. This is done to verify **DAMAGE or LOST**
D.3 Borrow History
![BorrowHisotry](image-11.png)
Members can view their full borrow history

**E. Fine Management**
E.1 View Fine History
![FineHistory](image-14.png)
Members can view their full Fine History
E.2 Pay Fine
![PayFine](image-12.png)
Members can pay fine with respect to FineID. 
**Future Scope**: Adding transactional checks 

**F. Damage Logs**
F.1 View Damage Log
![DamageLog](image-13.png)
* Members can view Damage Log History with the description added by the admin.
* This is created if the Admin marks the book copy status as **DAMAGED or LOST** at the **APPROVE RETURN** step.

**ADMIN MEU**
It consitis the operations perfomed by admin
**Admin Menu**
![AdminMenu](image-10.png)
If the Role  == "Admin", then automatically the user will be navigated to the Admin Menu

**A. DashBoard Reports**
A.1 Overal Borrowed Books
![Borrowhistory](image-15.png)
This is overall Borrow History (irrespective of the member)

A.2 OverDue Books:
![OverDue](image-17.png)
It shows the Borrow details of overdue books

A.3 Members With Pending Fines
![Fines](image-16.png)
It shows the Member Details ALong with fines

A.4 Most Borrowed Books
This shows popular books just like how it worked in the member side

**B. Member Management**
B.1 View All Members:
![AllMembers](image-18.png)
Member Details can be viewed but Member Password is not shown
B.2 Search Member by Id
![Search](image-21.png)
Similary B.3 Search my Name and B.4 Email works
B.5 Deactivate Member
![Deactivate](image-22.png)
The admin can Deactivate a member. (This is not hard delete. This is Soft Delete) The Member has a property isActive which is set to false of deactivation

**C. Book Management**
C.1 Add Book
![AddBook](image-23.png)
Primary Key BookID is auto - generated as it is of type SERIAL

C.2 Update Book
![UpdateBook](image-24.png)
The Admin can update the essential details of the book with Book ID

C.3 View All Books
This is similary to View All Books in Member Menu

C.4 Search by Title
This is similary to View All Books in Member Menu

C.5 Search by Author
This is similary to View All Books in Member Menu

C.6 Search by Category
This is similary to View All Books in Member Menu

C.7 Add Book copy
![AddBookCpy](image-25.png)
* Book Validation
* Book Copy added

**D. Borrow Management**
D.1 View Active Borrows
![ActiveBorrows](image-26.png)
Books that are currently in Borrow status

D.2 Approve Return
![ApproveReturn](image-28.png)
After the member returns the book, admin can verify it.
**case 1:** If book is damaged or lost
* Admin Marks the borrow status to **DAMAGE or LOST**
* Book Copy become **DAMAGE or UNAVAILABLE**
* Added to DamageLog with description

**case2:** No damage
* borrow status remains as **RETURNED**
* Book Copy status becomes **AVAILABLE**

D.3 View Borrow History by Member
![BorrowHisotry](image-27.png)

**E. Fine Management**
E.1 Overall Active Fines
![Fines](image-29.png)
Get the active Fine details (unpaid) of all members

E.2 Fine History Of Member
![FineHistory](image-30.png)
Get the Fine History (member specific) both paid and unpaid

**F. DamageLog**
F.1 View Overall Damage Log
![DamageLog](image-31.png)

F.2 Damalog of a Member
![FilterByMember](image-32.png)

F.3 DamageLog of a Book Copy
![FilterByCopy](image-33.png)





