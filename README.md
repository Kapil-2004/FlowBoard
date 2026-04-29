# ≋ FlowBoard – Modern Kanban Task Management

![Version](https://img.shields.io/badge/version-1.0.0--beta-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Tech](https://img.shields.io/badge/tech-Angular%20%7C%20.NET%208%20%7C%20Postgres-purple)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

FlowBoard is a high-performance, aesthetically pleasing Kanban-style task management application built with a modern tech stack. It features a robust .NET 8 microservice architecture for the backend and a sleek, responsive Angular 17+ standalone component architecture for the frontend.

---

## 📖 Table of Contents

- [Features](#-features)
- [Tech Stack](#%EF%B8%8F-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Quick Start (Docker)](#quick-start-docker)
  - [Manual Setup (Development)](#manual-setup-development)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Microservices Overview](#-microservices-overview)
- [Core Design Principles](#-core-design-principles)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### Phase 1: Authentication & Identity (Stable)
- **Secure Registration**: User signup with password hashing and validation.
- **JWT Authentication**: Token-based security for stateless API interaction.
- **OAuth Integration**: Support for Google and GitHub authentication.
- **Profile Management**: Update user details and avatars.
- **Premium Aesthetics**: Monochromatic design system with Plus Jakarta Sans typography.

### Phase 2: Workspace & Collaboration (Active)
- **Workspaces**: Create and manage isolated project environments.
- **Member Management**: Search and invite users to workspaces via email/name.
- **Role-Based Access**: ADMIN and MEMBER roles for granular control.
- **Real-time Notifications**: Custom toast system for instant operation feedback.
- **Visibility Control**: Toggle between PUBLIC and PRIVATE workspaces.

### Phase 3: Task Management (Upcoming)
- **Kanban Boards**: Drag-and-drop task management.
- **Lists & Cards**: Granular task organization with labels and deadlines.
- **Real-time Updates**: Instant synchronization across clients.

---

## 🛠️ Tech Stack

### Frontend
- **Framework**: Angular 17+ (Standalone Components)
- **State Management**: Signals & RxJS
- **Styling**: Vanilla CSS with a Custom Design System (CSS Variables)
- **Icons**: Lucide Icons & Custom SVG
- **Interceptors**: Automated JWT handling for secure API calls

### Backend
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Database**: PostgreSQL 16 (Relational storage)
- **ORM**: Entity Framework Core (Code First)
- **Security**: BCrypt hashing & JWT Bearer tokens
- **Documentation**: Swagger / OpenAPI 3.0

### DevOps & Infrastructure
- **Containerization**: Docker & Docker Compose
- **Orchestration**: Multi-container setup with health checks
- **Database Migrations**: Automated EF Core migrations on startup

---

## 🏗️ Architecture

FlowBoard utilizes a **Microservice Architecture** to ensure scalability and maintainability.

1.  **Auth Service (Port 5001)**: Handles identity, authentication, and user profiles.
2.  **Workspace Service (Port 5002)**: Manages workspaces, memberships, and roles.
3.  **Board Service (Port 5003)**: Manages Kanban boards, lists, and task logic.
4.  **Frontend (Port 4200)**: Angular SPA serving as the primary client interface.
5.  **Shared Databases**: Independent PostgreSQL instances for each microservice to enforce data isolation.

---

## 🚀 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)

### Quick Start (Docker)
The entire stack (Backend + Frontend + DB) can be launched with a single command:

```bash
docker-compose up --build
```
- **Frontend**: http://localhost:4200
- **Auth API**: http://localhost:5001
- **Workspace API**: http://localhost:5002/swagger

### Manual Setup (Development)

#### 1. Databases
Start the PostgreSQL containers for development:
```bash
docker-compose up -d postgres postgres_workspace
```

#### 2. Backend Services
Open two terminals:
```bash
# Terminal 1
cd Backend/FlowBoard-Auth
dotnet run

# Terminal 2
cd Backend/FlowBoard-Workspace
dotnet run
```

#### 3. Frontend
```bash
cd Frontend
npm install
npm start
```

---

## 📑 API Documentation

Each service provides its own Swagger UI for interactive documentation:

- **Auth Service**: `http://localhost:5001/`
- **Workspace Service**: `http://localhost:5002/swagger/index.html`

### Key Endpoints
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Create a new user account |
| `POST` | `/api/auth/login` | Authenticate and receive JWT |
| `GET` | `/api/auth/users/search` | Search for users by name/email |
| `POST` | `/api/workspaces` | Create a new workspace |
| `GET` | `/api/workspaces/member` | Get all workspaces for the current user |
| `POST` | `/api/workspaces/{id}/members` | Add a member to a workspace |

---

## 📁 Project Structure

```text
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/         # .NET 8 Identity Microservice
│   │   ├── Controllers/        # Auth & Profile Endpoints
│   │   ├── Services/           # Business Logic (BCrypt, JWT)
│   │   └── Data/               # AuthDbContext (Postgres)
│   ├── FlowBoard-Workspace/    # .NET 8 Workspace Microservice
│   │   ├── Controllers/        # Workspace & Member Endpoints
│   │   ├── Services/           # Logic for roles & memberships
│   │   └── Data/               # WorkspaceDbContext (Postgres)
│   └── test/                   # Comprehensive xUnit Tests
├── Frontend/
│   ├── src/
│   │   ├── app/                # Angular Components & Services
│   │   │   ├── auth/           # Login/Signup/OAuth logic
│   │   │   ├── workspaces/     # Workspace management UI
│   │   │   ├── services/       # API Clients (Angular Signals)
│   │   │   └── models/         # TypeScript Interfaces/DTOs
│   │   └── assets/             # Global tokens & design system
├── docker-compose.yml          # Root orchestration for the entire stack
└── README.md                   # Project documentation
```

---

## 💎 Core Design Principles

1.  **System Aesthetics**: We follow a "Modern System" design language—monochromatic, high contrast, and refined typography (Plus Jakarta Sans).
2.  **Stateless Identity**: Authentication is handled entirely via JWT, enabling seamless horizontal scaling of microservices.
3.  **Data Isolation**: Every microservice owns its data. No cross-service database queries are allowed; communication happens via APIs.
4.  **Reactive UI**: The frontend leverages Angular Signals for highly performant, reactive state updates.

---

## 🛠️ Technical Deep Dive

### JWT Authentication Flow
1.  **Identity Handshake**: User provides credentials to the Auth Service.
2.  **Token Issuance**: Auth Service validates and signs a JWT with user claims (Sub, Email, Name).
3.  **Client Persistence**: The Frontend stores the token in `localStorage`.
4.  **Automatic Header Injection**: An Angular `HttpInterceptor` automatically attaches the token as a `Bearer` header to all outgoing requests.
5.  **Cross-Service Validation**: Microservices (like Workspace) validate the token using a shared secret key to ensure the request is authorized without calling the Auth Service.

### Database Strategy
We use **PostgreSQL** with Entity Framework Core's **Code-First** approach.
-   **Migrations**: All database changes are tracked in migration files.
-   **Auto-Apply**: On startup, services check for pending migrations and apply them automatically, ensuring the environment is always up-to-date.
-   **Isolation**: Each service has its own schema and credentials to prevent data leakage.

---

## 🔧 Troubleshooting

### Docker Issues
-   **Containers not starting**: Check if the ports (5001, 5002, 4200, 5432, 5433) are already in use by other applications.
-   **Database Connection Failed**: Ensure the `postgres` and `postgres_workspace` containers are in a "Healthy" state before the backend starts.
-   **CORS Errors**: The backend is configured to allow `http://localhost:4200`. If you run the frontend on a different port, update `Cors:AllowedOrigins` in the `appsettings.json`.

### Frontend Issues
-   **npm install failures**: Ensure you are using Node.js v18 or later.
-   **API 404 Errors**: Double-check the `API_BASE` URLs in the frontend services to ensure they match your local or Docker environment.

---

## 🗺️ Roadmap

- [x] UC1: User Authentication (JWT)
- [x] UC2: User Profile Management
- [x] UC3: Board Creation & Management
- [x] UC4: Workspace Creation & Management
- [x] UC5: Workspace Member Invitation
- [x] UC6: Workspace Search (Member Discovery)
- [x] UC7: Board Member Management
- [ ] UC10: Task Lists & Card CRUD
- [ ] UC15: Activity Logging & Audit Trails

---

## 🧪 Testing

The backend includes a comprehensive suite of xUnit tests, covering both unit logic and full API integration.

### Running Tests
To run all backend tests across all services:
```powershell
cd Backend
dotnet test
```

### Coverage
- **UC1 (Auth)**: Comprehensive registration and login flow testing.
- **UC3 (Board)**: Verified board persistence, workspace association, and role-based access control.
- **Integration**: Full pipeline verification using InMemory database providers for Auth and Board services.

---

## 🤝 Contributing

1. Fork the Project.
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`).
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`).
4. Push to the Branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

*Developed with ❤️ by the FlowBoard Team. Built for performance, designed for speed.*
