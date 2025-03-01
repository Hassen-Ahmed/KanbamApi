# Kanbam Documentation

## Overview

**Kanbam** is a project management application built on the kanban board methodology. Designed for teams of all sizes, it enables streamlined task management and collaboration, offering flexibility, real-time updates, and actionable insights.

### Key Features

- **Dynamic Kanban Boards**: Create unlimited workspaces and boards to manage your task or projects.
- **Drag-and-Drop Functionality**: Update task status intuitively.
- **Collaborative Environment**: Share boards with teammates and assign tasks.
- **Data Visualization**: Leverage charts to analyze project progress.
- **LogIn and SignUp**: Users can create their own environment by signup and loging.
- **Reset Password**: If users lost their passwrod they can reset their password by utilizing this functionality.
- **Scalable Architecture**: Supports both small teams and mid-level projects.

---

## Frontend Documentation

**Repository**:

[Frontend (GitHub)](https://github.com/Hassen-Ahmed/Kanbam)

### Tech Stack

- **React.js**: Component based UI library.
- **TypeScript**: Type safe JavaScript.
- **Sass/SCSS**: CSS preprocessor and Streamlined styling.
- **Axios**: Manipulating data through HTTP requests.
- **Nivo**: Visual representation of project data.
- **styled-components**: Theme management and styling.
- **SignalR**: Real-time, two-way communication between the client and server facilitates live streaming and streamlines collaborative tasks or project management for users.
- **Gemini AI**: Used to generate user preference themes for the app.

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
   Open `http://localhost:5173` in your browser.

### Testing Guidelines

- **Unit Tests**: Use `npm test` (requires Vitest configuration).
- **Vitest setup**: **[VITEST.md](./readme_collection/VITEST.md)**

---

## Backend Documentation

**Repository**: [Backend (GitHub)](https://github.com/Hassen-Ahmed/KanbamApi)

### Tech Stack

- **ASP.NET Core**: Web API framework.
- **C#**: Backend logic programming.
- **MongoDB**: NoSQL database.
- **MongoDB Identity**: Auth and user manager.
- **Docker**: Containerized deployments.
- **SignalR**: Real-time, two-way communication between the client and server facilitates live streaming and streamlines collaborative tasks or project management for users..
- **Bogus**: Used to generate testing data.
- **DotNetEnv**: Effortlessly store and retrieve sensitive data.
- **Asp.net core Identity with MongoDB Identity**: Used for Authentication and other user management.
- **MailKit**: Used to send links to reset users password via their email address as primary option.
- **SendGrid**: Used to send links to reset users password via their email address as a secondary option.

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
   Use tools like Postman or cURL. The base URL is typically `http://localhost:5011`.

### API Endpoints

- **Authentication**:
  - `POST /api/Auth/login`: User login.
  - `POST /api/Auth/register`: User registration.
  - `POST /api/Auth/RefrshToken`: To retate refreshToken and accessToken.
  - `POST /api/Auth/RevokeRefrshToken`: To delete refreshToken from Cookie and database.
  - `POST /api/Auth/ForgotPassword`: To send a link to reset their password to users email address.
  - `POST /api/Auth/ResetPassword`: To reset users password.
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
  - `DELETE /api/boards/:id/:workspaceId`: Delete a board.
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
  - `POST /api/Cards/:cardId/comment`: Create a new cardComment.
  - `PATCH /api/Cards/:id`: Update card details.
  - `DELETE /api/Cards/:id/card`: Delete a card by cardId.
  - `DELETE /api/Cards/:listId/list`: Delete a card by listId.
  - `DELETE /api/Cards/:cardId/:commentId/comment`: Delete a cardComment.
- **Doc**:
  - `GET /api/`: For ping, status checking.

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

CONNECTION_STRING=...
DB_NAME=...
REFRESH_TOKENS_COLLECTION_NAME=...
LISTS_COLLECTION_NAME=...
CARDS_COLLECTION_NAME=...
COMMENTS_COLLECTION_NAME=...
BOARDS_COLLECTION_NAME=...
BOARDSMEMBERS_COLLECTION_NAME=...
WORSPACES_COLLECTION_NAME=...
WORSPACESMEMBERS_COLLECTION_NAME=...

SECRET_KEY=...

VALID_ISSUER=...
VALID_AUDIENCE=...

SENDGRID_API_KEY=...
SENDGRID_FROM_EMAIL=...
SENDGRID_FROM_NAME=...

SMTP_USERNAME=...
SMTP_PASSWORD=...
SMTP_PORT=...
SMTP_HOST=...

DOMAIN_NAME_PW=...
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
   - Use meaningful commit messages (e.g., `feat(front-end): add drag-and-drop for cards`).
   - Write clean and modular code.
3. Submit a pull request with a clear description of the changes.

---

## Roadmap

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
