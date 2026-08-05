# CES-013 — Company Engineering Standard
# Project Overview Template

| Document ID | CES-013 |
|------------|---------|
| Version | 1.1 |
| Status | Filled — School CRM Management System |
| Owner | Project Manager / Solution Architect |
| Reviewed By | VP Technology |
| Applies To | School CRM Management System |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. How to Use This Document
3. Project Information
4. Executive Summary
5. Business Background
6. Business Objectives
7. Success Criteria
8. Project Scope
9. Out of Scope
10. Stakeholders
11. Team Structure
12. High-Level Architecture
13. Technology Stack
14. External Integrations
15. Project Modules
16. Non-Functional Requirements
17. Assumptions
18. Constraints
19. Risks
20. Environment Details
21. Deployment Strategy
22. Documentation References
23. AI Engineering Instructions
24. Project Readiness Checklist
25. Version History

---

# 1. Purpose

This document provides a high-level overview of the project.

It is intended to give every stakeholder—including software engineers, QA engineers, project managers, architects, and AI coding assistants—a clear understanding of the project's purpose, scope, architecture, and implementation strategy before development begins.

This document should be the first document read by any new engineer joining the project.

---

# 2. How to Use This Document

Before starting development:

- Read this document completely.
- Understand the business domain.
- Review the project objectives.
- Understand the system architecture.
- Identify project constraints.
- Review referenced engineering standards.

This document provides the overall project context.

Detailed implementation guidance is available in the remaining CES documents.

---

# 3. Project Information

| Field | Value |
|---------|--------|
| Project Name | School CRM Management System |
| Project Code | SCHOOL-CRM |
| Customer | Internal / Multi-tenant School Product |
| Industry | Education |
| Project Type | Web Application (API + SPA) |
| Start Date | July 2026 |
| Expected Completion | TBD |
| Current Version | 1.0.0-dev |
| Current Phase | Development |

---

# 4. Executive Summary

Provide a concise overview of the project.

```
The School CRM Management System is a production-grade, multi-role web application that
digitizes end-to-end school operations — admissions, academics, attendance, examinations,
fees, HR/payroll, transport, hostel, library, inventory, accounts, and communication.

It is built as a REST API (ASP.NET Core 8, Clean Architecture) consumed by a React SPA,
serving 11 distinct roles ranging from Super Admin to Student/Parent, with real-time
notifications, role- and permission-based authorization, and reporting/export capability.
```

The executive summary should answer:

- What is the project?
- Why does it exist?
- Who will use it?
- What business problem does it solve?

---

# 5. Business Background

Describe the business context that led to the project.

Include:

- Current challenges
- Existing manual processes
- Business pain points
- Customer expectations
- Market opportunities

```
Schools currently manage admissions, attendance, fees, exams, payroll, and communication
through disconnected registers, spreadsheets, and point solutions. This causes duplicate
data entry, delayed fee/attendance reporting, poor parent visibility into student progress,
and no single source of truth for administrators. The School CRM consolidates these
processes into one role-based platform with real-time dashboards and reporting.
```

---

# 6. Business Objectives

List measurable project objectives.

- Digitize admissions, attendance, exams, and fee collection end-to-end.
- Give every role (admin, teacher, student, parent) a live, permission-scoped dashboard.
- Reduce manual attendance/fee reconciliation effort by moving to a single source of truth.
- Provide real-time notifications (SignalR, SMS, Email, Push) for key school events.
- Support multi-branch / multi-campus schools on one platform.
- Build the system so it is Docker/Azure deployment-ready from day one.

Business objectives should be measurable whenever possible.

---

# 7. Success Criteria

Define what success looks like.

- Dashboard and list APIs (pagination/search/filter/sort) respond in under 500 ms.
- Fee and attendance reports generate within 5 seconds for a single branch.
- Zero critical security findings (OWASP Top 10) at UAT.
- Role-based access enforced on 100% of endpoints (no endpoint reachable without correct role/permission).
- CI pipeline (GitHub Actions) runs build + unit tests on every PR with no manual steps.

Success criteria should be measurable.

---

# 8. Project Scope

Describe what is included in this project.

Included modules

- Authentication, Role & Permission Management
- Dashboard (statistics, charts, quick actions)
- School Management (profile, academic year, branches, classes, sections, subjects, timetable, holiday calendar)
- Student, Parent, Teacher, and Employee (HR/Payroll) Management
- Attendance (student/teacher/staff, QR/biometric-ready)
- Exam Management (schedule, marks entry, grading, report cards, ranking)
- Homework & Assignment Management
- Library, Transport, and Hostel Management
- Fee Management (structure, collection, discounts, receipts, online-payment-ready)
- Inventory and Accounts
- Notifications (SMS, Email, Push, SignalR live) and Communication (notice board, chat)
- Reporting (Excel/PDF/CSV export) and Settings/Audit Logs

