# School CRM Management System — Planning Document

## 1. Project Overview

| Field | Value |
|-------|-------|
| Project Name | School CRM Management System |
| Type | Production-Grade Enterprise SaaS Application |
| Architecture | Clean Architecture (ASP.NET Core solution + separate React frontend) |
| ORM | Entity Framework Core |
| AI Tool | Claude Code |
| Start Date | July 2026 |

### Roles (11)

Super Admin · School Admin · Principal · Vice Principal · Teacher · Class Teacher ·
Accountant · Receptionist · Librarian · Student · Parent

### Core Feature Groups

- **Authentication & Authorization** — Login, Logout, Forgot/Reset Password, Refresh Token, Change Password, Update Profile, Role & Permission Management
- **Dashboard** — totals, today's attendance, fees collected/pending, upcoming exams, birthdays, announcements, recent admissions, charts
- **School Management** — profile, academic year, branches, campus, departments, classes, sections, subjects, timetable, periods, holiday calendar, events
- **Student Management** — registration, admission, profile, promotion, transfer, documents, attendance, leave, ID card, certificates, health record, transport, hostel, student login/dashboard
- **Parent Management** — registration/login, guardian details, dashboard, children list, attendance/marks/fee/homework view, notifications
- **Teacher Management** — registration, attendance, leave, salary, timetable, dashboard, documents, performance, login
- **Employee Management** — non-teaching staff, HR, payroll, salary, attendance, leave, departments, designation
- **Attendance** — student/teacher/staff, daily/monthly, QR-ready, biometric-ready, reports
- **Exam Management** — exam types, schedule, hall, marks entry, grade system, result publish, report card, ranking
- **Homework & Assignment Management**
- **Library, Transport, Hostel Management**
- **Fee Management** — structure, collection, discount, scholarship, installments, receipt, online-payment-ready, pending fees, reports
- **Inventory & Accounts**
- **Notification System** — SMS, Email, Push, SignalR live notifications
- **Communication** — notice board, announcements, circulars, chat, parent-teacher chat
- **Reports** — student, attendance, fee, salary, library, exam, inventory, financial
- **Settings & Security** — school/email/SMS settings, roles, permissions, audit logs, backup/restore

---

## 2. Technology Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 Web API, C# |
| ORM | Entity Framework Core |
| Database | MySQL |
| Auth | ASP.NET Core Identity, JWT (access + refresh tokens) |
| Mapping | AutoMapper |
| Validation | FluentValidation |
| Data Access | Repository Pattern + Unit of Work |
| Logging | Serilog |
| Caching | Redis |
| Realtime | SignalR |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit + Moq |
| CI/CD | GitHub Actions |
| Infra | Docker, Docker Compose, Azure-ready |
| Frontend | React.js (Vite), Material UI, React Router, Redux Toolkit, Axios, Formik, Yup |

---

## 3. Folder Structure

```text
school-crm/
├── SchoolCRM.sln
├── src/
│   ├── SchoolCRM.API/               # Controllers, Middlewares, Program.cs, appsettings.json
│   ├── SchoolCRM.Application/       # DTOs, Interfaces, Services, Mappings, Validators, Common
│   ├── SchoolCRM.Domain/            # Entities, Enums, BaseEntity (audit fields)
│   ├── SchoolCRM.Infrastructure/    # DbContext, Repositories, Identity, Caching, RealTime, Logging
│   └── SchoolCRM.Shared/            # Cross-cutting constants/helpers
│
├── tests/
│   └── SchoolCRM.Tests/             # Unit + Integration tests (xUnit + Moq)
│
├── client/                          # React.js (Vite) frontend
│   ├── src/
│   │   ├── pages/                   # per-module pages (students/, fees/, exams/, ...)
│   │   ├── layouts/                 # AdminLayout, TeacherLayout, ParentLayout, AuthLayout
│   │   ├── components/              # shared/reusable UI
│   │   ├── features/                # per-module Redux slices + Axios calls
│   │   ├── hooks/
│   │   ├── services/                # axios instance, interceptors, token refresh
│   │   ├── store/                   # Redux Toolkit store
│   │   ├── theme/                   # Material UI theme
│   │   ├── validationSchemas/       # Yup schemas used with Formik
│   │   ├── routes/                  # role/permission-guarded routes
│   │   └── main.jsx
│   ├── index.html
│   ├── vite.config.js
│   └── package.json
│
├── docs/                            # CES documentation (project-context)
├── docker/
├── docker-compose.yml
├── .env.example
├── README.md
└── planning.md
```

---

## 4. Database Design (Entity Framework Core / MySQL — core entities)

