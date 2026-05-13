# ≋ FlowBoard – Modern Kanban Task Management

![Version](https://img.shields.io/badge/version-1.0.0--beta-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Tech](https://img.shields.io/badge/tech-Angular%20%7C%20.NET%208%20%7C%20Postgres-purple)
![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Platform](https://img.shields.io/badge/platform-Docker-blue)

FlowBoard is a high-performance, aesthetically pleasing Kanban-style task management application built with a modern tech stack. It features a robust .NET 8 microservice architecture for the backend and a sleek, responsive Angular 17+ standalone component architecture for the frontend. Designed for developers who value both performance and premium design.

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
- [Testing](#-testing)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### Phase 1: Authentication & Identity (Stable)
- **Secure Registration**: User signup with password hashing and validation.
- **JWT Authentication**: Token-based security for stateless API interaction.
- **OAuth Integration**: Support for Google and GitHub authentication.
- **Profile Management**: Update user details, avatars, and security settings.
- **Premium Aesthetics**: Monochromatic design system with Plus Jakarta Sans typography.

### Phase 2: Workspace & Collaboration (Stable)
- **Workspaces**: Create and manage isolated project environments for teams.
- **Member Management**: Search and invite users to workspaces via email or username.
- **Role-Based Access**: ADMIN and MEMBER roles for granular workspace control.
- **Visibility Control**: Toggle between PUBLIC and PRIVATE workspace modes.

### Phase 3: Board & Layout (Stable)
- **Board Management**: Create, rename, and theme multiple Kanban boards per workspace.
- **Custom Themes**: Hex-based accent colors with glassmorphism backgrounds.
- **Real-time Status**: Track board activity and membership presence.

### Phase 4: List / Column Service (Stable - UC4)
- **Dynamic Columns**: Create, rename, and delete Kanban lists/columns dynamically.
- **Atomic Reordering**: Drag-and-drop lists with persistent, transaction-safe position updates.
- **Archive System**: Soft-delete lists to an archive panel for later restoration.
- **Cross-Board Movement**: Seamlessly move columns between different boards.

### Phase 5: Card & Task Service (Active - UC5)
- **Task Management**: Create, edit, and delete task cards within lists.
- **Rich Card Metadata**: Support for titles, detailed descriptions, and due dates.
- **Assignee System**: Assign tasks to workspace members with real-time updates.
- **Vertical Reordering**: Drag-and-drop cards within or across columns.
- **Overdue Tracking**: Visual indicators and filtering for cards past their deadline.
- **Card Archival**: Dedicated workflow for completed or redundant tasks.

---

## 🛠️ Tech Stack

### Frontend
- **Framework**: Angular 17+ (Standalone Component Architecture)
- **State Management**: Signals (Reactive) & RxJS (Asynchronous streams)
- **Styling**: Vanilla CSS with a Custom Design System (CSS Variables & HSL Tokens)
- **Icons**: Material Icons & Hand-optimized SVG assets
- **Interceptors**: Automated JWT injection and error handling middleware

### Backend
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Database**: PostgreSQL 16 (Independent instances per microservice)
- **ORM**: Entity Framework Core (Code First with automated migrations)
- **Security**: BCrypt password hashing & JWT Bearer token validation
- **Documentation**: Swagger / OpenAPI 3.0 for interactive API testing

### DevOps & Infrastructure
- **Containerization**: Docker & Docker Compose orchestration
- **Networking**: Isolated Docker networks for secure service communication
- **Migrations**: Automated schema synchronization on service startup
- **Environment**: Centralized `.env` management for configuration

---

## 🏗️ Architecture

FlowBoard utilizes a **Microservice Architecture** to ensure high availability, scalability, and domain isolation.

1.  **Auth Service (Port 5001)**: Handles identity, authentication, and user profiles.
2.  **Workspace Service (Port 5002)**: Manages workspaces, memberships, and roles.
3.  **Board Service (Port 5003)**: Manages Kanban boards and high-level board metadata.
4.  **List Service (Port 5004)**: Manages columns (lists) within boards and reordering logic.
5.  **Card Service (Port 5005)**: Manages task cards, assignments, and task lifecycles.
6.  **Frontend (Port 4200)**: Angular SPA serving as the unified user interface.
7.  **Data Layer**: Five independent PostgreSQL instances ensuring data sovereignty.

---

## 🚀 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Recommended)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/)
- [PostgreSQL Client](https://www.pgadmin.org/) (Optional, for DB inspection)

### Quick Start (Docker)
The entire stack (Backend + Frontend + DB) can be launched with a single command from the root directory:

```bash
docker-compose up --build
```
Once the containers are healthy:
- **Frontend UI**: [http://localhost:4200](http://localhost:4200)
- **Auth API**: [http://localhost:5001/swagger](http://localhost:5001/swagger)
- **Card Service API**: [http://localhost:5005/swagger](http://localhost:5005/swagger)

### Manual Setup (Development)

#### 1. Databases
Start the PostgreSQL containers only:
```bash
docker-compose up -d postgres postgres_workspace postgres_board postgres_list postgres_card
```

#### 2. Backend Services
Open terminals for the desired service:
```bash
cd Backend/FlowBoard-CardService
dotnet run
```

#### 3. Frontend Client
```bash
cd Frontend
npm install
npm start
```

---

## 📑 API Documentation

Each microservice provides a self-documenting Swagger UI:
- **Auth**: `http://localhost:5001/swagger`
- **Workspace**: `http://localhost:5002/swagger`
- **Board**: `http://localhost:5003/swagger`
- **List**: `http://localhost:5004/swagger`
- **Card**: `http://localhost:5005/swagger`

### Key Card Service Endpoints (UC5)

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/cards` | Create a new task card |
| `GET` | `/api/cards/list/{listId}` | Fetch all cards for a specific column |
| `PUT` | `/api/cards/{id}` | Update card details (title, description, etc.) |
| `PUT` | `/api/cards/{id}/move` | Move a card to a different list or position |
| `PUT` | `/api/cards/list/{listId}/reorder` | Update card sequence within a list |
| `PUT` | `/api/cards/{id}/assignee` | Assign/unassign a user to a task |
| `POST` | `/api/cards/{id}/archive` | Move card to archive |

---

## 📁 Project Structure

```text
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/         # Identity Microservice (5001)
│   ├── FlowBoard-Workspace/    # Workspace Microservice (5002)
│   ├── FlowBoard-Board/        # Board Microservice (5003)
│   ├── FlowBoard-ListService/  # Column/List Microservice (5004)
│   └── FlowBoard-CardService/  # Task/Card Microservice (5005)
├── Frontend/
│   ├── src/app/
│   │   ├── auth/               # Auth UI & Logic
│   │   ├── workspaces/         # Workspace Management UI
│   │   ├── boards/             # Kanban Board & List UI
│   │   ├── cards/              # Card Modals & Task UI
│   │   └── services/           # Signal-based API Clients
├── docker-compose.yml          # Infrastructure Orchestration
└── README.md                   # Project Documentation
```

---

## 💎 Core Design Principles

1.  **System Aesthetics**: High-end monochromatic design language with glassmorphism and subtle micro-animations.
2.  **Stateless Identity**: JWT-based authentication for seamless horizontal scaling across microservices.
3.  **Data Isolation**: Strict Database-per-Service pattern to prevent tight coupling and ensure reliability.
4.  **Reactive UI**: Angular Signals enable instantaneous UI updates without the overhead of full re-renders.
5.  **Atomic Transactions**: Critical operations like reordering use atomic updates to ensure data consistency.

---

## 🗺️ Roadmap

- [x] UC1: User Authentication (JWT & OAuth)
- [x] UC2: User Profile Management
- [x] UC3: Workspace & Member Discovery
- [x] UC4: Kanban Board & List Management
- [x] UC5: Task Card Lifecycle (CRUD, Assignment)
- [x] UC6: Drag-and-Drop Reordering (Lists & Cards)
- [ ] UC12: Labeling & Tagging System
- [ ] UC15: Activity Logging & Notifications
- [ ] UC20: Real-time Collaboration (WebSockets)

---

## 🧪 Testing

The solution includes comprehensive unit and integration tests across all microservices.

```powershell
# Run all tests in the solution
cd Backend
dotnet test

# Run frontend tests
cd Frontend
ng test --watch=false
```

---

## 🛠️ Troubleshooting

- **Database Connection Refused**: Ensure Docker containers are running. If running locally, check if ports (5432-5436) are occupied.
- **JWT Unauthorized**: Ensure the `Jwt:Secret` matches across all microservices in `appsettings.json` or `docker-compose.yml`.
- **CORS Issues**: Check the `Cors:AllowedOrigins` setting in the service configuration; it must include `http://localhost:4200`.
- **Node Modules Error**: Run `npm cache clean --force` followed by `npm install`.

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---
*Developed with ❤️ by the FlowBoard Team. Built for performance, designed for speed.*