Clearly define project boundaries.

---

# 9. Out of Scope

Identify functionality intentionally excluded.

- Native Mobile Application (web app is responsive; native apps are a future phase).
- Offline Support.
- Actual payment gateway integration (system is "online-payment-ready" only; gateway wiring is a later phase).
- Live GPS tracking hardware integration (transport module is "GPS-ready structure" only).
- Multi-language / localization (English only for v1).
- AI Chatbot / third-party marketplace.

Clearly documenting exclusions prevents scope creep.

---

# 10. Stakeholders

Identify all project stakeholders.

| Role | Responsibility |
|------|----------------|
| School / Product Owner | Business Owner, feature priorities |
| Solution Architect | Clean Architecture, Technology Decisions |
| VP Technology | Engineering Governance |
| Technical Lead | Technical Execution |
| Backend Engineers | ASP.NET Core API, EF Core, Domain logic |
| Frontend Engineers | React SPA (Redux Toolkit, MUI) |
| QA Lead | Quality Assurance (xUnit, integration, UI) |
| DevOps Engineer | Docker, GitHub Actions, Azure |
| End Users | Super Admin, School Admin, Principal, Vice Principal, Teacher, Class Teacher, Accountant, Receptionist, Librarian, Student, Parent |

---

# 11. Team Structure

Document the engineering organization.

Example

```
VP Technology

↓

Solution Architect

↓

Project Manager

↓

Technical Lead

↓

Backend Engineers

Frontend Engineers

QA Engineers

DevOps Engineer
```

Clearly define reporting relationships.

---

# 12. High-Level Architecture

Describe the architecture.

```
Users (Web Browser)

↓

Frontend — React 18 + Vite + Redux Toolkit + Material UI

↓

Backend — SchoolCRM.API (ASP.NET Core 8 Web API, JWT + Refresh Tokens)

↓

SchoolCRM.Application (Services, DTOs, AutoMapper, FluentValidation, CQRS-style use cases)

↓

SchoolCRM.Domain (Entities, Enums, Domain Rules) — no external dependencies

↓

SchoolCRM.Infrastructure (EF Core + MySQL, Repository/Unit of Work, Redis Cache, SignalR, Identity, Serilog)

↓

MySQL Database  |  Redis Cache  |  SMS/Email Providers
```

Architecture Summary

- Clean Architecture (API → Application → Domain ← Infrastructure)
- REST APIs versioned and documented via Swagger/OpenAPI
- Repository Pattern + Unit of Work
- Service Layer with DTOs and AutoMapper
- JWT Authentication with Refresh Tokens, ASP.NET Core Identity
- Role-Based and Permission-Based Authorization
- SignalR for real-time notifications
- Redis for caching
- Serilog for structured logging

Detailed implementation is documented in CES-001.

---

# 13. Technology Stack

Document all technologies.

## Frontend

- React.js
- Vite
- Material UI
- React Router
- Redux Toolkit
- Axios
- Formik + Yup Validation

---

## Backend