```text
BaseEntity
  Id (Guid), CreatedBy, UpdatedBy, DeletedBy, CreatedAt, UpdatedAt, DeletedAt, IsDeleted

School            : BaseEntity   (Name, Code, AcademicYearId, Branches[])
AcademicYear      : BaseEntity   (Name, StartDate, EndDate, SchoolId)
Branch / Campus    : BaseEntity
Department         : BaseEntity
ClassRoom          : BaseEntity   (Name, AcademicYearId)
Section             : BaseEntity   (Name, ClassRoomId, ClassTeacherId)
Subject             : BaseEntity   (Name, Code, ClassRoomId)
Timetable / Period   : BaseEntity

ApplicationUser     : IdentityUser (extends Identity — backs Staff/Student/Parent logins)
Role / Permission     : Identity Role + custom Permission entity (many-to-many with Role)

Student             : BaseEntity   (AdmissionNo, UserId, SectionId, ParentId, Status)
Parent / Guardian     : BaseEntity   (UserId, Students[])
Teacher              : BaseEntity   (UserId, EmployeeCode, Subjects[], Sections[])
Employee              : BaseEntity   (UserId, DepartmentId, Designation)

Attendance            : BaseEntity   (Date, StudentId/TeacherId/EmployeeId, Status)
ExamType / Exam         : BaseEntity
ExamSchedule            : BaseEntity   (ExamId, SubjectId, Date, MaxMarks)
Mark                     : BaseEntity   (ExamScheduleId, StudentId, MarksObtained)
GradeSystem               : BaseEntity
ReportCard                 : BaseEntity   (StudentId, ExamId, PublishedAt)

Homework / Assignment        : BaseEntity   (ClassId, SubjectId, TeacherId, DueDate)
HomeworkSubmission             : BaseEntity   (HomeworkId, StudentId, SubmittedAt)

Book / BookCategory              : BaseEntity
BookIssue                          : BaseEntity   (BookId, StudentId, IssueDate, DueDate, ReturnDate, Fine)

Route / Vehicle / Driver / PickupPoint : BaseEntity
StudentTransportAllocation               : BaseEntity

Room / Bed / HostelAllocation              : BaseEntity

FeeStructure / FeeHead                       : BaseEntity
FeeInstallment                                 : BaseEntity   (StudentId, DueDate, Amount, Status)
FeeReceipt                                       : BaseEntity   (InstallmentId, PaidAmount, PaidAt)

InventoryItem / StockTransaction                   : BaseEntity
LedgerEntry / IncomeExpense                          : BaseEntity

Notification                                           : BaseEntity   (UserId, Type, Message, IsRead)
Announcement / Circular / Chat Message                   : BaseEntity
AuditLog                                                   : BaseEntity   (Entity, Action, PerformedBy, Timestamp)
```

Every table uses soft delete (`IsDeleted`/`DeletedAt`/`DeletedBy`) and audit fields
(`CreatedBy`/`UpdatedBy`/`CreatedAt`/`UpdatedAt`), enforced through `BaseEntity` and a
global EF Core query filter.

---

## 5. API Endpoints (representative — full list documented per module in Swagger)

### Authentication

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | /api/auth/register | Register user (staff/student/parent) | No |
| POST | /api/auth/login | Login, returns access + refresh token | No |
| POST | /api/auth/refresh | Refresh access token | No |
| POST | /api/auth/logout | Revoke refresh token | Yes |
| POST | /api/auth/forgot-password | Send reset link/OTP | No |
| POST | /api/auth/reset-password | Reset password | No |
| POST | /api/auth/change-password | Change password | Yes |
| GET/PATCH | /api/auth/me | Get/update own profile | Yes |

### School / Academic Setup

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET/POST/PATCH/DELETE | /api/academic-years | Manage academic years | Yes (Admin) |
| GET/POST/PATCH/DELETE | /api/classes | Manage classes | Yes (Admin) |
| GET/POST/PATCH/DELETE | /api/sections | Manage sections | Yes (Admin) |
| GET/POST/PATCH/DELETE | /api/subjects | Manage subjects | Yes (Admin) |
| GET/POST | /api/timetable | Manage timetable | Yes (Admin/Principal) |

### Students / Parents / Teachers / Employees

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | /api/students | List (pagination, search, filter, sort) | Yes |
| POST | /api/students | Admit new student | Yes (Admin/Receptionist) |
| GET/PATCH/DELETE | /api/students/{id} | Get/update/soft-delete student | Yes |
| POST | /api/students/{id}/promote | Promote/transfer student | Yes (Admin) |
| GET/POST/PATCH/DELETE | /api/parents | Parent CRUD | Yes |
| GET/POST/PATCH/DELETE | /api/teachers | Teacher CRUD | Yes (Admin) |
| GET/POST/PATCH/DELETE | /api/employees | Employee/HR CRUD | Yes (Admin) |

