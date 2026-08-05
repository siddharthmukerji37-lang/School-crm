# CES-014 — Company Engineering Standard
# Project Folder Structure Guidelines

| Document ID | CES-014 |
|------------|---------|
| Version | 1.0 |
| Status | Approved |
| Owner | VP Technology |
| Audience | Software Architects, Technical Leads, Frontend Engineers, Backend Engineers, Full Stack Engineers |
| Applies To | All Software Projects |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. Scope
3. Folder Structure Philosophy
4. General Principles
5. Repository Structure
6. Backend Project Structure
7. Frontend Project Structure
8. Full Stack Monorepo Structure
9. Module Structure
10. Shared Components
11. Configuration Files
12. Documentation Structure
13. Test Structure
14. Asset Organization
15. Scripts & Automation
16. Infrastructure Folder
17. Naming Conventions
18. AI Engineering Guidelines
19. Folder Structure Checklist
20. Common Anti-Patterns

---

# 1. Purpose

This document defines the standard project folder structure to be followed across all software projects.

The objectives are:

- Maintain consistency across projects
- Improve developer onboarding
- Improve maintainability
- Improve discoverability
- Improve AI-assisted development
- Reduce architectural drift

A predictable folder structure allows engineers and AI coding assistants to understand a project quickly.

---

# 2. Scope

This standard applies to:

- Backend Applications
- Frontend Applications
- Full Stack Applications
- Microservices
- Internal Libraries
- Shared Packages
- Monorepositories
- AI Generated Projects

---

# 3. Folder Structure Philosophy

Every folder should represent a clear responsibility.

Folders should be organized around:

- Business Domains
- Features
- Responsibilities

Avoid organizing projects solely by file type.

Good folder structures improve:

- Scalability
- Team Collaboration
- Code Navigation
- AI Code Generation

---

# 4. General Principles

Every project should be:

- Modular
- Predictable
- Consistent
- Easy to Navigate
- Easy to Maintain

Every folder should have a clearly defined purpose.

Avoid unnecessary nesting.

---

# 5. Repository Structure

Every repository should follow the following layout.

```text
school-crm/

├── .github/
├── .vscode/
├── docs/
├── project-context/
├── docker/
├── SchoolCRM.sln
├── src/
│   ├── SchoolCRM.API/
│   ├── SchoolCRM.Application/
│   ├── SchoolCRM.Domain/
│   ├── SchoolCRM.Infrastructure/
│   └── SchoolCRM.Shared/
├── tests/
│   └── SchoolCRM.Tests/
├── client/                  (React + Vite frontend)
├── uploads/
├── logs/
├── .env.example
├── .gitignore
├── docker-compose.yml
├── README.md
└── LICENSE
```

Repository root should remain clean and uncluttered.

---

# 6. Backend Project Structure

School CRM uses ASP.NET Core 8 Clean Architecture across five projects inside `SchoolCRM.sln`.

```text
src/

├── SchoolCRM.API/                  (presentation layer)
│   ├── Controllers/                (Students, Teachers, Attendance, Fees, Exams, ...)
│   ├── Middlewares/                (ExceptionHandling, RequestLogging)
│   ├── Filters/
│   ├── Extensions/                 (ServiceCollection/DI registration)
│   ├── appsettings.json
│   └── Program.cs
│
├── SchoolCRM.Application/          (use cases / business orchestration)
│   ├── DTOs/                       (per module: Students/, Fees/, Exams/, ...)
│   ├── Interfaces/                 (IStudentService, IUnitOfWork, IRepository<T>, ...)
│   ├── Services/                   (StudentService, FeeService, ExamService, ...)
│   ├── Mappings/                   (AutoMapper Profiles)
│   ├── Validators/                 (FluentValidation validators)
│   └── Common/                     (ApiResponse<T>, PagedResult<T>, Exceptions)
│
├── SchoolCRM.Domain/                (enterprise layer — no external dependencies)
│   ├── Entities/                   (Student, Teacher, Class, Fee, Exam, ...)
│   ├── Enums/
│   └── Common/                     (BaseEntity with audit fields)
│
├── SchoolCRM.Infrastructure/        (data access & external services)
│   ├── Persistence/
│   │   ├── SchoolCrmDbContext.cs
│   │   ├── Configurations/         (EF Core Fluent API per entity)
│   │   └── Migrations/
│   ├── Repositories/                (Repository<T>, UnitOfWork)
│   ├── Identity/                    (ASP.NET Core Identity, JWT/Refresh Token services)
│   ├── Caching/                     (Redis)
│   ├── RealTime/                    (SignalR Hubs)
│   ├── Logging/                     (Serilog configuration)
│   └── ExternalServices/            (SMS, Email, Push)
│
└── SchoolCRM.Shared/                 (cross-cutting constants/helpers used by all layers)
```

