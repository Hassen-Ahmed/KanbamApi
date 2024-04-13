# ⭐️ Kanbam ⭐️

Kanbam is a web application inspired by Trello's kanban board, designed to help teams manage projects and tasks effectively.

## Features

- Create multiple boards to organize projects.
- Add lists to boards to represent different stages of a project.
- Add cards to lists to represent tasks or items to be completed.
- Drag and drop cards between lists to update their status.
- Collaborate with team members by sharing boards and assigning tasks.
- And more!

## Tech Stack

- **Backend**:

  - ASP.NET Core: A cross-platform, high-performance framework for building web applications with C#.
  - C#: A powerful, statically typed programming language for building robust applications.
  - MongoDB: A NoSQL database for storing and managing data. Also I used Mogodb Drive package which help dotnet api to interact with mongodb server.
  - Docker: A platform for building, shipping, and running applications in containers.

- **Frontend**:
  - React.js: A JavaScript library for building user interfaces.
  - TypeScript: A statically typed superset of JavaScript that adds optional static typing.
  - Sass/SCSS: A CSS extension language that adds features like variables, mixins, and nesting.
  - Chart.js: Is JavaScript library for data visualization.

## Getting Started

### Prerequisites

- Docker installed on your machine.
- Node.js and npm installed for frontend development.
- I am using VS Code code editor.
- You need to download Dotnet SDK based on your system I am using Linux.

#### For installation visit this microsoft link

```js
https://learn.microsoft.com/en-us/dotnet/core/install/
```

### Installation

1. Clone the repository Front-End

   ```bash
   git clone https://github.com/Hassen-Ahmed/Kanbam.git
   cd kanbam
   npm install
   npm run dev
   ```

2. Clone the repository Back-End
   ```bash
   git clone https://github.com/Hassen-Ahmed/KanbamApi.git
   cd kanbamApi
   dotnet watch run
   ```

## KanbamApi dotnet packages

- DotNetEnv
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.OpenApi
- MongoDB.Driver
- Swashbuckle.AspNetCore

## Contributing

Contributions are welcome! If you'd like to contribute to Kanbam or KanbamApi, please follow these steps:

- Fork the repository.
- Create a new branch for your feature or bug fix: git checkout -b feature/my-feature.
- Commit your changes: git commit -am 'Add new feature'.
- Push to your branch: git push origin feature/my-feature.
- Submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE.txt). See the [LICENSE](LICENSE.txt) file for details.
