# CES-008 — Company Engineering Standard
# Business Rules Template

| Document ID | CES-008 |
|------------|---------|
| Version | 1.1 |
| Status | Filled — School CRM Management System |
| Owner | Product Owner / Business Analyst / Solution Architect |
| Reviewed By | VP Technology |
| Applies To | School CRM Management System |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. Scope
3. Document Ownership
4. How to Use this Document
5. Project Overview
6. Business Objectives
7. Stakeholders
8. User Roles
9. Business Terminology
10. Functional Modules
11. Business Rules
12. Validation Rules
13. Workflow Rules
14. Notification Rules
15. Reporting Requirements
16. Security Rules
17. Performance Expectations
18. Non-Functional Business Requirements
19. Assumptions
20. Constraints
21. Out of Scope
22. AI Engineering Guidelines
23. Business Review Checklist
24. Version History

---

# 1. Purpose

This document defines all project-specific business rules.

Unlike other Company Engineering Standards (CES), this document is expected to change for every project.

Its purpose is to ensure that:

- Developers understand the business.
- AI coding assistants receive proper business context.
- Business logic remains centralized.
- Engineering decisions align with business expectations.

This document is the primary source of truth for all business requirements.

---

# 2. Scope

This template should be completed before development begins.

It applies to:

- Frontend Engineers
- Backend Engineers
- Full Stack Engineers
- QA Engineers
- Technical Leads
- AI Coding Assistants

---

# 3. Document Ownership

| Role | Responsibility |
|------|----------------|
| Business Analyst | Prepare business rules |
| Product Owner | Review business requirements |
| Solution Architect | Validate technical feasibility |
| VP Technology | Final approval |

Developers should never assume business logic that is not documented.

---

# 4. How to Use this Document

Before implementing any feature:

- Read this document completely.
- Understand business objectives.
- Clarify unclear requirements.
- Never invent business rules.
- If requirements are missing, raise questions before implementation.

AI Coding Assistants should always read this document before generating code.

---

# 5. Project Overview

## Project Name

```
School CRM Management System
```

---

## Business Domain

```
Education (School Management SaaS)
```

---

## Project Description

```
The application is used by schools to manage the complete academic and administrative
lifecycle — admissions, attendance, exams, fees, HR/payroll, transport, hostel, library,
inventory, and accounts — with dedicated dashboards and permissions for administrators,
teaching/non-teaching staff, students, and parents.
```

---

# 6. Business Objectives

Describe what success means for this project.

- Replace manual registers/spreadsheets for attendance, fees, and exams.
- Give parents real-time visibility into their child's attendance, marks, homework, and fees.
- Give administrators a single dashboard for enrollment, attendance, and fee collection status.
- Reduce time spent on report card generation and fee reconciliation.
- Enforce role-based data access across every module.

---

# 7. Stakeholders

Identify stakeholders.

| Stakeholder | Responsibility |
|--------------|----------------|
| School Management (Super Admin / School Admin) | Business owner, final decisions |
| Principal / Vice Principal | Academic oversight |
| Teachers / Class Teachers | Day-to-day academic delivery |
| Accountant | Fee collection, payroll, accounts |
| Receptionist | Front-desk, admissions intake |
| Librarian | Library operations |
| Students & Parents | End users / consumers of data |
| VP Technology | Engineering oversight |

---

# 8. User Roles

List every application role.

| Role | Description |
|------|-------------|
| Super Admin | Full system/tenant access, cross-school configuration |
| School Admin | Full access within a single school |
| Principal | Academic and staff oversight for the school |
| Vice Principal | Delegated academic oversight |
| Teacher | Subject teaching, marks entry, homework, attendance |
| Class Teacher | Teacher + class-level administration (promotion, class attendance) |
| Accountant | Fee collection, payroll, accounts, financial reports |
| Receptionist | Admissions intake, front-desk, visitor/enquiry management |
| Librarian | Book issue/return, library catalogue |
| Student | View own attendance, marks, homework, fees |
| Parent | View children's attendance, marks, fees, homework, notifications |

