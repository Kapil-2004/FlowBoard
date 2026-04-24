# ≋ FlowBoard – Modern Kanban Task Management

![Version](https://img.shields.io/badge/version-1.0.0--beta-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Tech](https://img.shields.io/badge/tech-Angular%20%7C%20.NET%208%20%7C%20Postgres-purple)

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
- [Use Cases](#-use-cases)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### Phase 1: Authentication & Identity (Current)
- **Secure Registration**: User signup with password hashing and validation.
- **JWT Authentication**: Token-based security for stateless API interaction.
- **Modern Dashboard**: Personalized landing page for authenticated users.
- **Responsive Design**: Fully functional across desktop and mobile devices.
- **Premium Aesthetics**: Monochromatic design system with Plus Jakarta Sans typography.

### Phase 2: Core Task Management (In Progress)
- **Workspaces**: Group boards by project or team.
- **Kanban Boards**: Drag-and-drop task management.
- **Lists & Cards**: Granular task organization.
- **Real-time Updates**: Instant synchronization across clients.

---

## 🛠️ Tech Stack

### Frontend
- **Framework**: Angular 17+ (Standalone Components)
- **Styling**: Vanilla CSS (Custom Design System)
- **State Management**: Signals & RXJS
- **Icons**: Custom SVG & Lucide Icons

### Backend
- **Framework**: ASP.NET Core 8.0 (Web API)
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core
- **Identity**: Custom JWT Implementation
- **Documentation**: Swagger / OpenAPI

### DevOps & Tools
- **Containerization**: Docker & Docker Compose
- **Version Control**: Git
- **Testing**: xUnit & Moq

---

## 🏗️ Architecture

FlowBoard follows a **Modular Monolith** approach on the backend, ensuring clean separation of concerns while maintaining simplicity for development.

- **Controllers**: Handle HTTP requests and routing.
- **Services**: Contain core business logic.
- **Repositories**: Abstract database interactions.
- **DTOs**: Data Transfer Objects for API contracts.
- **Middlewares**: Custom JWT and Error handling.

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
- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger

### Manual Setup (Development)

#### 1. Database
Start a PostgreSQL instance or use the provided docker-compose:
```bash
cd Backend/FlowBoard-Auth
docker-compose up -d postgres
```

#### 2. Backend
```bash
cd Backend/FlowBoard-Auth
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

Once the backend is running, you can explore the API using Swagger:
`http://localhost:5001/swagger/index.html`

### Key Endpoints
- `POST /api/auth/register`: Create a new account.
- `POST /api/auth/login`: Authenticate and receive a JWT.
- `GET /api/auth/profile`: Retrieve current user info (Secured).

---

## 📁 Project Structure

```text
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/       # .NET 8 Auth Microservice
│   │   ├── Controllers/      # API Endpoints
│   │   ├── Models/           # DB Entities
│   │   ├── Services/         # Business Logic
│   │   └── Data/             # EF Core Context
│   └── test/                 # xUnit Tests
├── Frontend/
│   ├── src/
│   │   ├── app/              # Angular Components
│   │   │   ├── auth/         # Login/Signup
│   │   │   ├── services/     # API Clients
│   │   │   └── dashboard/    # User Dashboard
│   │   └── assets/           # Global styles & images
├── docker-compose.yml        # Root orchestration
└── README.md                 # You are here
```

---

## 📋 Use Cases

1. **UC01: User Registration** - New users can create accounts with email verification logic.
2. **UC02: Secure Login** - Multi-factor ready JWT authentication.
3. **UC03: Dashboard Overview** - User-specific data visualization.
4. **UC04: Workspace Creation** - Organizing boards into logical groups.

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

*Developed with ❤️ by the FlowBoard Team.*
