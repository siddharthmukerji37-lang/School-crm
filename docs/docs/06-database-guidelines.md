# CES-006 — Company Engineering Standard
# Database Design & Development Guidelines

| Document ID | CES-006 |
|------------|---------|
| Version | 1.0 |
| Status | Approved |
| Owner | VP Technology |
| Audience | Software Architects, Backend Engineers, Full Stack Engineers, Database Engineers |
| Applies To | PostgreSQL, MySQL, SQL Server, MongoDB, DocumentDB and Enterprise Data Stores |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. Scope
3. Database Design Philosophy
4. General Design Principles
5. Naming Conventions
6. Primary Keys
7. Foreign Keys
8. Table Design
9. Column Standards
10. Data Types
11. Relationships
12. Normalization
13. Denormalization
14. Indexing Strategy
15. Constraints
16. Audit Columns
17. Soft Delete Strategy
18. Transactions
19. Concurrency Management
20. Query Standards
21. Performance Guidelines
22. Database Security
23. Backup & Recovery
24. Migration Standards
25. ORM Guidelines
26. AI Engineering Guidelines
27. Database Review Checklist
28. Common Anti-Patterns

---

# 1. Purpose

This document defines the database design and development standards for all software projects within the organization.

The objectives are:

- Maintainable database schema
- Consistent naming
- High performance
- Data integrity
- Scalability
- Security
- AI-assisted database development consistency

---

# 2. Scope

These standards apply to:

- PostgreSQL
- MySQL
- SQL Server
- MongoDB
- AWS DocumentDB
- Cloud Databases
- Enterprise Applications
- AI Generated Database Schemas
- Human Designed Schemas

---

# 3. Database Design Philosophy

A database should be:

- Simple
- Consistent
- Normalized
- Scalable
- Secure
- Maintainable
- Well documented

Design databases around business domains rather than application screens.

---

# 4. General Design Principles

Every database should follow:

- Single Source of Truth
- Data Integrity
- Referential Integrity
- Consistent Naming
- Minimize Data Duplication
- Optimize for Readability first
- Optimize for Performance when required

Business rules belong in the application layer unless database constraints are necessary.

---

# 5. Naming Conventions

## Tables

Use plural nouns.

Good

```
users

projects

tasks

project_members
```

Avoid

```
User

tblUsers

ProjectMaster
```

---

## Columns

Use snake_case.

Good

```
first_name

created_at

project_status
```

Avoid

```
FirstName

firstName

createdDate
```

---

## Primary Keys

Always use

```
id
```

Avoid

```
user_id

tblUserId
```

as the primary key name.

---

## Foreign Keys

Use:

```
user_id

project_id

company_id
```

---

# 6. Primary Keys

Preferred options:

- UUID (Enterprise Applications)
- BIGINT AUTO_INCREMENT (where applicable)

Rules:

- Every table must have a primary key.
- Primary keys should never change.
- Avoid composite primary keys unless absolutely necessary.

---

# 7. Foreign Keys

Use foreign key constraints whenever supported.

Benefits:

- Data integrity
- Easier joins
- Referential consistency

Never store orphan records intentionally.

---

# 8. Table Design

Every table should represent one business entity.

Example

```
users

projects

tasks

roles

permissions
```

Avoid mixing multiple business entities into one table.

---

# 9. Column Standards

Every column should have:

- Appropriate data type
- Nullability defined
- Default value where appropriate
- Clear purpose

Avoid generic columns such as:

```
field1

value

misc
```

---

# 10. Data Types

Use the smallest appropriate data type.

Examples

```
BOOLEAN

INTEGER

BIGINT

UUID

VARCHAR

TEXT

DATE

TIMESTAMP

JSONB (PostgreSQL)
```

Avoid storing dates as strings.

Avoid storing numbers inside VARCHAR columns.

---

# 11. Relationships

Supported relationships:

One-to-One

One-to-Many

Many-to-Many

Many-to-Many relationships should use junction tables.

Example

```
project_members

user_roles

role_permissions
```

---

# 12. Normalization

Follow normalization up to Third Normal Form (3NF) unless justified.

Benefits

- Reduced duplication
- Easier maintenance
- Better consistency

---

# 13. Denormalization

Denormalize only when:

- Performance requires it
- Read-heavy systems justify it
- Documented architectural decision exists

Never denormalize for convenience.

---

# 14. Indexing Strategy

Indexes should support:

- Foreign Keys
- Frequently searched columns
- Frequently sorted columns
- Frequently filtered columns