- ASP.NET Core 8 Web API (C#)
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication + Refresh Tokens
- AutoMapper
- FluentValidation
- Repository Pattern + Unit of Work
- Serilog (logging)
- Redis (caching)
- SignalR (real-time notifications)
- Swagger / OpenAPI

---

## Database

- MySQL

---

## Infrastructure

- Docker
- Docker Compose
- Azure (deployment target)

---

## CI/CD

- GitHub Actions

---

## Testing

- xUnit
- Moq

---

## AI Tools

- Claude Code
- GitHub Copilot
- Cursor
- Continue
- Cline

---

# 14. External Integrations

List external systems.

- SMS Gateway (for attendance/fee notifications)
- Email Provider (SMTP / SendGrid-style)
- Payment Gateway (online-payment-ready hook, provider TBD)
- File/Image Storage (local/Blob storage for documents, ID cards, certificates)
- Push Notification Provider

For each integration specify:

- Purpose
- Authentication Method
- Criticality

---

# 15. Project Modules

Document all major modules.

```
Authentication & Authorization        Homework & Assignments

Dashboard                             Library Management

School Management                     Transport Management

Student Management                    Hostel Management

Parent Management                     Fee Management

Teacher Management                    Inventory

Employee Management (HR/Payroll)      Accounts

Attendance                            Notifications & Communication

Exam Management                       Reports

                                       Settings, Security & Audit Logs
```

Provide a brief description of each module.

---

# 16. Non-Functional Requirements

Performance

```
API response <500 ms for list/dashboard endpoints
```

Availability

```
99.9% (school working hours are critical)
```

Security

```
OWASP Top 10 Compliance, JWT + Refresh Tokens, Role & Permission based Authorization
```

Scalability

```
Support multi-branch/multi-campus schools; designed to scale to 10,000+ students per tenant
```

Reliability

```
Zero data loss; soft delete + audit fields (CreatedBy/UpdatedBy/DeletedBy, CreatedAt/UpdatedAt/DeletedAt) on all entities
```

Maintainability

```
Clean Architecture, SOLID, Company Engineering Standards
```

---

# 17. Assumptions

Document assumptions.

Examples

- Internet connectivity available.
- Third-party APIs operational.
- Authentication provider available.
- Database accessible.
- Required licenses available.

Review assumptions periodically.

---

# 18. Constraints

Examples

- Fixed Budget
- Fixed Timeline
- Existing Technology Stack
- Regulatory Compliance
- Legacy Integration
- Customer Infrastructure

Understanding constraints helps guide engineering decisions.

---

# 19. Risks

Document known project risks.

Example

| Risk | Impact | Mitigation |
|------|---------|------------|
| Third-party API changes | High | Version APIs |
| Key resource unavailable | Medium | Cross-training |
| Tight deadlines | High | Prioritize MVP |
| Legacy integration | Medium | Early testing |

Risks should be reviewed throughout the project lifecycle.

---

# 20. Environment Details

Document environments.

| Environment | Purpose |
|------------|---------|
| Local | Development |
| Development | Team Integration |
| QA | Testing |
| UAT | Customer Validation |
| Staging | Pre-production |
| Production | Live System |

Document URLs, deployment strategy, and ownership separately.

---

# 21. Deployment Strategy

Document deployment approach.

Example

```
Developer

↓

Git Repository

↓

CI Pipeline

↓

Development

↓

QA

↓

UAT

↓

Production
```

Deployment should support:

- Rollback
- Zero Downtime
- Monitoring
- Logging
- Health Checks

---

# 22. Documentation References

Every project should maintain:

- CES-001 Architecture
- CES-002 Coding Standards
- CES-003 API Guidelines
- CES-004 UI Guidelines
- CES-005 Security Standards
- CES-006 Database Standards
- CES-007 Testing Standards
- CES-008 Business Rules
- CES-009 Error Handling
- CES-010 Definition of Done
- CES-011 Git Workflow
- CES-012 AI Engineering Instructions
- CES-014 Folder Structure
- README.md

---

# 23. AI Engineering Instructions

Before generating code, AI must:

- Read this Project Overview.
- Read every CES document.
- Understand the project scope.
- Follow the documented architecture.
- Respect business constraints.
- Reuse existing project structure.
- Avoid introducing unnecessary frameworks.
- Preserve consistency across modules.

AI should optimize for long-term maintainability rather than short-term implementation speed.

---

# 24. Project Readiness Checklist

Before development begins verify:

## Business

- [ ] Business objectives documented.
- [ ] Scope defined.
- [ ] Success criteria defined.

---

## Technical

- [ ] Architecture completed.
- [ ] Technology stack finalized.
- [ ] Integrations identified.

---

## Engineering

- [ ] Engineering Standards available.
- [ ] Folder structure approved.
- [ ] Coding standards reviewed.
- [ ] Security standards reviewed.
- [ ] Testing strategy approved.

---

## Project

- [ ] Stakeholders identified.
- [ ] Risks documented.
- [ ] Constraints documented.
- [ ] Environments prepared.

---

## AI

- [ ] Context files available.
- [ ] AI Instructions reviewed.
- [ ] Business Rules completed.

---

# 25. Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Template | VP Technology |
| 1.1 | July 2026 | Filled in for School CRM Management System (ASP.NET Core 8 / MySQL / React stack, roles, modules) | Solution Architect |

---

# Final Notes

The **Project Overview** serves as the entry point for every software project.

Every engineer, QA engineer, architect, project manager, and AI coding assistant should begin by reading this document before reviewing detailed engineering standards.

A well-written Project Overview minimizes onboarding time, reduces misunderstandings, aligns engineering decisions with business goals, and ensures consistent implementation throughout the project lifecycle.

---

# End of Document

**Company Engineering Standard — CES-013**  
**Project Overview Template**  
**Version 1.0**