# Real-Time Collaborative Whiteboard

## Tech Stack
- ASP.NET Core Web Application (.NET 10)
- Entity Framework Core
- SQLite
- SignalR (to be added in Phase 2)
- HTML5 Canvas UI (to be added in Phase 3)

## Current Progress
Phase 1 completed:
- ASP.NET Core project scaffolded
- EF Core configured with SQLite
- Core models implemented: User, WhiteboardSession, DrawingLog
- DbContext implemented
- Session CRUD APIs implemented with service layer
- Input validation and server-side error handling baseline added

## Project Structure
- RealtimeWhiteboard/Controllers
- RealtimeWhiteboard/Models
- RealtimeWhiteboard/Data
- RealtimeWhiteboard/Hubs (planned in Phase 2)
- RealtimeWhiteboard/Services
- RealtimeWhiteboard/wwwroot

## Run Locally
1. cd RealtimeWhiteboard
2. dotnet restore
3. dotnet build
4. dotnet run

API base URL (development):
- https://localhost:xxxx/api/sessions

Swagger UI is enabled in development for API testing.

## Implemented API Endpoints (Phase 1)
- GET /api/sessions
- GET /api/sessions/{id}
- POST /api/sessions
- PUT /api/sessions/{id}
- DELETE /api/sessions/{id}