Every permission should be role-based, and every role above must also be permission-scoped
(e.g., a Class Teacher can only mark attendance for their assigned class/section).

---

# 9. Business Terminology

Define business-specific terminology.

| Term | Meaning |
|------|----------|
| Academic Year | School's annual session (e.g., 2026-2027) used to scope classes, fees, and attendance |
| Class / Section | Grade level (e.g., Class 8) and its subdivision (e.g., Section A) |
| Admission | Process of enrolling a new student into a class/section |
| Promotion | Moving a student from one class to the next at year-end |
| Fee Structure | Defined set of fee heads and amounts applicable to a class/academic year |
| Installment | A partial, scheduled payment of a fee structure |
| Homework | Task assigned by a teacher to a class, with a submission deadline |
| Grade System | Mapping of marks/percentage ranges to letter grades |
| Report Card | Consolidated result document generated after result publish |

Never assume terminology.

---

# 10. Functional Modules

List every module.

```
Authentication & Role/Permission Management     Homework & Assignments
Dashboard                                       Library Management
School Management (Branches, Classes, Timetable)  Transport Management
Student Management                              Hostel Management
Parent Management                               Fee Management
Teacher Management                              Inventory
Employee Management (HR/Payroll)                Accounts
Attendance (Student/Teacher/Staff)               Notifications & Communication
Exam Management                                  Reports & Settings
```

Each module should have clearly documented business behavior.

---

# 11. Business Rules

Document every important business rule.

## Admission / Student

Rule BR-001

```
A student must be admitted to exactly one class and section within an academic year.
```

---

Rule BR-002

```
A student cannot be promoted to the next class unless their current-year fees are cleared
or an explicit promotion override is approved by the School Admin.
```

---

Rule BR-003

```
A student record can be soft-deleted (transferred/left) but never hard-deleted.
```

---

## Attendance

Rule BR-004

```
Only a Teacher or Class Teacher assigned to a class/section can mark attendance for it.
```

---

Rule BR-005

```
Attendance for a given date cannot be marked twice for the same student; it can only be
updated by a Class Teacher or Admin.
```

---

## Exam

Rule BR-006

```
Marks can only be entered by the subject Teacher for classes/subjects assigned to them.
```

---

Rule BR-007

```
A result cannot be published until marks for all subjects of that exam are entered.
```

---

## Fee

Rule BR-008

```
A fee receipt, once generated, is immutable; corrections require a linked adjustment
entry, never an edit to the original receipt.
```

---

Rule BR-009

```
A student cannot have more outstanding installments than defined in their fee structure.
```

---

## Teacher / Employee

Rule BR-010

```
A Teacher's timetable slots must not overlap for the same academic year.
```

---

## User

Rule BR-011

```
User email addresses must be unique per tenant/school.
```

Continue documenting every business rule in this format.

---

# 12. Validation Rules

Document validation separately.

| Field | Validation |
|--------|------------|
| Email | Unique per school, required |
| Password | Minimum 8 characters, hashed (never stored plain) |
| Student Admission Number | Unique per school |
| Fee Installment Due Date | Future date only, within academic year |
| Fee Amount | Positive number |
| Marks | Between 0 and the exam's maximum marks |
| Attendance Date | Cannot be a future date |

Validation rules should never be hidden inside code — use FluentValidation validators in
`SchoolCRM.Application`.

---

# 13. Workflow Rules

Document workflow behavior.

Student Admission Lifecycle

```
Registered → Documents Verified → Admitted → Active → Promoted / Transferred / Left
```

Fee Collection Lifecycle

```
Fee Structure Assigned → Installment Due → Paid / Partially Paid / Overdue → Receipt Generated
```

Exam Result Lifecycle

```
Exam Scheduled → Marks Entry → Marks Verified → Result Published → Report Card Generated
```

Homework Lifecycle

```
Created by Teacher → Visible to Class/Student → Submitted by Student → Reviewed by Teacher → Graded
```

Every status transition should be documented.

---

