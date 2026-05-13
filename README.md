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

### Phase 2: Workspace & Collaboration (Stable)
- **Workspaces**: Create and manage isolated project environments.
- **Member Management**: Search and invite users to workspaces via email/name.
- **Role-Based Access**: ADMIN and MEMBER roles for granular control.
- **Visibility Control**: Toggle between PUBLIC and PRIVATE workspaces.

### Phase 3: Board & Layout (Stable)
- **Board Management**: Create, rename, and theme Kanban boards.
- **Custom Themes**: Hex-based accent colors with glassmorphism backgrounds.
- **Real-time Status**: Track board activity and membership.

### Phase 4: List / Column Service (Active - UC4)
- **Dynamic Columns**: Create, rename, and delete Kanban lists/columns.
- **Atomic Reordering**: Drag-and-drop lists with persistent, transaction-safe position updates.
- **Archive System**: Soft-delete lists to an archive panel for later restoration.
- **Cross-Board Movement**: Seamlessly move columns between different boards.
- **Color Accents**: Set custom colors for individual lists to categorize workflows.

---

## 🛠️ Tech Stack

### Frontend
- **Framework**: Angular 17+ (Standalone Components)
- **State Management**: Signals & RxJS
- **Styling**: Vanilla CSS with a Custom Design System (CSS Variables)
- **Icons**: Material Icons & Custom SVG
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
- **Database Migrations**: Automated table creation & schema syncing on startup

---

## 🏗️ Architecture

FlowBoard utilizes a **Microservice Architecture** to ensure scalability and maintainability.

1.  **Auth Service (Port 5001)**: Handles identity, authentication, and user profiles.
2.  **Workspace Service (Port 5002)**: Manages workspaces, memberships, and roles.
3.  **Board Service (Port 5003)**: Manages Kanban boards and high-level board metadata.
4.  **List Service (Port 5004)**: Manages columns (lists) within boards, reordering, and archival.
5.  **Frontend (Port 4200)**: Angular SPA serving as the primary client interface.
6.  **Shared Databases**: Independent PostgreSQL instances for each microservice.

---

## 🚀 Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/)

### Quick Start (Docker)
The entire stack (Backend + Frontend + DB) can be launched with a single command:

```bash
docker-compose up --build
```
- **Frontend**: http://localhost:4200
- **Auth API**: http://localhost:5001/swagger
- **List Service API**: http://localhost:5004/swagger

### Manual Setup (Development)

#### 1. Databases
Start the PostgreSQL containers for development:
```bash
docker-compose up -d postgres postgres_workspace postgres_board postgres_list
```

#### 2. Backend Services
Open terminals for each service:
```bash
# Example for List Service
cd Backend/FlowBoard-ListService
dotnet run
```

---

## 📑 API Documentation

Each service provides its own Swagger UI:
- **Auth**: `http://localhost:5001/swagger`
- **Workspace**: `http://localhost:5002/swagger`
- **Board**: `http://localhost:5003/swagger`
- **List**: `http://localhost:5004/swagger`

### Key List Service Endpoints (UC4)
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/lists` | Create a new column on a board |
| `PUT` | `/api/lists/board/{id}/reorder` | Update positions of all columns atomically |
| `POST` | `/api/lists/{id}/archive` | Soft-delete a list |
| `PUT` | `/api/lists/{id}/move` | Move a list to a different board |

---

## 📁 Project Structure

```text
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/         # Identity Microservice (5001)
│   ├── FlowBoard-Workspace/    # Workspace Microservice (5002)
│   ├── FlowBoard-Board/        # Board Microservice (5003)
│   └── FlowBoard-ListService/  # Column/List Microservice (5004)
├── Frontend/
│   ├── src/app/
│   │   ├── auth/               # Identity UI
│   │   ├── workspaces/         # Workspace UI
│   │   ├── boards/             # Kanban & Board UI (UC4)
│   │   └── services/           # Signal-based API Clients
├── docker-compose.yml          # Infrastructure Orchestration
└── README.md                   # You are here
```

---

## 💎 Core Design Principles

1.  **System Aesthetics**: High-end monochromatic design language with glassmorphism effects.
2.  **Stateless Identity**: JWT-based authentication for seamless microservice scaling.
3.  **Data Isolation**: Every microservice owns its PostgreSQL instance.
4.  **Reactive UI**: Angular Signals for instantaneous UI updates without full re-renders.

---

## 🗺️ Roadmap

- [x] UC1: User Authentication (JWT)
- [x] UC2: User Profile Management
- [x] UC3: Board Creation & Management
- [x] UC4: List / Column CRUD & Reordering
- [x] UC5: List Archival & Movement
- [x] UC6: Workspace Search (Member Discovery)
- [ ] UC10: Task Cards & Assignment
- [ ] UC12: Labeling System
- [ ] UC15: Activity Logging

---

## 🧪 Testing

```powershell
cd Backend
dotnet test
```

---

## 📄 License
Distributed under the MIT License. See `LICENSE` for more information.

---
*Developed with ❤️ by the FlowBoard Team. Built for performance, designed for speed.*