Examples

```
email

created_at

status

project_id
```

Avoid unnecessary indexes.

Too many indexes slow writes.

---

# 15. Constraints

Use database constraints whenever possible.

Examples

- PRIMARY KEY
- FOREIGN KEY
- UNIQUE
- CHECK
- NOT NULL

Do not rely solely on application validation.

---

# 16. Audit Columns

Every business table should contain:

```
created_at

created_by

updated_at

updated_by
```

Optional

```
deleted_at

deleted_by
```

These fields improve traceability.

---

# 17. Soft Delete Strategy

Business entities should support soft delete where applicable.

Recommended columns

```
deleted_at

deleted_by

is_deleted
```

Avoid permanently deleting business-critical data.

---

# 18. Transactions

Use transactions whenever multiple operations must succeed together.

Examples

- Invoice Creation
- Payment Processing
- Order Placement
- User Registration

Transactions should be:

- Short
- Atomic
- Reliable

---

# 19. Concurrency Management

Protect against concurrent updates.

Strategies

- Optimistic Locking
- Pessimistic Locking
- Row Versioning

Choose the appropriate strategy based on business requirements.

---

# 20. Query Standards

Queries should:

- Select only required columns
- Use indexes efficiently
- Avoid SELECT *
- Use pagination
- Avoid unnecessary joins

Good

```sql
SELECT id, name, email
FROM users;
```

Avoid

```sql
SELECT *
FROM users;
```

---

# 21. Performance Guidelines

Monitor:

- Slow Queries
- Missing Indexes
- Execution Plans
- Table Growth
- Lock Contention

Recommendations

- Batch operations
- Connection pooling
- Query optimization
- Proper indexing
- Database caching

Optimize based on measurement.

---

# 22. Database Security

Apply:

- Least Privilege
- Encryption at Rest
- Encryption in Transit
- Strong Authentication
- Role-based Database Access

Never expose databases directly to the internet.

---

# 23. Backup & Recovery

Every production database must have:

- Automated backups
- Recovery testing
- Backup retention policy
- Disaster recovery plan

Backups should be encrypted.

Recovery procedures should be documented.

---

# 24. Migration Standards

Every schema change must be version controlled.

Migration rules:

- Forward-only migrations
- No manual production schema changes
- Tested before deployment
- Rollback plan available

Never modify production databases manually.

---

# 25. ORM Guidelines

Recommended ORMs

- Prisma
- Sequelize
- TypeORM
- Entity Framework
- Hibernate

Guidelines

- Keep entities simple
- Avoid business logic in models
- Use repositories
- Avoid N+1 queries
- Review generated SQL

ORM convenience should not replace database knowledge.

---

# 26. AI Engineering Guidelines

When generating database schemas using AI:

AI SHOULD

- Follow naming conventions
- Normalize appropriately
- Generate foreign keys
- Generate indexes
- Include audit columns
- Generate migration scripts
- Use appropriate data types

AI SHOULD NOT

- Create unnecessary tables
- Ignore constraints
- Store everything as TEXT
- Skip indexing
- Duplicate data
- Generate poor naming conventions

Developers must review all AI-generated schemas before implementation.

---

# 27. Database Review Checklist

Before approving a schema verify:

- Naming conventions followed.
- Tables represent business entities.
- Data types appropriate.
- Primary keys defined.
- Foreign keys implemented.
- Constraints applied.
- Indexes created.
- Audit columns present.
- Soft delete considered.
- Migrations included.
- Security reviewed.
- Backup strategy documented.
- Performance reviewed.

---

# 28. Common Anti-Patterns

Avoid:

- SELECT *
- Missing Primary Keys
- Missing Foreign Keys
- Duplicate Data
- Large VARCHAR for every column
- Storing Dates as Strings
- Storing JSON unnecessarily
- Missing Indexes
- Excessive Indexes
- Manual Database Changes
- Business Logic inside Database Triggers
- Circular Relationships
- Poor Naming
- Hardcoded IDs

---

# Final Engineering Principles

A well-designed database should be:

- Consistent
- Scalable
- Secure
- Performant
- Normalized
- Easy to Maintain
- Easy to Understand
- AI-Friendly

A database is considered successful when developers, database administrators, reporting systems, and AI coding assistants can understand its structure, extend it safely, and maintain data integrity without introducing unnecessary complexity.

---

# Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Release | VP Technology |

---

# End of Document

**Company Engineering Standard — CES-006**  
**Database Design & Development Guidelines**  
**Version 1.0**