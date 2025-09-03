# FileChunkingSystem

FileChunkingSystem is a .NET 9 console application designed for distributed file chunking and reassembly. It allows users to upload files, split them into chunks for distributed storage, and reassemble them when needed. The application uses File System, PostgreSQL and MongoDB for metadata and storage management, with Docker for containerized deployment.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/install/)
- PostgreSQL and MongoDB must be accessible (database setup is automatically handled by the application on startup)

## Configuration

The application uses the following connection strings, configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "MetadataConnection": "Host=localhost;Database=filechunking_dev;Username=postgres;Password=1",
  "PostgreSQL": "Host=localhost;Database=filechunking_storage_dev;Username=postgres;Password=1",
  "MongoDB": "mongodb://localhost:27017"
}
```

Ensure that PostgreSQL and MongoDB are accessible with the specified credentials. Database setup (e.g., table and schema creation) is automatically performed when the application started.

## Setup and Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/resuldemirok/FileChunkingSystem.git
   cd FileChunkingSystem
   ```

2. **Build the Project**
   ```bash
   dotnet build
   ```

3. **Run Database Migrations**
   - For Metadata:
     ```bash
     dotnet ef migrations add InitialMetadataCreate --context MetadataDbContext --project FileChunkingSystem.Infrastructure --startup-project FileChunkingSystem.Console --output-dir Migrations/Metadata
     ```
   - For Storage:
     ```bash
     dotnet ef migrations add InitialStorageCreate --context StorageDbContext --project FileChunkingSystem.Infrastructure --startup-project FileChunkingSystem.Console --output-dir Migrations/Storage
     ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

## Running the Application

### Local Development
1. Navigate to the console project:
   ```bash
   cd FileChunkingSystem.Console
   ```
2. Run the application:
   ```bash
   dotnet run
   ```

### Using Docker
1. Ensure Docker and Docker Compose are installed.
2. Build and Run the application with Docker Compose:
   ```bash
   docker build -t filechunking-app .
   docker compose run --rm filechunking-app
   ```
3. Stop the containers:
   ```bash
   docker compose down
   ```

### File Storage
The application uses the following directories for file operations:
- **Upload Directory**: `C:/filechunking/files` (mapped to `/app/files` in the Docker container)
- **Restored Files Directory**: `C:/filechunking/restored` (mapped to `/app/restored` in the Docker container)

Files uploaded to `C:/filechunking/files` (e.g., `C:/filechunking/storage/test.exe`) are chunked and stored in a distributed manner. Reassembled files are saved to `C:/filechunking/restored`.

## Usage

1. **File Upload and Chunking**:
   - Place files in the `C:/filechunking/files` directory (or `/app/files` in the container).
   - The application splits the files into chunks and stores them in the configured databases (MongoDB, PostgreSQL and File System for storage, PostgreSQL for metadata).

2. **File Reassembly**:
   - The application retrieves chunks from the databases and the file system and reassembles them into the original file, saving it to `C:/filechunking/restored` (or `/app/restored` in the container).

## Project Structure

- **FileChunkingSystem.Console**: Entry point for the console application, responsible for running the main program logic.
- **FileChunkingSystem.Infrastructure**: Contains database contexts and migrations for interacting with PostgreSQL and MongoDB.
- **FileChunkingSystem.Application**: Contains the business logic and services for handling file chunking and reassembly operations.
- **FileChunkingSystem.Application.Tests**: Contains unit and integration tests for the Application layer to ensure the reliability of business logic.
- **FileChunkingSystem.Domain**: Defines the core domain models and entities representing the business objects used in file chunking and storage.
- **Migrations/Metadata**: EF Core migrations for the MetadataDbContext, managing the schema for metadata storage in PostgreSQL.
- **Migrations/Storage**: EF Core migrations for the StorageDbContext, managing the schema for file storage in PostgreSQL.

## Notes
- Ensure the specified directories (`C:/filechunking/files` and `C:/filechunking/restored`) exist on your system while running the application after auto mount.
- For Docker, verify that the volume mappings in the `docker-compose.yml` file match your local directory structure.
- The application automatically handles PostgreSQL and MongoDB database setup on startup, as long as the databases are accessible with the provided credentials.

## Troubleshooting
- **Database Connection Issues**: Verify that PostgreSQL and MongoDB are accessible with the provided credentials.
- **File Path Errors**: Ensure the file directories exist and are accessible by the application or Docker container.
- **Docker Issues**: Check that the Docker Compose file is correctly configured and that the volumes are properly mapped.