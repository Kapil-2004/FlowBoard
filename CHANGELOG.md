# Changelog

All notable changes to FlowBoard are documented here.  
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [1.0.0-beta] — 2026-05-13

### Added
- **UC6 — Comment & Attachment Service** (Port 5006)
  - Threaded comment support via `ParentCommentId` (nullable for top-level)
  - Soft-delete via `IsDeleted` EF Core global query filter
  - Attachment model with `FileName`, `FileUrl`, `FileType`, `SizeKb`, `UploadedAt`
  - MassTransit + RabbitMQ event publishing on every comment/attachment action
  - Full card detail modal UI with description, comments, attachments, status, priority
  - xUnit + Moq unit test project for UC6 service layer
- **UC5 — Card & Task Service** (Port 5005)
  - Full CRUD for task cards with priority and status fields
  - Drag-and-drop card reordering within and across columns (optimistic UI)
  - Card archive and delete with real-time board state updates
  - Card move API (`PUT /api/cards/{id}/move`)
- **UC4 — List/Column Service** (Port 5004)
  - Column drag-and-drop with atomic position persistence
  - Archive and restore column workflow
  - Cross-board column movement
  - Color picker for column accent theming
- **UC3 — Workspace & Member Service** (Port 5002)
  - Member invitation by email/username search
  - Role-based access (`ADMIN`, `MEMBER`)
  - Workspace visibility (`PUBLIC`, `PRIVATE`)
- **UC1/UC2 — Auth Service** (Port 5001)
  - JWT authentication with 24h token expiry
  - BCrypt password hashing
  - User profile management

### Infrastructure
- Docker Compose with 6 independent PostgreSQL instances
- RabbitMQ for async event messaging
- Nginx-served Angular production build in Docker

---

## [Unreleased]

### Planned
- UC7: Real-time collaboration via SignalR / WebSockets
- UC8: Card labels and tagging
- UC9: Activity log and audit trail
- UC12: Full-text search across cards and comments
