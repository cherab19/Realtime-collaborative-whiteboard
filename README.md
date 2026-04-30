# Real-Time Collaborative Whiteboard

## Tech Stack
- ASP.NET Core Web Application (.NET 10)
- Entity Framework Core
- SQLite
- SignalR (implemented in Phase 2)
- HTML5 Canvas UI (to be added in Phase 3)

## Current Progress
Phase 1 completed:
- ASP.NET Core project scaffolded
- EF Core configured with SQLite
- Core models implemented: User, WhiteboardSession, DrawingLog
- DbContext implemented
- Session CRUD APIs implemented with service layer
- Input validation and server-side error handling baseline added

Phase 2 completed:
- SignalR hub implemented for real-time drawing synchronization
- Drawing events broadcast to session groups
- Session join/leave with user notifications
- Canvas clear functionality with history persistence
- Connect/disconnect lifecycle logging
- CORS configured for hub and API clients

## Project Structure
- RealtimeWhiteboard/Controllers
- RealtimeWhiteboard/Models
- RealtimeWhiteboard/Data
- RealtimeWhiteboard/Hubs (completed in Phase 2)
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

SignalR Hub URL (development):
- ws://localhost:xxxx/whiteboardHub

## Implemented API Endpoints (Phase 1)
- GET /api/sessions
- GET /api/sessions/{id}
- POST /api/sessions
- PUT /api/sessions/{id}
- DELETE /api/sessions/{id}

## SignalR Hub Methods (Phase 2)

Hub URL: `/whiteboardHub`

**Server-to-Client methods (invoked by frontend):**
- `JoinSession(int sessionId, string userDisplayName)` - User joins a session
- `LeaveSession(int sessionId, string userDisplayName)` - User leaves a session
- `SendDrawing(int sessionId, DrawingEventDto drawingEvent)` - Broadcast drawing
- `ClearCanvas(int sessionId)` - Clear canvas and remove history

**Client-to-Server events (received by frontend):**
- `LoadHistory` - Drawing history for session
- `UserJoined` - Notification when user joins
- `UserLeft` - Notification when user leaves
- `ReceiveDrawing` - Remote drawing event
- `CanvasCleared` - Canvas was cleared

## Remote Team Guide For Remaining Phases (Phase 2-5)

This section is the implementation contract for all remote teammates. Follow it strictly for all remaining phases.

### Academic Guideline (Must Follow)
- Platform: ASP.NET Core Web Application
- Language: C# (.NET 6 or later)
- Real-time: SignalR is mandatory
- UI: HTML5 Canvas or Blazor
- Database: SQLite or SQL Server (current project uses SQLite)
- ORM: Entity Framework Core
- Architecture style: OOP, modular, clean code

### Mandatory Functional Scope
- Multiple users drawing simultaneously
- Real-time synchronization of drawing events
- User session handling (create, join, leave)
- CRUD operations for whiteboard sessions
- Input validation and error handling
- Simple reporting: list whiteboards and active users

### Team Roles And Ownership
1. Backend Developer (`backend-dev`)
- Maintains Models, DbContext, APIs, and data rules.
- Extends CRUD and persistence contracts as needed.

2. Real-Time Engineer (`signalr-dev`)
- Builds and maintains SignalR hub logic.
- Handles connect/disconnect, session join/leave, and broadcasting.

3. Frontend Developer (`frontend-dev`)
- Builds canvas UI and drawing tools.
- Implements responsive and smooth drawing experience.

4. Integration Developer (`integration-dev`)
- Connects frontend and SignalR client workflows.
- Ensures draw payloads are sent/received and rendered correctly.

5. System And QA Engineer (`qa-dev`)
- Validates session workflows and edge cases.
- Verifies input validation, error handling, and reporting endpoints/UI.

### Branching Strategy (Required)
Each teammate works only on their branch:
- `backend-dev`
- `signalr-dev`
- `frontend-dev`
- `integration-dev`
- `qa-dev`

Keep `main` protected. Use Pull Requests for all merges.

### Commit Message Standard
Use meaningful, feature-focused messages.

Good examples:
- `Implemented SignalR drawing synchronization hub`
- `Added canvas tools for line and rectangle drawing`
- `Created session join and leave APIs with validation`

Bad examples:
- `update`
- `fix`
- `changes`

### Required Folder Structure
- `RealtimeWhiteboard/Controllers`
- `RealtimeWhiteboard/Models`
- `RealtimeWhiteboard/Data`
- `RealtimeWhiteboard/Hubs`
- `RealtimeWhiteboard/Services`
- `RealtimeWhiteboard/wwwroot`

### Remaining Phase Plan

#### Phase 2: SignalR Real-Time Infrastructure
Owner: `signalr-dev`

**Status: COMPLETED**

Deliverables (all completed):
- `WhiteboardHub` created in `RealtimeWhiteboard/Hubs`
- Drawing sync: `SendDrawing` broadcasts to session group
- Session events: `JoinSession` and `LeaveSession` with user notifications
- Lifecycle: `OnConnectedAsync` and `OnDisconnectedAsync` logging
- Canvas clear: `ClearCanvas` with history persistence
- Connection state: CORS configured for hub clients
- Responsive HTML5 canvas UI
- Tools: freehand, line, rectangle
- Color picker and brush size selector
- Clear and consistent tool state handling

Definition of done:
- User can draw smoothly with all required tools in a single-client mode.

#### Phase 4: Frontend + SignalR Integration
Owner: `integration-dev`

Deliverables:
- SignalR client connection in frontend JS/Blazor
- Send local drawing events to hub
- Receive remote drawing events and render on canvas
- Reconnect handling and user-friendly connection states

Definition of done:
- Multi-client drawing synchronization works end-to-end in real time.

#### Phase 5: Sessions, Validation, Reporting, QA
Owner: `qa-dev` (with cross-team support)

Deliverables:
- Session create/join/leave end-to-end flow
- Validation for session input and drawing payload constraints
- Error handling for API and SignalR failures
- Reporting:
	- list sessions
	- show active users per session

Definition of done:
- All required flows pass manual QA checks and return expected API/UI results.

### Pull Request Checklist (Use In Every PR)
- Scope is limited to assigned role and phase
- Code follows OOP and existing project structure
- Input validation added for new inputs
- Error paths handled and tested manually
- `dotnet build` succeeds
- No secrets or local artifacts committed
- README updated if behavior/contracts changed

### Suggested Integration Rules
- Avoid changing unrelated files.
- Rebase branch with latest `main` before opening PR.
- Resolve conflicts locally and re-run `dotnet build`.
- Include short PR notes:
	- what changed
	- why changed
	- how to test

### Test Commands For Teammates
From repository root:
1. `cd RealtimeWhiteboard`
2. `dotnet restore`
3. `dotnet build`
4. `dotnet run`

### Current Status Summary
- Phase 1: Completed and pushed.
- Phase 2: Completed and ready for push.
- Phase 3-5: Pending; must be implemented according to this guide.
