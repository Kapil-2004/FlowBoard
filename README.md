# ≋ FlowBoard — Modern Kanban Task Management

<div align="center">

![Version](https://img.shields.io/badge/version-1.0.0--beta-blue?style=for-the-badge)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)
![Docker](https://img.shields.io/badge/docker-ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-17+-DD0031?style=for-the-badge&logo=angular)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)

**A high-performance, beautifully designed Kanban task management platform built on a .NET 8 microservice architecture with an Angular 17+ reactive frontend.**

[Features](#-features) · [Architecture](#-architecture) · [Quick Start](#-quick-start) · [API Docs](#-api-documentation) · [Testing](#-testing)

</div>

---

## ✨ Features

### UC1 — User Authentication & Identity
- **Secure Registration** with BCrypt password hashing and strong validation
- **JWT Authentication** — stateless, scalable, secure Bearer tokens (24h expiry)
- **OAuth2 Integration** — Google and GitHub social login support
- **Token Refresh** — automatic session management with expiry handling
- **Premium Auth UI** — monochromatic glassmorphism design with micro-animations

### UC2 — User Profile Management
- Update display name, avatar URL, and account details
- Secure password change with current password verification
- Account deactivation and data management

### UC3 — Workspace & Member Management
- **Workspace CRUD** — create, rename, and manage isolated project environments
- **Member Invitations** — search users by email or username and invite them
- **Role-Based Access** — `ADMIN` and `MEMBER` roles with granular permission gates
- **Visibility Modes** — `PUBLIC` and `PRIVATE` workspace switching

### UC4 — Kanban Board & List/Column Service
- **Board Management** — create, theme, and manage boards per workspace
- **Custom Themes** — hex-based accent colors per board with live preview
- **Dynamic Columns** — create, rename, delete, and reorder Kanban list columns
- **Column Drag-and-Drop** — atomic position updates persisted to the database
- **Archive & Restore** — soft-delete columns to an archive panel
- **Cross-Board Movement** — move entire columns between boards seamlessly

### UC5 — Card & Task Service
- **Task Cards** — full CRUD (create, view, edit, delete) with rich metadata
- **Priority Levels** — `LOW`, `MEDIUM`, `HIGH`, `CRITICAL` with visual badges
- **Status Tracking** — `TO_DO`, `IN_PROGRESS`, `IN_REVIEW`, `DONE` workflow
- **Descriptions** — rich text descriptions on each card
- **Card Drag-and-Drop** — move cards within or across columns with optimistic UI
- **Assignee System** — assign tasks to workspace members
- **Overdue Tracking** — identify and surface cards past their deadline
- **Card Archival** — archive completed or redundant tasks

### UC8 — Notification Service *(Real-time Alerts)*
- **5 Notification Types** — `ASSIGNMENT`, `MENTION`, `DUE_DATE`, `COMMENT`, `MOVE`
- **In-App Badge** — live unread count pushed via **SignalR WebSocket** (no polling)
- **Notification Panel** — per-type icon + accent colour, mark-as-read, bulk clear, deep-link metadata
- **Bulk Broadcast** — admin `SendBulk` endpoint delivers system announcements to multiple users
- **Email Dispatch** — **SendGrid** SDK sends formatted HTML emails for critical events
- **Event-Driven** — `MassTransit` consumer on `notification-events` RabbitMQ queue picks up events from Comment & Card services
- **Full Lifecycle** — `MarkAsRead`, `MarkAllRead`, `DeleteRead`, `DeleteNotification`, `GetAll` (admin)
- **Deep-Linking** — `RelatedId` + `RelatedType` (`CARD`/`BOARD`) for direct navigation

- **Board-Scoped Labels** — create, edit, and delete color-coded labels per board
- **Multi-Label Cards** — assign multiple labels to a single card for categorization
- **Mini Label Badges** — visual label indicators on Kanban cards for quick scanning
- **Dynamic Checklists** — add multiple checklists per card with custom titles
- **Checklist Items** — add, toggle, and delete items within a checklist
- **Progress Tracking** — automatic calculation of completion percentage per card
- **Clean Modal Integration** — seamless labels and checklist management in the card detail modal

### UC6 — Comment & Attachment Service *(Collaboration)*
- **Threaded Comments** — two-level threading via `ParentCommentId` (nullable for top-level)
- **Soft-Delete Moderation** — `IsDeleted` flag via EF Core query filters preserves history
- **File Attachments** — link files (Azure Blob / AWS S3 ready) to cards with metadata
- **Attachment Metadata** — file type, size (KB), uploader, and upload timestamp recorded
- **Card Detail Modal** — full-featured slide-up modal with description, comments, attachments, status, priority, and delete
- **Event-Driven Notifications** — every comment/attachment triggers a `MassTransit` + `RabbitMQ` event to alert card watchers and `@mentioned` users

---

## 🏗️ Architecture

FlowBoard follows a **Database-per-Service** microservice pattern with event-driven cross-service communication.

```
┌─────────────────────────────────────────────────────────┐
│                    Angular Frontend                      │
│              (Port 4200 · Standalone Components)        │
└──────────────┬──────────────────────────────────────────┘
               │  HTTP (JWT Bearer)
┌──────────────▼──────────────────────────────────────────┐
│                  Backend Microservices                   │
│                                                         │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │ Auth Service│  │  Workspace   │  │ Board Service │  │
│  │  Port 5001  │  │  Port 5002   │  │  Port 5003    │  │
│  └──────┬──────┘  └──────┬───────┘  └───────┬───────┘  │
│         │                │                  │           │
│  ┌──────▼──────┐  ┌──────▼───────┐  ┌───────▼───────┐  │
│  │ PostgreSQL  │  │  PostgreSQL  │  │  PostgreSQL   │  │
│  │  Port 5432  │  │  Port 5433   │  │  Port 5434    │  │
│  └─────────────┘  └──────────────┘  └───────────────┘  │
│                                                         │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │ List Service│  │ Card Service │  │Comment Service│  │
│  │  Port 5004  │  │  Port 5005   │  │  Port 5006    │  │
│  └──────┬──────┘  └──────┬───────┘  └───────┬───────┘  │
│         │                │                  │           │
│  ┌──────▼──────┐  ┌──────▼───────┐  ┌───────▼───────┐  │
│  │ PostgreSQL  │  │  PostgreSQL  │  │  PostgreSQL   │  │
│  │  Port 5435  │  │  Port 5436   │  │  Port 5437    │  │
│  └─────────────┘  └──────────────┘  └───────┬───────┘  │
│                                              │           │
│  ┌─────────────┐  ┌──────────────┐  ┌────────▼───────┐  │
│  │Label Service│  │  PostgreSQL  │  │   RabbitMQ    │  │
│  │  Port 5007  │  │  Port 5438   │  │  Port 5672    │  │
│  └──────┬──────┘  └──────────────┘  └───────────────┘  │
│         │                                               │
└─────────┼───────────────────────────────────────────────┘
          │
          ▼
    [Card Enrichment]
```

### Service Map

| Service | Port | Database Port | Description |
|---|---|---|---|
| Auth Service | `5001` | `5432` | Identity, JWT, OAuth2 |
| Workspace Service | `5002` | `5433` | Workspaces, members, roles |
| Board Service | `5003` | `5434` | Kanban boards, themes |
| List Service | `5004` | `5435` | Columns, ordering, archive |
| Card Service | `5005` | `5436` | Tasks, assignments, lifecycle |
| Comment & Attachment Service | `5006` | `5437` | Collaboration, file links |
| Label & Checklist Service | `5007` | `5438` | Labels, checklists, progress |
| Notification Service | `5008` | `5439` | In-app alerts, email, SignalR |
| Frontend (Angular SPA) | `4200` | — | Unified UI client |
| RabbitMQ Management UI | `15672` | — | Message broker dashboard |

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Frontend Framework** | Angular 17+ (Standalone Component Architecture) |
| **State Management** | Angular Signals + RxJS |
| **Styling** | Vanilla CSS (Custom Design System, CSS Variables, HSL Tokens) |
| **HTTP Client** | Angular `HttpClient` with JWT interceptor |
| **Backend Framework** | ASP.NET Core 8.0 Web API |
| **ORM** | Entity Framework Core 8 (Code-First, auto migrations) |
| **Database** | PostgreSQL 16 (independent instance per service) |
| **Authentication** | BCrypt + JWT Bearer Tokens |
| **Message Broker** | RabbitMQ via MassTransit |
| **API Documentation** | Swagger / OpenAPI 3.0 |
| **Containerization** | Docker + Docker Compose |
| **Testing** | xUnit + Moq (.NET) |

---

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v4.0+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) *(for local dev only)*
- [Node.js 18+](https://nodejs.org/) *(for local dev only)*

### Option A — Docker (Recommended)

Launch the entire stack with a single command:

```bash
git clone https://github.com/your-username/flowboard.git
cd flowboard
docker-compose up --build
```

Once healthy, open:

| Service | URL |
|---|---|
| **FlowBoard App** | http://localhost:4200 |
| **Auth API Swagger** | http://localhost:5001/swagger |
| **Card API Swagger** | http://localhost:5005/swagger |
| **Label API Swagger** | http://localhost:5007/swagger |
| **Notification API Swagger** | http://localhost:5008/swagger |
| **Comment API Swagger** | http://localhost:5006/swagger |
| **RabbitMQ Dashboard** | http://localhost:15672 *(guest / guest)* |
| **Notification Hub (WS)** | ws://localhost:5008/hubs/notifications?userId={id} |

> **Note:** First-time build takes ~3–5 minutes. Subsequent starts are instant.

### Option B — Local Development

**Step 1** — Start infrastructure containers:
```bash
docker-compose up -d postgres postgres_workspace postgres_board postgres_list postgres_card postgres_comment postgres_label postgres_notification rabbitmq
```

**Step 2** — Run backend services (each in a separate terminal):
```bash
# Auth Service
cd Backend/FlowBoard-Auth && dotnet run

# Comment & Attachment Service
cd Backend/FlowBoard-Comment_AttachmentService && dotnet run

# (repeat for other services as needed)
```

**Step 3** — Run the Angular frontend:
```bash
cd Frontend
npm install
npm start
```

The app will be available at http://localhost:4200 with hot-reload enabled.

---

## 📑 API Documentation

Every service exposes an interactive Swagger UI in `Development` mode:

| Service | Swagger URL |
|---|---|
| Auth | http://localhost:5001/swagger |
| Workspace | http://localhost:5002/swagger |
| Board | http://localhost:5003/swagger |
| List | http://localhost:5004/swagger |
| Card | http://localhost:5005/swagger |
| Comment & Attachment | http://localhost:5006/swagger |
| Label & Checklist | http://localhost:5007/swagger |
| Notification | http://localhost:5008/swagger |

### Core Endpoints

#### Auth Service (UC1)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Login and receive JWT |
| `GET` | `/api/auth/me` | Get current user profile |

#### Comment & Attachment Service (UC6)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/comments` | Add a new comment (or reply) |
| `GET` | `/api/comments/card/{cardId}` | Get all comments for a card |
| `GET` | `/api/comments/{id}/replies` | Get threaded replies |
| `PUT` | `/api/comments/{id}` | Edit a comment |
| `DELETE` | `/api/comments/{id}` | Soft-delete a comment |
| `GET` | `/api/comments/card/{cardId}/count` | Get comment count |
| `POST` | `/api/attachments` | Link a file attachment |
| `GET` | `/api/attachments/card/{cardId}` | Get attachments for a card |
| `DELETE` | `/api/attachments/{id}` | Delete an attachment |

#### Label & Checklist Service (UC7)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/label` | Create a board label |
| `GET` | `/api/label/board/{boardId}` | Get labels for a board |
| `POST` | `/api/label/card/{cardId}/label/{labelId}` | Add label to card |
| `DELETE` | `/api/label/card/{cardId}/label/{labelId}` | Remove label from card |
| `POST` | `/api/checklist` | Create a checklist on a card |
| `POST` | `/api/checklist/{id}/item` | Add item to checklist |
| `PUT` | `/api/checklist/item/{itemId}/toggle` | Toggle item completion |
| `GET` | `/api/checklist/card/{cardId}/progress` | Get completion percentage |

#### Notification Service (UC8)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/notifications/recipient/{id}` | Get all notifications for a user |
| `GET` | `/api/notifications/unread-count/{id}` | Get unread badge count |
| `PUT` | `/api/notifications/{id}/read` | Mark a notification as read |
| `PUT` | `/api/notifications/recipient/{id}/read-all` | Mark all as read |
| `DELETE` | `/api/notifications/{id}` | Delete a notification |
| `DELETE` | `/api/notifications/recipient/{id}/read` | Delete all read (housekeeping) |
| `POST` | `/api/notifications/send` | Dispatch a single notification |
| `POST` | `/api/notifications/bulk` | Broadcast to multiple recipients |
| `GET` | `/api/notifications/all` | Get all notifications (admin) |

#### Card Service (UC5)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/cards` | Create a task card |
| `GET` | `/api/cards/list/{listId}` | Get cards in a column |
| `GET` | `/api/cards/board/{boardId}` | Get all cards on a board |
| `PUT` | `/api/cards/{id}` | Update card (title, desc, priority, status) |
| `PUT` | `/api/cards/{id}/move` | Move card to different list/position |
| `PUT` | `/api/cards/list/{listId}/reorder` | Reorder cards within a list |
| `POST` | `/api/cards/{id}/archive` | Archive a card |
| `DELETE` | `/api/cards/{id}` | Delete a card |

---

## 📁 Project Structure

```
FlowBoard/
├── Backend/
│   ├── FlowBoard-Auth/                    # UC1 — Identity & JWT
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   └── Dockerfile
│   ├── FlowBoard-Workspace/               # UC2/UC3 — Workspace & Members
│   ├── FlowBoard-Board/                   # UC4 — Board Management
│   ├── FlowBoard-ListService/             # UC4 — Column/List Service
│   ├── FlowBoard-CardService/             # UC5 — Task/Card Service
│   ├── FlowBoard-Comment_AttachmentService/ # UC6 — Comments & Attachments
│   │   ├── ...
│   │   └── Dockerfile
│   ├── FlowBoard-LabelService/            # UC7 — Labels & Checklists
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   └── Dockerfile
│   ├── FlowBoard-NotificationService/     # UC8 — Notifications
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Hubs/                          # SignalR NotificationHub
│   │   ├── Consumers/                     # MassTransit NotificationEventConsumer
│   │   ├── DTOs/
│   │   ├── Migrations/
│   │   └── Dockerfile
│   └── test/
│       ├── FlowBoard-CommentService.Tests/
│       ├── FlowBoard-CardService.Tests/
│       ├── FlowBoard-LabelService.Tests/
│       ├── FlowBoard-NotificationService.Tests/ # UC8 unit tests
│       ├── auth-tests/
│       ├── board-tests/
│       ├── list-tests/
│       └── workspace-tests/
├── Frontend/
│   └── src/app/
│       ├── auth/                          # Login, Signup UI
│       ├── dashboard.component.ts         # Workspace dashboard
│       ├── boards/
│       │   ├── board-list/                # Board grid view
│       │   └── board-detail/              # Full Kanban board + card modal
│       ├── services/
│       │   ├── auth.service.ts
│       │   ├── board.service.ts
│       │   ├── list.service.ts
│       │   ├── card.service.ts
│       │   ├── comment.service.ts
│       │   ├── label.service.ts
│       │   └── in-app-notification.service.ts # UC8 notification API + signal state
│       ├── notifications/
│       │   └── notification-bell/         # UC8 bell + panel standalone component
│       └── models/
│           ├── auth.models.ts
│           ├── board.models.ts
│           ├── card.models.ts
│           ├── comment.models.ts
│           ├── label.models.ts
│           └── notification.models.ts     # UC8 AppNotification, NOTIFICATION_META
├── docker-compose.yml                     # Full stack orchestration
├── Sprint.sln                             # .NET solution file
└── README.md
```

---

## 🧪 Testing

The solution includes unit and integration tests for all microservices.

```bash
# Run all .NET tests
cd Backend
dotnet test

# Run a specific service's tests
dotnet test test/FlowBoard-CommentService.Tests

# Run Angular unit tests
cd Frontend
ng test --watch=false --browsers=ChromeHeadless
```

### Test Coverage

| Service | Test Project | Framework |
|---|---|---|
| Auth Service | `auth-tests` | xUnit + Moq |
| Workspace Service | `workspace-tests` | xUnit + Moq |
| Board Service | `board-tests` | xUnit + Moq |
| List Service | `list-tests` | xUnit + Moq |
| Card Service | `FlowBoard-CardService.Tests` | xUnit + Moq |
| Comment & Attachment | `FlowBoard-CommentService.Tests` | xUnit + Moq |
| Label & Checklist | `FlowBoard-LabelService.Tests` | xUnit + Moq |
| Notification | `FlowBoard-NotificationService.Tests` | xUnit + Moq (8 tests) |

---

## 💎 Design Principles

1. **Database-per-Service** — complete data isolation, no shared databases between microservices
2. **Stateless Identity** — JWT Bearer tokens enable horizontal scaling across all services
3. **Event-Driven Collaboration** — MassTransit + RabbitMQ decouples notification logic
4. **Reactive UI** — Angular Signals provide zero-boilerplate reactive state with instant UI updates
5. **Atomic Transactions** — reordering operations use atomic updates for guaranteed consistency
6. **Soft Deletes** — EF Core global query filters on `IsDeleted` preserve audit trails for moderation
7. **Optimistic UI** — drag-and-drop updates the UI instantly, rolling back only on API failure
8. **System Aesthetics** — monochromatic design system with glassmorphism, micro-animations, and premium typography (Plus Jakarta Sans)

---

## 🗺️ Roadmap

- [x] **UC1** — User Authentication (JWT + OAuth2)
- [x] **UC2** — User Profile Management
- [x] **UC3** — Workspace & Member Discovery
- [x] **UC4** — Kanban Board, List/Column Management, Archive, Drag-and-Drop
- [x] **UC5** — Task Card Lifecycle (CRUD, Assignments, Priority, Status)
- [x] **UC6** — Comments & Attachments (Threaded, Soft-delete, Event-driven)
- [x] **UC7** — Card Labels & Checklist System (Progress tracking)
- [x] **UC8** — Notification Service (SignalR real-time badge, SendGrid email, MassTransit events)
- [ ] **UC9** — Activity Log & Audit Trail
- [ ] **UC10** — Due Date Reminders (Quartz.NET scheduler)
- [ ] **UC11** — Board Templates
- [ ] **UC12** — Search & Filter (full-text across cards and comments)

---

## 🛠️ Troubleshooting

| Problem | Solution |
|---|---|
| **Port already in use** | Run `docker-compose down` then `docker-compose up --build` |
| **Database connection refused** | Ensure Docker containers are running: `docker ps` |
| **JWT Unauthorized (401)** | Verify `Jwt__Secret` matches across all services in `docker-compose.yml` |
| **CORS errors in browser** | Check `Cors__AllowedOrigins` includes `http://localhost:4200` |
| **Frontend not reflecting code changes** | Run `docker-compose build web-client && docker-compose up -d web-client` |
| **RabbitMQ connection error** | Wait 30s after startup; RabbitMQ takes time to become healthy |
| **npm install errors** | Run `npm cache clean --force` then `npm install` |

---

## 🤝 Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for development setup, branching strategy, and code standards.

---

## 📄 License

Distributed under the **MIT License**. See [LICENSE](./LICENSE) for details.

---

<div align="center">
  <i>Built with ❤️ — Designed for speed, built for scale.</i>
</div>