Every module (Students, Teachers, Fees, Exams, Attendance, Library, Transport, Hostel, ...)
follows this same DTO → Interface → Service → Repository pattern across the layers above.

---

# 7. Frontend Project Structure

School CRM frontend: React 18 + Vite + Redux Toolkit + Material UI + React Router.

```text
client/src/

├── pages/               (StudentList, StudentAdmission, FeeCollection, ExamMarksEntry, ...)
├── layouts/             (AdminLayout, TeacherLayout, ParentLayout, AuthLayout)
├── components/          (shared/reusable UI only — DataTable, PageHeader, ConfirmDialog)
├── features/            (per module: students/, fees/, exams/, attendance/, ...)
│   └── students/
│       ├── studentsSlice.js     (Redux Toolkit slice)
│       ├── studentsApi.js       (Axios calls)
│       └── components/          (feature-local components)
├── hooks/
├── services/            (axios instance, interceptors, auth refresh handling)
├── store/               (Redux store + root reducer)
├── theme/               (Material UI theme)
├── constants/
├── utils/
├── validationSchemas/   (Yup schemas used with Formik)
├── routes/              (route guards per role/permission)
└── main.jsx
```

Business features should be grouped together under `features/`.

Avoid dumping everything inside `components`.

---

# 8. Full Stack Repository Structure (School CRM)

School CRM is not a package-based monorepo; it is a single repository containing one
.NET solution and one React app, kept side by side at the repository root:

```text
school-crm/

├── SchoolCRM.sln
├── src/                 (SchoolCRM.API, .Application, .Domain, .Infrastructure, .Shared)
├── tests/                (SchoolCRM.Tests)
├── client/               (React + Vite frontend)
├── docs/
├── project-context/
├── docker/
```

See sections 5–7 above for the detail inside `src/` and `client/`.

---

# 9. Module Structure

Every module should follow the same pattern across the layered projects.

Example — the Students module

```text
SchoolCRM.API/Controllers/StudentsController.cs
SchoolCRM.Application/DTOs/Students/{StudentDto, CreateStudentDto, UpdateStudentDto}.cs
SchoolCRM.Application/Interfaces/IStudentService.cs
SchoolCRM.Application/Services/StudentService.cs
SchoolCRM.Application/Validators/StudentValidators.cs
SchoolCRM.Application/Mappings/StudentProfile.cs
SchoolCRM.Domain/Entities/Student.cs
SchoolCRM.Infrastructure/Persistence/Configurations/StudentConfiguration.cs
SchoolCRM.Tests/Services/StudentServiceTests.cs
```

Every module should be self-contained: one controller, one DTO set, one service, one
entity, one EF configuration, one validator set, and matching tests.

---

# 10. Shared Components

Reusable code belongs inside dedicated shared folders.

Examples

```text
shared/

common/

ui/

utils/

hooks/

types/
```

Avoid duplicating functionality across modules.

---

# 11. Configuration Files

Configuration should be centralized.

Backend (ASP.NET Core)

```text
SchoolCRM.API/
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

Strongly-typed options classes (e.g., `JwtSettings`, `RedisSettings`, `SmsSettings`) live in
`SchoolCRM.Infrastructure`/`SchoolCRM.Shared` and are bound via `IOptions<T>` — never read
`IConfiguration` directly inside services.

Frontend (React)

```text
client/
├── .env.example
├── .env.development
└── .env.production
```

Never scatter configuration throughout the application.

---

# 12. Documentation Structure

Every project should contain:

```text
docs/

├── architecture/
├── api/
├── deployment/
├── diagrams/
├── release-notes/
├── decisions/
└── onboarding/
```

Documentation should evolve alongside the application.

---

# 13. Test Structure

Recommended

```text
tests/

├── unit/
├── integration/
├── api/
├── e2e/
├── performance/
├── security/
└── fixtures/
```

Each test type should have a dedicated location.

---

# 14. Asset Organization

Frontend assets

```text
assets/

