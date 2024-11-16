# Kanbam Documentation

## Overview

**Kanbam** is a project management application built on the kanban board methodology. Designed for teams of all sizes, it enables streamlined task management and collaboration, offering flexibility, real-time updates, and actionable insights.

### Key Features

- **Dynamic Kanban Boards**: Create unlimited workspaces and boards to manage your task or projects.
- **Drag-and-Drop Functionality**: Update task status intuitively.
- **Collaborative Environment**: Share boards with teammates and assign tasks.
- **Data Visualization**: Leverage charts to analyze project progress.
- **Scalable Architecture**: Supports both small teams and mid-level projects.

---

## Frontend Documentation

**Repository**:

[Frontend (GitHub)](https://github.com/Hassen-Ahmed/Kanbam)

### Tech Stack

- **React.js**: Component based UI framework.
- **TypeScript**: Type safe JavaScript.
- **Sass/SCSS**: Streamlined styling.
- **Axios**: Manipulating data through HTTP requests.
- **nivo**: Visual representation of project data.
- **styled-components**: Theme management and styling.

### Setup Instructions

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/Hassen-Ahmed/Kanbam.git
   cd Kanbam
   ```

2. **Install Dependencies**:
   ```bash
   npm install
   ```
3. **Run the Development Server**:
   ```bash
   npm run dev
   ```
4. **Access the Application**:
   Open `http://localhost:3000` in your browser.

### Testing Guidelines

- **Unit Tests**: Use `npm test` (requires Vitest configuration).

---

## Backend Documentation

**Repository**: [Backend (GitHub)](https://github.com/Hassen-Ahmed/KanbamApi)

### Tech Stack

- **ASP.NET Core**: Web API framework.
- **C#**: Backend logic programming.
- **MongoDB**: NoSQL database.
- **Docker**: Containerized deployments.

### Others

- **UptimeRobot**: To pinging my API hosted on a free-tier service.
- **Cron-job.org**: To pinging my API hosted on a free-tier service.

### Setup Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Hassen-Ahmed/KanbamApi.git
   cd KanbamApi
   ```
2. **Install Prerequisites**:
   - Install [Docker](https://www.docker.com/).
   - Install [.NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/).
3. **Run the Server**:
   ```bash
   dotnet watch run
   ```
4. **Test the API**:
   Use tools like Postman or cURL. The base URL is typically `http://localhost:5000`.

### API Endpoints

- **Authentication**:
  - `POST /api/Auth/login`: User login.
  - `POST /api/Auth/register`: User registration.
- **Workspaces**:
  - `GET /api/Workspaces`: Fetch all workspaces.
  - `POST /api/Workspaces`: Create a new workspaces.
  - `PATCH /api/Workspaces/:id`: Update workspaces details.
  - `DELETE /api/Workspaces/:id`: Delete a workspaces.
- **WorkspacesMembers**:
  - `GET /api/WorkspacesMembers`: Fetch all WorkspacesMembers.
  - `GET /api/WorkspacesMembers/:id`: Fetch all WorkspacesMembers by id.
  - `POST /api/WorkspacesMembers`: Create a new WorkspacesMembers.
  - `PATCH /api/WorkspacesMembers/:id`: Update WorkspacesMembers details.
  - `DELETE /api/WorkspacesMembers/:id`: Delete a WorkspacesMembers.
- **Boards**:
  - `GET /api/boards`: Fetch all boards.
  - `GET /api/boards/:workspaceId`: Fetch all boards by workspaceId.
  - `POST /api/boards`: Create a new board.
  - `PATCH /api/cards/:id`: Update Boards details.
  - `DELETE /api/boards/:id`: Delete a board.
- **BoardsMembers**:
  - `GET /api/BoardsMembers`: Fetch all BoardsMembers.
  - `GET /api/BoardsMembers/:workspaceId`: Fetch all BoardsMembers by workspaceId.
  - `POST /api/BoardsMembers`: Create a new BoardsMembers.
  - `PATCH /api/BoardsMembers/:id`: Update BoardsMembers details.
  - `DELETE /api/BoardsMembers/:id`: Delete a BoardsMembers .
- **Lists**:
  - `GET /api/Lists`: Fetch all Lists.
  - `GET /api/Lists/:boardId`: Fetch all Lists by boardId.
  - `POST /api/Lists`: Create a new Lists.
  - `PATCH /api/Lists/:id`: Update Lists details.
  - `DELETE /api/Lists/:id`: Delete a Lists.
- **Cards**:
  - `GET /api/Cards/:listId/list`: Get all Cards for a Cards by listId.
  - `GET /api/Cards/:cardId/card`: Get all Cards for a Cards by cardid.
  - `POST /api/Cards`: Create a new card.
  - `PATCH /api/Cards/:id`: Update card details.
  - `DELETE /api/Cards/:id/card`: Delete a card by cardId.
  - `DELETE /api/Cards/:listId/list`: Delete a card by listId.

---

## Environment Variables

### Frontend

Define environment variables in a `.env.development` and `.env.production` file:

```bash
VITE_KANBAM_API_URL=...
```

### Backend

Define environment variables in a `.env.development` and `.env.production` file:

```bash
TOKEN_KEY=...
PASSWORD_KEY=...


CONNECTION_STRING=...
DB_NAME=...
AUTH_COLLECTION_NAME=...
USERS_COLLECTION_NAME=...
LISTS_COLLECTION_NAME=...
CARDS_COLLECTION_NAME=...
VISITORS_COLLECTION_NAME=...

BOARDS_COLLECTION_NAME=...
BOARDSMEMBERS_COLLECTION_NAME=...
WORSPACES_COLLECTION_NAME=...
WORSPACESMEMBERS_COLLECTION_NAME=...


SECRET_KEY=...

VALID_ISSUER=...
VALID_AUDIENCE=...
```

---

## Design System

### UI Components

- Buttons, modals, and card components should follow reusable patterns.
- Use consistent typography, spacing, and colors as per SCSS variables.

---

## Development Notes

- Secure sensitive data using environment variables.
- Sanitize user inputs to avoid XSS vulnerabilities.
- Validate all incoming API requests to prevent malicious actions.

---

## Contributing Guidelines

### How to Contribute

1. Fork the repository and create a branch for your changes.
2. Follow coding standards:
   - Use meaningful commit messages (e.g., `feat: add drag-and-drop for cards`).
   - Write clean and modular code.
3. Submit a pull request with a clear description of the changes.

---

## Roadmap

- **Real-Time Updates**: Implement WebSockets for live collaboration.
- **Advanced Analytics**: Add Gantt chart views and team performance metrics.
- **Localization**: Support multiple languages.
- **Offline Mode**: Allow board access without connectivity, with synchronization.

---

## Known Issues

- **Frontend**: Drag-and-drop may require polyfills for older browsers.

---

## Live Demo

Experience the application here: [Kanbam Live Demo](https://kanbam.netlify.app/).

---

## Acknowledgments

Created and maintained by **Hassen Ahmed**. Contributions are welcome via GitHub Issues or Pull Requests:

- Frontend: [Kanbam Issues](https://github.com/Hassen-Ahmed/Kanbam/issues)
- Backend: [KanbamApi Issues](https://github.com/Hassen-Ahmed/KanbamApi/issues).

## License

This project is licensed under the [MIT License](LICENSE.txt). See the [LICENSE](LICENSE.txt) file for details.
