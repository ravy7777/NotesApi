# Notes API

This is a simple Notes API built with ASP.NET Core and Dapper for managing notes. The API supports CRUD operations and pagination.

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/ravy7777/NotesApi.git
    cd NotesApi
    ```

2. Restore the dependencies:
    ```sh
    dotnet restore
    ```

3. Update the connection string in `appsettings.json`:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Your SQL Server connection string here"
    }
    ```

4. Run the application:
    ```sh
    dotnet run
    ```

### API Documentation

The API documentation is available via Swagger. Once the application is running, you can access it at: http://localhost:5141/swagger/index.html