├── images/
├── icons/
├── illustrations/
├── logos/
├── fonts/
└── videos/
```

Optimize assets before committing.

Never commit unnecessary large files.

---

# 15. Scripts & Automation

Automation scripts should be separated.

```text
scripts/

├── build/
├── deploy/
├── migration/
├── backup/
├── seed/
├── cleanup/
└── utilities/
```

Scripts should be documented.

---

# 16. Infrastructure Folder

Infrastructure-as-Code belongs here.

```text
infrastructure/

├── terraform/
├── kubernetes/
├── nginx/
├── apache/
├── cloudformation/
├── ansible/
└── monitoring/
```

Infrastructure changes should follow version control.

---

# 17. Naming Conventions

## Folder Names

- Backend project/solution folders: `PascalCase` (matches C# project naming, e.g. `SchoolCRM.API`).
- Frontend folders (`client/`): `kebab-case` or `camelCase`, meaningful names (e.g. `student-admission`, `feature/attendance`).

Avoid

```text
Temp

NewFolder

misc

test123
```

---

## File Names

Backend (C# — PascalCase, matches Microsoft conventions)

```text
StudentController.cs

StudentService.cs

IStudentRepository.cs

Student.cs

StudentDto.cs

StudentProfile.cs             (AutoMapper profile)

StudentValidator.cs           (FluentValidation)
```

Frontend (React — PascalCase for components, camelCase for logic)

```text
StudentList.jsx

StudentForm.jsx

studentsSlice.js

studentsApi.js

studentValidationSchema.js
```

---

# 18. AI Engineering Guidelines

When using AI Coding Assistants:

AI SHOULD

- Preserve existing folder structure.
- Reuse existing modules.
- Place files in the correct directories.
- Follow module boundaries.
- Follow naming conventions.
- Avoid duplicate folders.

AI SHOULD NOT

- Create unnecessary folders.
- Create folders like:

```text
misc/

temp/

new/

helpers2/

utils-new/
```

- Move existing files without reason.
- Break project organization.

Before generating code, AI should inspect the current folder structure and follow existing patterns.

---

# 19. Folder Structure Checklist

Before creating new files verify:

## Structure

- [ ] Existing folder reviewed.
- [ ] Correct module selected.
- [ ] No duplicate folders.

---

## Naming

- [ ] Folder names follow convention.
- [ ] File names follow convention.

---

## Organization

- [ ] Configuration centralized.
- [ ] Tests placed correctly.
- [ ] Documentation updated.

---

## AI

- [ ] Existing structure reused.
- [ ] No unnecessary directories created.

---

# 20. Common Anti-Patterns

Avoid:

- Deep folder nesting
- Duplicate modules
- Duplicate utilities
- Generic folders like

```text
misc/

temp/

common2/

helpers-old/

backup/
```

- Business logic inside utils
- Shared mutable state
- Mixed frontend/backend code
- Hardcoded assets
- Configuration spread across modules
- Monolithic folders
- Unused directories
- Dead files
- Committed build artifacts
- Committed secrets

---

# Recommended Project Structure

```text
school-crm/

├── docs/
├── project-context/
├── docker/
├── SchoolCRM.sln
│
├── src/
│   ├── SchoolCRM.API/
│   ├── SchoolCRM.Application/
│   ├── SchoolCRM.Domain/
│   ├── SchoolCRM.Infrastructure/
│   └── SchoolCRM.Shared/
│
├── tests/
│   └── SchoolCRM.Tests/
│
├── client/                  (React + Vite frontend)
│
├── uploads/
├── logs/
├── README.md
├── .env.example
└── .gitignore
```

---

# Final Engineering Principles

A well-structured project should be:

- Consistent
- Modular
- Predictable
- Discoverable
- Maintainable
- Scalable
- AI-Friendly

Developers should be able to locate any feature, service, component, or configuration file within minutes without relying on tribal knowledge.

Likewise, AI coding assistants should be able to understand the project organization immediately and generate code that seamlessly integrates with the existing architecture rather than introducing unnecessary folders or inconsistent structures.

Folder structure is not merely an organizational preference—it is a fundamental part of software architecture and plays a critical role in long-term maintainability.

---

# Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Release | VP Technology |

---

# End of Document

**Company Engineering Standard — CES-014**  
**Project Folder Structure Guidelines**  
**Version 1.0**