# 14. Notification Rules

Document notification behavior.

Notify Parent when:

- Student marked absent
- Homework assigned
- Fee due / overdue
- Exam result published

Notify Teacher when:

- Homework submission received
- Timetable updated

Notify School Admin when:

- New admission registered
- Fee collection report ready

All real-time notifications inside the app use SignalR; SMS/Email/Push are used for
off-app delivery.

---

# 15. Reporting Requirements

Reports should answer business questions.

Dashboard

- Total Students, Teachers, Staff, Classes
- Today's Attendance
- Fees Collected / Pending
- Upcoming Exams, Today's Birthdays, Recent Admissions

Fee Report

- Collected vs pending, by class/section, by date range

Attendance Report

- Daily/monthly attendance %, by class/section/student

Exam Report

- Result summary, subject-wise performance, student ranking

Reports should describe purpose, filters, and expected output, and support Excel/PDF/CSV export.

---

# 16. Security Rules

Business-specific security.

- Parents can only view their own children's data.
- Students can only view their own attendance, marks, homework, and fees.
- Teachers can only mark attendance/enter marks for classes and subjects assigned to them.
- Accountants can collect fees but cannot alter marks or attendance.
- Only School Admin / Principal can publish exam results.
- Only Super Admin can create/manage other schools/branches in a multi-school deployment.

Business permissions must always be documented.

---

# 17. Performance Expectations

Business expectations.

Example

Dashboard

```
Load within 3 seconds.
```

Search

```
Return results within 2 seconds.
```

Large reports

```
Generated asynchronously.
```

---

# 18. Non-Functional Business Requirements

Examples

Availability

```
99.9%
```

Audit Trail

```
Every update should be logged.
```

Localization

```
Support English initially.
```

Compliance

```
GDPR
```

Business continuity requirements should be documented.

---

# 19. Assumptions

Document assumptions.

Example

- Users have internet connectivity.
- Email service is available.
- Authentication provider is operational.
- Customer data is accurate.

Assumptions reduce ambiguity.

---

# 20. Constraints

Document project constraints.

Example

- Fixed timeline
- Limited budget
- Existing technology stack
- Regulatory compliance
- Third-party dependency

---

# 21. Out of Scope

Clearly identify excluded functionality.

- Native mobile application (v1 is a responsive web app).
- Offline support.
- Live payment gateway processing (structure is ready; provider integration is a later phase).
- Live GPS hardware/biometric device integration (structure is ready; device integration is later).
- Multi-language support.

Out-of-scope items prevent scope creep.

---

# 22. AI Engineering Guidelines

When using AI Coding Assistants:

AI SHOULD

- Read this document completely.
- Never invent business rules.
- Ask for clarification if business logic is missing.
- Follow documented workflows.
- Follow role permissions.
- Generate validation based on documented rules.

AI SHOULD NOT

- Assume requirements.
- Introduce undocumented workflows.
- Create additional statuses.
- Modify business logic.
- Ignore role permissions.

Business rules always override AI assumptions.

---

# 23. Business Review Checklist

Before development begins verify:

- Project overview completed.
- Business objectives documented.
- User roles defined.
- Functional modules listed.
- Business rules documented.
- Validation rules completed.
- Workflow documented.
- Reports documented.
- Notifications documented.
- Security rules defined.
- Constraints identified.
- Out-of-scope documented.
- AI instructions reviewed.

---

# 24. Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Template | VP Technology |
| 1.1 | July 2026 | Filled in for School CRM Management System (11 roles, admission/attendance/exam/fee rules) | Solution Architect |

---

# Final Notes

This is the **only document in the Company Engineering Standards (CES)** that is expected to be customized for every project.

All engineers, architects, QA engineers, product owners, business analysts, and AI coding assistants must treat this document as the authoritative source for project-specific business behavior.

Whenever there is a conflict between implementation and this document, the documented business rules take precedence until formally revised.

---

# End of Document

**Company Engineering Standard — CES-008**  
**Business Rules Template**  
**Version 1.0**