# Project Title

Library System with Book and Member CRUD operations

## Features

- Add and Get Book
- Add and Get Members
- Get all Books and Members
(As mentioned in the Assignment)

## Concepts Used
- **DTOs**
* RequestDTO : class that has only properties that require input at runtime
![RequestDTO](assests/RequestDTO.png)
* ResponseDTO : class which contains the output properties to be displayed as result
(This is created so sensitive info like password will not be displayed)
![ResponseDTO](assests/ResponseDTO.png)

- **Dependency Injection and Dependency Inversion**
* The dependent objects are injected with Controller based injection creating loose coupling.
* The Controller is depent on Interfaces and not directly on Concrete Classes.
![Dependency Injection](assests/DI.png)

- **Abstracting the Connection String by adding it to appsettings.json**
![ConnectionString](assests/ConnectionString.png)

- **Builder Creational Pattern**
* Created the WebApplication wrt to Builder Pattern.
* All components are added to it.
* On Execution, it is build to one unit.
![Builder](assests/Builder.png)

- **Scopped Injections**
* Added Components to the Builder with **scoped** type injection (connection created and terminated per each request).
![Scopped](assests/Scopped.png)

- **Region**
* Created Logical Separation 
![Region](assests/Region.png)

- **Swagger API**
* Used Swagger API for output testing
![Swagger](assests/Swagger.png)

## Swagger Display
![SwaggerDisplay](assests/SwaggerDisplay.png)

## Output Screenshots
- **GetBooks()**
* This api retrieves all the Books with Status code 200 (OK)
![GetBooks](assests/GetBooks.png)

- **GetBook(int bookId)**
* Retrieves Book by bookId, if Found -> 200(OK)
![GetBook](assests/GetBook.png)
*If not Found -> 404(Not Found)
![404Book](assests/404Book.png)

- **PostBook(BookRequestDTO book)**
* On successful entry of valid Data -> 201(Created)
![PostBook](assests/PostBook.png)
* Title Validation -> IF empty -> 400(Bad Request)
![400Book](assests/400Book.png)
* Author Validation -> If Empty -> 400 (Bad Request)
![400BookAuthor](assests/400BookAuthor.png)
* Available Copies < 0 -> 400 (Bad Request)
![400BookCopy](assests/400BookCopy.png)

- **GetMembers**
![GetMembers](assests/GetMembers.png)

-**GetMember(int memberId)**
* Retrieves Member by memberId, if Found -> 200(OK)
![GetMember](assests/GetMember.png)
*If not Found -> 404(Not Found)
![404Member](assests/404Member.png)

-**PostMember(MemberRequestDTO member)**
* On successful entry of valid Data -> 201(Created)
![PostMember](assests/PostMember.png)
* Name Validation -> If Empty -> 400(Bad Request)
* Same Null Validation is done for Email and PhoneNumber field
![400Name](assests/400Name.png)
* Email Validation -> IF @ or ".com" not present -> 400 (Bad Request)
![400Email](assests/400Email.png)
* Phone Validation -> Should be of size 10 and contain only digits, else -> 400 (Bad Request)
![400Phone](assests/400Phone.png)


## SQL Tables
* Book
![BookTable](assests/BookTable.png)
* Member
![MemberTable](assests/MemberTable.png)
