# ≋ FlowBoard — Enterprise Kanban Task Management Platform

<div align="center">

![Version](https://img.shields.io/badge/version-1.0.0--beta-blue?style=for-the-badge)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)
![Docker](https://img.shields.io/badge/docker-ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-17+-DD0031?style=for-the-badge&logo=angular)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)

A high-performance, beautifully designed Kanban task management platform built on a **.NET 8 microservice architecture** with **Angular 17+ reactive frontend**. Featuring real-time collaboration, event-driven notifications, and a modern glassmorphism UI.

[Use Cases](#-use-cases) · [Features](#-features) · [Architecture](#-architecture) · [Quick Start](#-quick-start) · [API](#-api-endpoints)

</div>

---

## 📋 Use Cases

### **UC1 — User Authentication & Identity**
- Secure user registration with BCrypt password hashing and strength validation
- JWT Bearer token authentication (24-hour expiry) with automatic refresh handling
- OAuth2 social login integration (Google, GitHub)
- Premium auth UI with glassmorphism design and micro-animations
- Stateless identity architecture for horizontal scaling

### **UC2 — User Profile Management**
- Edit display name, avatar URL, and account preferences
- Secure password change with current password verification
- Account status management and data privacy controls

### **UC3 — Workspace & Member Management**
- Create, rename, and manage isolated workspace environments
- Member discovery and email-based invitations
- Role-based access control (`ADMIN`, `MEMBER`) with granular permissions
- Public/Private workspace visibility modes
- Member deactivation and role assignment

### **UC4 — Kanban Board & List/Column Service**
- Create and manage multiple Kanban boards per workspace
- Custom board themes with hex-based accent colors and live preview
- Dynamic column creation, renaming, deletion, and reordering
- Atomic drag-and-drop reordering persisted to database
- Column archival and restoration (soft-delete with recovery)
- Cross-board column movement

### **UC5 — Card & Task Service**
- Full task card lifecycle (CRUD operations)
- Priority levels: `LOW`, `MEDIUM`, `HIGH`, `CRITICAL` with visual indicators
- Task status workflow: `TO_DO`, `IN_PROGRESS`, `IN_REVIEW`, `DONE`
- Rich text descriptions and metadata management
- Card assignment to workspace members
- Optimistic drag-and-drop within or across columns
- Overdue card tracking and alerts
- Card archival and soft deletion

### **UC6 — Comment & Attachment Service (Team Collaboration)**
- Two-level threaded comments via parent-child relationships
- Soft-delete moderation (preserves history with `IsDeleted` flag)
- File attachment linking (Azure Blob Storage / AWS S3 ready)
- Attachment metadata: file type, size (KB), uploader, timestamp
- Event-driven notifications on comments/attachments
- @mention support for team member notifications
- Seamless card detail modal with full collaboration context

### **UC7 — Labels & Checklist Service**
- Board-scoped color-coded labels for categorization
- Multi-label card assignment
- Visual label mini-badges on Kanban cards
- Dynamic checklists per card with custom titles
- Checklist item toggle and deletion
- Automatic completion percentage tracking per card
- Progress visualization in card previews

### **UC8 — Real-Time Notification Service**
- Five notification types: `ASSIGNMENT`, `MENTION`, `DUE_DATE`, `COMMENT`, `MOVE`
- Live unread badge counter via **SignalR WebSocket** (zero polling)
- Notification panel with type-specific icons, accent colors, and actions
- Mark single/bulk notifications as read
- Deep-linking to related cards/boards
- Admin bulk broadcast functionality for system announcements
- Email dispatch via **SendGrid** for critical notifications
- Event-driven consumer using **MassTransit** + **RabbitMQ**
- Lifecycle management: read, delete, archive operations

## 🏗️ Architecture

FlowBoard follows a **Database-per-Service** microservice pattern with event-driven inter-service communication via RabbitMQ.

```
Frontend (Angular 4200) ──HTTP/JWT──▶ 8 Microservices (Ports 5001–5008)
                                      ├─ Auth (5001) ──▶ PostgreSQL (5432)
                                      ├─ Workspace (5002) ──▶ PostgreSQL (5433)
                                      ├─ Board (5003) ──▶ PostgreSQL (5434)
                                      ├─ List (5004) ──▶ PostgreSQL (5435)
                                      ├─ Card (5005) ──▶ PostgreSQL (5436)
                                      ├─ Comment (5006) ──▶ PostgreSQL (5437)
                                      ├─ Label (5007) ──▶ PostgreSQL (5438)
                                      └─ Notification (5008) ──▶ PostgreSQL (5439)
                                                ▲
                                         RabbitMQ (5672)
                                      MassTransit Events
```

| Service | Port | Database | Purpose |
|---------|------|----------|---------|
| **Auth Service** | 5001 | 5432 | JWT, OAuth2, user identity |
| **Workspace Service** | 5002 | 5433 | Workspace CRUD, member roles |
| **Board Service** | 5003 | 5434 | Kanban boards, themes |
| **List Service** | 5004 | 5435 | Columns, ordering, archive |
| **Card Service** | 5005 | 5436 | Tasks, assignments, lifecycle |
| **Comment & Attachment** | 5006 | 5437 | Collaboration, files, threads |
| **Label & Checklist** | 5007 | 5438 | Labels, checklists, progress |
| **Notification Service** | 5008 | 5439 | Real-time alerts, email, SignalR |

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Angular 17+ (Standalone Components, Signals, RxJS) |
| **UI Design** | Vanilla CSS (Custom Design System, Glassmorphism, HSL Tokens) |
| **Backend Framework** | ASP.NET Core 8.0 Web API |
| **Database** | PostgreSQL 16 (database-per-service) |
| **ORM** | Entity Framework Core 8 (Code-First, Migrations) |
| **Authentication** | JWT Bearer + BCrypt + OAuth2 |
| **Message Broker** | RabbitMQ via MassTransit |
| **Real-Time** | SignalR WebSocket |
| **Email** | SendGrid SDK |
| **Containerization** | Docker + Docker Compose |
| **Testing** | xUnit + Moq (.NET) |
| **API Docs** | Swagger / OpenAPI 3.0 |

---

## 🚀 Quick Start

### Prerequisites
- Docker Desktop (v4.0+)
- .NET 8 SDK *(local dev only)*
- Node.js 18+ *(local dev only)*

### Docker (Recommended)
```bash
git clone https://github.com/your-username/flowboard.git
cd flowboard
docker-compose up --build
```

Access the stack:
| Service | URL |
|---------|-----|
| **App** | http://localhost:4200 |
| **Auth API** | http://localhost:5001/swagger |
| **Card API** | http://localhost:5005/swagger |
| **Notification WS** | ws://localhost:5008/hubs/notifications |
| **RabbitMQ Dashboard** | http://localhost:15672 *(guest/guest)* |

### Local Development
```bash
# Start infrastructure
docker-compose up -d postgres rabbitmq

# Run services (separate terminals)
cd Backend/FlowBoard-Auth && dotnet run
cd Backend/FlowBoard-CardService && dotnet run
# ... other services

# Run frontend
cd Frontend && npm install && npm start
```

---

## 📡 Core API Endpoints

### **Auth (UC1)**
- `POST /api/auth/register` — Register new user
- `POST /api/auth/login` — Login & get JWT
- `GET /api/auth/me` — Current user profile

### **Card Service (UC5)**
- `POST /api/cards` — Create task
- `PUT /api/cards/{id}` — Update card details
- `PUT /api/cards/{id}/move` — Move card between columns
- `GET /api/cards/board/{boardId}` — Get board cards

### **Comment & Attachment (UC6)**
- `POST /api/comments` — Add comment or reply
- `GET /api/comments/card/{cardId}` — Get comments thread
- `POST /api/attachments` — Link file attachment
- `GET /api/attachments/card/{cardId}` — Get attachments

### **Label & Checklist (UC7)**
- `POST /api/label` — Create board label
- `POST /api/label/card/{cardId}/label/{labelId}` — Add label to card
- `POST /api/checklist` — Create checklist
- `GET /api/checklist/card/{cardId}/progress` — Get completion %

### **Notification (UC8)**
- `GET /api/notifications/recipient/{id}` — Get user notifications
- `PUT /api/notifications/{id}/read` — Mark as read
- `DELETE /api/notifications/{id}` — Delete notification
- `POST /api/notifications/bulk` — Broadcast to multiple users

---

## 📁 Project Structure

```
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/              # UC1 — Auth & Identity
│   ├── FlowBoard-Workspace/         # UC3 — Workspace & Members
│   ├── FlowBoard-Board/             # UC4 — Boards
│   ├── FlowBoard-ListService/       # UC4 — Columns
│   ├── FlowBoard-CardService/       # UC5 — Cards/Tasks
│   ├── FlowBoard-Comment_AttachmentService/ # UC6 — Collaboration
│   ├── FlowBoard-LabelService/      # UC7 — Labels & Checklists
│   ├── FlowBoard-NotificationService/ # UC8 — Real-time Alerts
│   └── test/                        # Unit tests (xUnit + Moq)
├── Frontend/
│   └── src/app/                     # Angular components & services
├── docker-compose.yml               # Full stack orchestration
└── README.md
```

---

## 🧪 Testing

```bash
# Run all .NET tests
cd Backend && dotnet test

# Run Angular tests
cd Frontend && ng test --watch=false

# Test coverage for all services
dotnet test test/FlowBoard-CommentService.Tests
dotnet test test/FlowBoard-CardService.Tests
dotnet test test/FlowBoard-LabelService.Tests
dotnet test test/FlowBoard-NotificationService.Tests
```

---

## 💡 Design Principles

1. **Database-per-Service** — Complete data isolation and autonomous scaling
2. **Stateless Authentication** — JWT enables horizontal load balancing
3. **Event-Driven Architecture** — MassTransit + RabbitMQ for loose coupling
4. **Reactive UI** — Angular Signals for zero-boilerplate reactive state
5. **Atomic Operations** — Database transactions guarantee reordering consistency
6. **Soft Deletes** — EF Core query filters preserve audit history
7. **Optimistic Updates** — Instant UI feedback with automatic rollback on errors
8. **Modern Aesthetics** — Glassmorphism design, micro-animations, premium typography

---

## 🗺️ Roadmap

- [x] UC1 — User Authentication (JWT + OAuth2)
- [x] UC2 — User Profile Management
- [x] UC3 — Workspace & Members
- [x] UC4 — Kanban Boards & Columns
- [x] UC5 — Task Cards & Assignments
- [x] UC6 — Comments & Attachments
- [x] UC7 — Labels & Checklists
- [x] UC8 — Real-time Notifications
- [ ] UC9 — Activity Log & Audit Trail
- [ ] UC10 — Due Date Reminders (Quartz.NET)
- [ ] UC11 — Board Templates
- [ ] UC12 — Advanced Search & Filters

---

## 🛠️ Troubleshooting

| Issue | Solution |
|-------|----------|
| **Port already in use** | `docker-compose down` then `docker-compose up --build` |
| **JWT Unauthorized (401)** | Verify `Jwt__Secret` matches across services |
| **CORS errors** | Check `Cors__AllowedOrigins` includes `http://localhost:4200` |
| **RabbitMQ connection error** | Wait 30s; RabbitMQ takes time to initialize |
| **Frontend changes not reflected** | Run `docker-compose build` and restart |

---

## 🤝 Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for development guidelines and code standards.

---

## 📄 License

Distributed under the **MIT License**. See [LICENSE](./LICENSE) for details.

---

<div align="center">
  <i>Built with ❤️ — Designed for speed, built for scale.</i>
</div>
