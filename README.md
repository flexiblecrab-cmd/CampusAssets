# Campus Equipment Loan Data Access Layer
# .NET Fundamentals Topic 3

# Project Overview
This project is a simple C# console application that demonstrates how to use Entity Framework Core with SQLite to manage equipment loan records. It allows users to record borrowed equipment, view active loans, update loan status, and remove records from the database.

The project was created as a learning exercise to understand database operations using Entity Framework Core.

# Features
- Create a new equipment loan record
- View active equipment loans
- Mark equipment as returned
- Delete equipment loan records
- Store data in a SQLite database
- Demonstrate CRUD (Create, Read, Update, Delete) operations

# Technologies Used
- C#
- .NET
- Entity Framework Core
- SQLite

# Project Structure
- EquipmentLoan – Represents a loan record stored in the database.
- CampusAssetContext – Database context used to communicate with SQLite.
- campus-assets.db – SQLite database file created automatically.

# Prerequisites
Before running the project, ensure you have:
- .NET SDK installed
- Visual Studio or Visual Studio Code
- Entity Framework Core packages
- SQLite provider package

# Setup Instructions
1. Clone the repository:
git clone <repository-url>
2. Navigate to the project folder:
cd CampusAssetManagement
3. Restore dependencies:
dotnet restore
4. Build the project:
dotnet build
5. Run the application:
dotnet run

# How It Works
Create
- Adds a new equipment loan record.

Example:

Borrower: Maria Santos
Equipment: Laptop 014

# Read
Displays active loans that have not yet been returned.

# Update
Changes the loan status from active to returned.

# Delete
Removes the selected loan record from the database.

# Database
The application automatically creates a SQLite database file:

campus-assets.db
This file stores all equipment loan information.

# Assumptions
- Each equipment loan has one borrower.
- Equipment names are limited to 120 characters.
- Borrower names are limited to 120 characters.
- Active loans are records where IsReturned is set to false.

# Known Limitations
- No graphical user interface (GUI).
- No authentication or user management.
- No validation beyond basic database rules.
- Data is stored locally using SQLite.
- Designed for learning and demonstration purposes only.

# Sample Output
Active loans before return: 1

# Learning Objectives
This project demonstrates:
- Entity Framework Core fundamentals
- SQLite database integration
- CRUD operations
- LINQ queries
- Asynchronous database operations using async/await
