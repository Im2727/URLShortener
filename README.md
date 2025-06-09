# URL Shortener API

A robust URL shortener service built with ASP.NET Core, Entity Framework Core, and MySQL/SQLite. Features include:
- Shorten URLs (random or custom code)
- Redirect with analytics (redirect count)
- URL expiration
- Persistent storage (SQLite or MySQL)
- Modern web UI
- RESTful API endpoints

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (or compatible)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (or use SQLite for local testing)
- (Optional) [Git](https://git-scm.com/) for version control

---

## Setup Instructions

### 1. Clone the Repository
```sh
git clone <your-repo-url>
cd URLShortenerApi
```

### 2. Configure the Database

#### MySQL (Recommended for Production)
1. Install MySQL Server and ensure it is running.
2. Create a database (e.g., `urlshortener`):
   ```sh
   mysql -u root -p
   CREATE DATABASE urlshortener;
   ```
3. Update the `appsettings.json` file with your MySQL connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;port=3306;database=urlshortener;user=root;password=YOUR_PASSWORD;"
   }
   ```

#### SQLite (For Local Testing)
- The project is pre-configured to use SQLite if you set the connection string accordingly in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=urlshortener.db"
   }
   ```

### 3. Install Dependencies
```sh
dotnet restore
```

### 4. Apply Database Migrations
```sh
dotnet ef database update
```
If you get an error about missing tools, install EF Core tools:
```sh
dotnet tool install --global dotnet-ef
```

### 5. Run the Application
```sh
dotnet run
```
The API and UI will be available at [http://localhost:5016](http://localhost:5016).

---

## Usage

### Web UI
- Open [http://localhost:5016](http://localhost:5016) in your browser.
- Use the form to shorten URLs, view analytics, and perform reverse lookups.

### API Endpoints
- `POST /shorten/shorten` — Shorten a URL (optionally with custom code and expiration)
- `GET /shorten/shorten?url={url}` — Shorten a URL (random code)
- `GET /shorten/shorten/{code}?url={url}` — Shorten with custom code
- `GET /shorten/custom/{customCode}/{*url}` — Shorten with custom code (route style)
- `GET /shorten/{*url}` — Shorten with random code (route style)
- `GET /shorten/analytics/{code}` — Get redirect count
- `GET /shorten/original/{code}` — Get original URL and expiration
- `GET /{code}` — Redirect to original URL (increments analytics)

### Example Request (cURL)
```sh
curl -X POST http://localhost:5016/shorten/shorten -H "Content-Type: application/json" -d '{"url": "https://example.com"}'
```

---

## Development

- Feature development is done in separate git branches. Use `git checkout -b feature/your-feature` for new features.
- Run unit tests:
  ```sh
  cd ../URLShortenerApi.Tests
  dotnet test
  ```

---

## Troubleshooting
- Ensure MySQL or SQLite is running and accessible.
- Check your connection string in `appsettings.json`.
- If ports conflict, update `launchSettings.json` or `Program.cs`.

---
