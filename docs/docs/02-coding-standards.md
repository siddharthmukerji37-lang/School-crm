# CES-002 — Company Engineering Standard
# Coding Standards

| Document ID | CES-002 |
|------------|---------|
| Version | 1.0 |
| Status | Approved |
| Owner | VP Technology |
| Audience | Software Engineers, Technical Leads, Architects, QA Engineers |
| Applies To | Frontend, Backend, Full Stack Development |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. Scope
3. Engineering Philosophy
4. General Coding Principles
5. Naming Conventions
6. File & Folder Naming
7. Variables
8. Constants
9. Functions
10. Classes
11. Interfaces
12. Enumerations
13. Code Formatting
14. Comments & Documentation
15. Error Handling
16. Logging Standards
17. Async Programming
18. Configuration Management
19. Dependency Management
20. Security Practices
21. Performance Guidelines
22. Code Review Standards
23. AI Coding Guidelines
24. Common Anti-Patterns
25. Review Checklist

---

# 1. Purpose

This document defines the coding standards that every software engineer must follow while developing software within the organization.

The purpose is to ensure:

- Consistency
- Readability
- Maintainability
- Scalability
- Security
- AI-friendly code generation

---

# 2. Scope

These standards apply to:

- Backend Applications
- Frontend Applications
- Full Stack Applications
- Internal Libraries
- Shared Components
- AI Generated Code
- Human Written Code

---

# 3. Engineering Philosophy

Every line of code should be:

- Easy to read
- Easy to understand
- Easy to maintain
- Easy to test
- Easy to extend

Write code for humans first and computers second.

---

# 4. General Coding Principles

Always follow:

