Thank you for sending me this Technical Task.

This is my solution to the following:

Please see the task instructions below-
» We’d like you to complete a small self-contained solution that adds, edits, and deletes
customers.
» We’d like you to complete this task using ASP.NET UI project for the fronted, and ASP.NET
API for the backend/ data access.
» You can implement some sort of database/ data storage for this task, or you can hard code
it.
» Please note, this is not a UI/ design exercise, we are not majorly concerned about the look
and feel.
» The task should take you 1-2 hours to complete.

My solution is to have 2 projects, an asp.net core 8 mvc project and a asp.net core 8 API project.
The mvc project will be the front end and the API project will be the backend. 
The mvc project will call the API project to get the data and display it in the UI. 
The mvc project will also call the API project to add, edit and delete customers.
The API project is secured using JWT tokens.
The API project uses Entity Framework Core to access the database.
The API project uses a SQL express database. Please install this first, and change the connection string as applicable.
The API project has seed data which will populate the database with some initial data.
I have created a healthcheck table, which will be used to check if the database is up and running.
I have also added a swagger UI to the API project, so you can test the API endpoints.
I have also added serilog to log any issues and information to a log file.
I have added identity with roles to the project, you need to be logged in to alter the customer data, this is done using a custom Authorize attribute, which can be applied to the controller / controller actions like this: [CustomAuthorize("Admin")]
I have created an AuthenticationHandler, which will check the JWT token and validate it.

----------------------------------------------------------------------------------------------------------------------------

Prerequisites:
1. Visual Studio 2022
2. SQL Express

To run this project, please follow the steps below:
1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Change the connection string in the API project to point to your SQL Express database
4. Configure startup projects, and select both as startup projects
5. Run the solution

Thanks, Craig Williams.