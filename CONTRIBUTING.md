# Contributing to FlowBoard

Thank you for your interest in contributing! This guide covers everything you need to set up the development environment, follow our conventions, and submit a great pull request.

---

## 📋 Table of Contents
- [Development Setup](#development-setup)
- [Branching Strategy](#branching-strategy)
- [Commit Convention](#commit-convention)
- [Code Standards](#code-standards)
- [Pull Request Process](#pull-request-process)

---

## Development Setup

### 1. Clone the repository
```bash
git clone https://github.com/your-username/flowboard.git
cd flowboard
```

### 2. Start infrastructure
```bash
docker-compose up -d postgres postgres_workspace postgres_board postgres_list postgres_card postgres_comment rabbitmq
```

### 3. Run a backend service
```bash
cd Backend/FlowBoard-Auth
dotnet restore
dotnet run
```

### 4. Run the frontend
```bash
cd Frontend
npm install
npm start
```

---

## Branching Strategy

| Branch | Purpose |
|---|---|
| `main` | Stable, production-ready code only |
| `develop` | Integration branch — all features merge here first |
| `feature/uc{N}-description` | New feature work (e.g. `feature/uc7-websockets`) |
| `fix/short-description` | Bug fixes |
| `chore/short-description` | Dependency updates, config, tooling |

---

## Commit Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(scope): <short description>

[optional body]
```

**Types:**
- `feat` — new feature
- `fix` — bug fix
- `docs` — documentation only
- `style` — formatting, no logic change
- `refactor` — code restructuring without feature change
- `test` — adding or fixing tests
- `chore` — build, CI, config changes

**Examples:**
```
feat(uc6): add threaded comment support with ParentCommentId
fix(board-detail): card click not opening modal after Docker rebuild
docs(readme): update API endpoint table for comment service
test(uc6): add unit tests for soft-delete comment logic
```

---

## Code Standards

### Backend (.NET)
- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- All public methods must have XML doc comments
- Use `async/await` for all I/O operations — no `.Result` or `.Wait()`
- All new services must have a corresponding unit test project

### Frontend (Angular)
- Use **standalone components** only — no NgModules
- Use **Angular Signals** for local component state
- Use **RxJS** for async service calls
- Follow the existing naming pattern: `feature.component.ts`, `feature.service.ts`
- CSS must go in the component's `.css` file — no inline styles

---

## Pull Request Process

1. Create your branch from `develop`
2. Ensure `dotnet test` and `ng build` both pass with **zero errors**
3. Update the `README.md` if you add a new endpoint or UC feature
4. Open a PR against `develop` with a clear description of what changed and why
5. At least one review approval is required before merging
