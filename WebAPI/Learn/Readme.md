**Learn**
Web API project created for experimental purpose
**To Run**
dotnet restore -> dotnet build -> dotnet run

**Project Structure:**
* Controller : has API endpoints
* Models : Entity Defintion
* Program.cs : Main Execution of code (Top - level statment Execution)
* Properties : has the behaviour (properties) of the project environment
* .csproj : has dependencies

**WorkFlow**
Program.cs (client) -> Routing Table (request) -> Controller perform operation wrt Model -> Response -> Client