### Attendance / Exams / Fees

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | /api/attendance | Mark attendance (student/teacher/staff) | Yes (Teacher/Admin) |
| GET | /api/attendance/reports | Attendance reports | Yes |
| GET/POST | /api/exams | Manage exams/schedule | Yes (Admin/Principal) |
| POST | /api/exams/{id}/marks | Enter marks | Yes (Teacher) |
| POST | /api/exams/{id}/publish | Publish result | Yes (Admin/Principal) |
| GET | /api/fees/structures | List fee structures | Yes |
| POST | /api/fees/collect | Collect fee, generate receipt | Yes (Accountant) |
| GET | /api/fees/pending | Pending fee report | Yes |

### Notifications / Reports

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | /api/notifications | List current user's notifications | Yes |
| PATCH | /api/notifications/{id}/read | Mark as read | Yes |
| GET | /api/reports/{type}?format=excel\|pdf\|csv | Export a report | Yes |
| GET | /api/dashboard/stats | Dashboard KPIs | Yes |

All list endpoints support pagination, searching, filtering, and sorting via standard
query parameters and return the standardized `ApiResponse<PagedResult<T>>` envelope.

---

## 6. Frontend Pages (React Router, role-guarded)

| Route | Page | Roles |
|-------|------|-------|
| /login, /forgot-password, /reset-password | Auth pages | All |
| / | Dashboard (role-specific widgets) | All |
| /students, /students/:id, /students/admission | Student management | Admin/Receptionist/Teacher |
| /teachers, /teachers/:id | Teacher management | Admin |
| /parents, /parents/:id | Parent management | Admin/Receptionist |
| /attendance | Mark/view attendance | Teacher/Class Teacher/Admin |
| /exams, /exams/:id/marks-entry, /report-cards | Exam management | Teacher/Admin/Principal |
| /homework | Homework/assignments | Teacher/Student/Parent |
| /library | Library/book issue-return | Librarian/Student |
| /transport, /hostel | Transport/Hostel | Admin |
| /fees, /fees/collect | Fee management | Accountant/Admin/Parent (view) |
| /inventory, /accounts | Inventory & Accounts | Accountant/Admin |
| /notifications, /notice-board | Notifications/Communication | All |
| /reports | Reports (export) | Admin/Principal/Accountant |
| /settings, /roles-permissions, /audit-logs | Settings & Security | Super Admin/School Admin |
| /my-dashboard (student/parent view) | Attendance/marks/fees/homework view | Student/Parent |

---

## 7. Implementation Phases

### Phase 1: Foundation (Week 1)

- Scaffold `SchoolCRM.sln` (API, Application, Domain, Infrastructure, Shared, Tests)
- Configure EF Core + MySQL, base migrations, `BaseEntity` with audit + soft delete
- Global exception handling middleware, Serilog, standardized `ApiResponse<T>`
- Swagger/OpenAPI setup, Docker Compose (mysql, redis, api, client)
- Initialize React (Vite) client with MUI theme, Redux Toolkit store, React Router, Axios instance

### Phase 2: Identity, Auth & Roles (Week 1-2)

- ASP.NET Core Identity setup, JWT + refresh token issuance/rotation
- Role & Permission management (11 roles), permission-based authorization policies
- Frontend: Login/Register/Forgot/Reset pages, protected + role-guarded routes

### Phase 3: School Setup & Core Entities (Week 2)

- Academic Year, Branches, Classes, Sections, Subjects, Timetable, Holiday Calendar
- Repository Pattern + Unit of Work, AutoMapper profiles, FluentValidation validators

### Phase 4: Student / Parent / Teacher / Employee Management (Week 2-3)

- CRUD + admission/promotion/transfer workflows
- Frontend: list/detail/forms with pagination, search, filter, sort

### Phase 5: Attendance & Exam Management (Week 3-4)

- Attendance marking (student/teacher/staff) with QR/biometric-ready hooks
- Exam scheduling, marks entry, grading, result publish, report cards, ranking

### Phase 6: Homework, Library, Transport, Hostel (Week 4)

- Homework/assignment workflow with submissions
- Library issue/return with fines, Transport route/vehicle allocation, Hostel room/bed allocation

### Phase 7: Fee Management, Inventory, Accounts (Week 5)

- Fee structures, installments, collection, receipts, pending-fee reports
- Inventory stock/issue tracking, ledger/income-expense accounts

### Phase 8: Notifications, Communication, Dashboard, Reports (Week 5-6)