- SOLID Principles
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple)
- YAGNI (You Aren't Gonna Need It)
- Separation of Concerns

Avoid clever code that reduces readability.

---

# 5. Naming Conventions

Choose meaningful names that clearly describe intent.

Good:

```text
calculateInvoiceTotal()

findActiveUsers()

sendNotification()
```

Avoid:

```text
calc()

tmp()

data()

abc()
```

Names should describe business intent rather than implementation.

---

# 6. File & Folder Naming

Use lowercase with hyphens or framework conventions.

Good

```text
user.service.ts

auth.controller.ts

invoice.repository.ts

user-profile.component.tsx
```

Avoid

```text
UserService.ts

Temp.ts

testFile.ts
```

---

# 7. Variables

Variable names should clearly express their purpose.

Good

```typescript
const activeUsers = [];

const totalInvoiceAmount = 0;
```

Avoid

```typescript
const x = [];

const a = 0;
```

Avoid abbreviations unless universally understood.

---

# 8. Constants

Constants must use UPPER_SNAKE_CASE.

Example

```typescript
MAX_LOGIN_ATTEMPTS

JWT_EXPIRATION

DEFAULT_PAGE_SIZE
```

Never hardcode magic numbers.

Bad

```typescript
if (attempt > 5)
```

Good

```typescript
if (attempt > MAX_LOGIN_ATTEMPTS)
```

---

# 9. Functions

Functions should perform one responsibility.

Guidelines

- Maximum 30 lines where practical
- Maximum 3–5 parameters (prefer object if more)
- Return predictable values
- Avoid hidden side effects

Good

```typescript
calculateTax()

validateUser()

generateInvoice()
```

Avoid

```typescript
processEverything()
```

---

# 10. Classes

Classes should represent one business responsibility.

A class should not exceed approximately 300 lines without justification.

Split large classes into smaller services.

---

# 11. Interfaces

Prefer interfaces over concrete implementations where applicable.

Example

```typescript
UserRepository

NotificationProvider

PaymentGateway
```

Program against abstractions.

---

# 12. Enumerations

Use enums for fixed values.

Example

```typescript
UserStatus

TaskPriority

InvoiceStatus

ProjectHealth
```

Avoid using string literals throughout the application.

---

# 13. Code Formatting

Use project formatter configuration.

Mandatory:

- Consistent indentation
- One statement per line
- Opening braces on same line
- Remove unused imports
- Remove dead code

Formatting should be automated using Prettier or equivalent.

---

# 14. Comments & Documentation

Code should be self-explanatory.

Write comments only when necessary.

Good comments explain:

- Why
- Business rule
- Complex algorithm

Avoid commenting obvious code.

Bad

```typescript
// Increment i

i++;
```

Good

```typescript
// Retry payment to handle temporary gateway failures.
```

---

# 15. Error Handling

Never ignore exceptions.

Avoid

```typescript
catch(error){}
```

Always:

- Log the error
- Return meaningful responses
- Preserve stack trace where appropriate

---

# 16. Logging Standards

Log important business events.

Examples

- Login
- Logout
- Payment
- Assignment
- Status change
- External API failure

Never log:

- Passwords
- Tokens
- Secrets
- Personal information

Use structured logging.

---

# 17. Async Programming

Prefer async/await.

Avoid nested callbacks.

Always handle rejected promises.

Bad

```typescript
promise.then(...)
```

Good

```typescript
await userService.findUser();
```

Never leave floating promises.

---

# 18. Configuration Management

Never hardcode:

- URLs
- Secrets
- Tokens
- Database credentials

Use environment variables.

Example

```text
DATABASE_URL

JWT_SECRET

REDIS_HOST
```

---

# 19. Dependency Management

Only introduce libraries when justified.

Before adding a package:

- Check maintenance status
- Check security advisories
- Check community adoption
- Check license compatibility

Avoid duplicate libraries solving the same problem.

---

# 20. Security Practices

Always:

- Validate input
- Sanitize output
- Escape user content where required
- Protect secrets
- Implement authorization checks
- Use secure defaults

Never trust client-side validation.

---

# 21. Performance Guidelines

Avoid:

- N+1 queries
- Duplicate API calls
- Unnecessary renders
- Blocking operations
- Repeated calculations

Optimize based on profiling, not assumptions.

---

# 22. Code Review Standards

Every Pull Request should verify:

- Code follows architecture
- Naming is meaningful
- No duplicated logic
- Tests included
- Error handling implemented
- Logging present where required
- Security reviewed
- Documentation updated

No code should be merged without review.

---

# 23. AI Coding Guidelines

When using AI coding tools:

AI SHOULD:

- Follow project architecture
- Follow coding standards
- Generate reusable code
- Generate unit tests
- Follow naming conventions
- Respect existing patterns

AI SHOULD NOT:

- Introduce unnecessary abstractions
- Duplicate business logic
- Ignore linting rules
- Ignore formatting
- Create unused code
- Add dependencies without justification

Every AI-generated change must be reviewed before merge.

---

# 24. Common Anti-Patterns

Avoid the following:

- God Classes
- Fat Controllers
- Duplicate Code
- Long Methods
- Nested Conditionals
- Hardcoded Values
- Commented Dead Code
- Copy-Paste Programming
- Circular Dependencies
- Unused Variables
- Unused Imports
- Silent Exception Handling
- Deep Nesting
- Mixed Responsibilities

---

# 25. Coding Review Checklist

Before committing code verify:

- Naming is meaningful.
- Functions are small.
- Classes have one responsibility.
- No duplicated logic.
- Code passes linting.
- Code is formatted.
- Tests pass.
- No hardcoded values.
- Configuration uses environment variables.
- Logging follows standards.
- Error handling is implemented.
- Documentation updated if necessary.
- AI-generated code has been reviewed.
- Pull Request is ready for review.

---

# Final Engineering Principles

Every engineer should strive to write code that is:

- Readable
- Maintainable
- Testable
- Secure
- Performant
- Reusable
- Consistent
- Predictable
- AI-Friendly

Good code is not measured by how quickly it is written, but by how easily it can be understood, maintained, and extended by both engineers and AI-assisted development tools.

---

# Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Release | VP Technology |

---

# End of Document

**Company Engineering Standard — CES-002**  
**Coding Standards**  
**Version 1.0**