- SignalR live notifications, SMS/Email/Push wiring
- Notice board, announcements, parent-teacher chat
- Role-specific dashboards with charts/graphs, Excel/PDF/CSV export

### Phase 9: Testing, Security Hardening, CI/CD (Week 6-7)

- xUnit + Moq unit/integration tests (repositories, services, controllers)
- OWASP hardening, audit trail, login history, account lock
- GitHub Actions pipeline (build, test, docker build), Azure deployment readiness

---

## 8. Docker Compose

```yaml
version: '3.8'
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_DATABASE: schoolcrm
      MYSQL_ROOT_PASSWORD: root
      MYSQL_USER: appuser
      MYSQL_PASSWORD: apppassword
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    build: ./src/SchoolCRM.API
    ports:
      - "5000:80"
    environment:
      ConnectionStrings__DefaultConnection: "server=mysql;port=3306;database=schoolcrm;user=appuser;password=apppassword"
      Redis__ConnectionString: "redis:6379"
      Jwt__Secret: "your-super-secret-jwt-key"
      Jwt__AccessTokenExpirationMinutes: "15"
      Jwt__RefreshTokenExpirationDays: "7"
    depends_on:
      - mysql
      - redis

  client:
    build: ./client
    ports:
      - "3000:3000"
    environment:
      VITE_API_URL: http://localhost:5000
    depends_on:
      - api

volumes:
  mysql_data:
```

---

## 9. AI Workflow Strategy

### Context Engineering Approach

1. Before each feature: read `project-context/13-project-overview.md` and
   `project-context/08-business-rules.md`, then the relevant CES standard
   (architecture, coding, API, security).
2. Prompt structure: include layer being worked on (Domain/Application/Infrastructure/API),
   existing patterns, and the module's business rules.
3. Validation: `dotnet build` + `dotnet test` after each AI-generated backend feature;
   lint/build after frontend features.
4. Manual review: verify business logic, role/permission enforcement, and error handling.

### AI Usage Map

| Task | AI Role | Human Role |
|------|---------|------------|
| Entity/DTO scaffolding | Generate Domain entities + Application DTOs | Review relationships, audit fields |
| Service + Repository | Generate service/repository implementations | Verify business rules, transactions |
| Controller + Swagger | Generate controllers with standardized responses | Verify auth policies |
| Validation | Generate FluentValidation validators | Verify business constraints |
| React pages/forms | Generate MUI pages/Formik forms | Review UX, accessibility |
| Notification wiring | Generate SignalR hub + event triggers | Verify trigger correctness |
| Testing | Generate xUnit/Moq test scaffolding | Write assertion logic |
| Debugging | Analyze errors, suggest fixes | Verify root cause |

---

## 10. Documentation Deliverables

| File | Content |
|------|---------|
| planning.md | This document |
| LESSONS_LEARNED.md | AI workflow reflection |
| AI_WORKFLOW_REPORT.md | Tools, prompts, decisions |
| AI_DECISION_LOG.md | Task-by-task AI vs human decisions |
| README.md | Setup instructions, architecture |
| docs/13-project-overview.md, docs/08-business-rules.md | Project-specific CES docs (filled) |

---

## 11. Success Criteria

- [ ] Clean Architecture solution builds (`SchoolCRM.API/.Application/.Domain/.Infrastructure/.Shared/.Tests`)
- [ ] Docker Compose starts mysql, redis, api, and client
- [ ] JWT + refresh token auth works, all 11 roles enforced via role/permission policies
- [ ] Student/Parent/Teacher/Employee CRUD with pagination, search, filter, sort
- [ ] Attendance marking + reports functional
- [ ] Exam → marks entry → result publish → report card pipeline functional
- [ ] Fee structure → installment → collection → receipt pipeline functional
- [ ] SignalR live notifications + SMS/Email/Push wiring in place
- [ ] Dashboards render correct KPIs per role
- [ ] Reports export to Excel/PDF/CSV
- [ ] Swagger docs accessible and complete
- [ ] xUnit + Moq tests cover services/repositories/controllers for core modules
- [ ] GitHub Actions CI builds, tests, and (optionally) publishes Docker images
- [ ] All CES standards (docs/*.md) followed

---

## 12. Environment Variables

### Backend (appsettings / .env)

```env
ConnectionStrings__DefaultConnection=server=localhost;port=3306;database=schoolcrm;user=appuser;password=apppassword
Redis__ConnectionString=localhost:6379
Jwt__Secret=your-super-secret-jwt-key
Jwt__AccessTokenExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=7
Serilog__MinimumLevel=Information
Cors__AllowedOrigins=http://localhost:3000
```

### Frontend (.env)

```env
VITE_API_URL=http://localhost:5000
```

---

**End of Planning Document**
