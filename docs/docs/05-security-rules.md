# CES-005 — Company Engineering Standard
# Security Standards

| Document ID | CES-005 |
|------------|---------|
| Version | 1.0 |
| Status | Approved |
| Owner | VP Technology |
| Audience | Software Architects, Technical Leads, Software Engineers, QA Engineers, DevOps Engineers |
| Applies To | Frontend, Backend, Full Stack, APIs, Databases, Infrastructure, AI-Assisted Development |
| Last Updated | July 2026 |

---

# Table of Contents

1. Purpose
2. Scope
3. Security Philosophy
4. Security by Design
5. Authentication Standards
6. Authorization Standards
7. Password Management
8. Session Management
9. JWT Standards
10. Input Validation
11. Output Encoding
12. SQL Injection Prevention
13. Cross Site Scripting (XSS)
14. Cross Site Request Forgery (CSRF)
15. File Upload Security
16. API Security
17. Sensitive Data Protection
18. Secrets Management
19. Logging & Auditing
20. Database Security
21. Infrastructure Security
22. Dependency Security
23. Secure Coding Practices
24. AI Engineering Security Guidelines
25. Security Review Checklist
26. Common Security Anti-Patterns
27. OWASP Top 10 Reference

---

# 1. Purpose

This document defines the minimum security standards that every software project must follow throughout the Software Development Life Cycle (SDLC).

The objective is to ensure:

- Confidentiality
- Integrity
- Availability
- Secure by Design
- Secure by Default
- Compliance with OWASP recommendations
- AI-generated code follows enterprise security practices

Security is everyone's responsibility.

---

# 2. Scope

These standards apply to:

- Web Applications
- REST APIs
- Microservices
- Internal Applications
- Public Applications
- Mobile Backends
- Databases
- Cloud Infrastructure
- AI Generated Code
- Human Written Code

---

# 3. Security Philosophy

Security must be built into the application from the beginning.

Never treat security as:

- a final testing phase
- an optional enhancement
- a customer request

Every engineer is responsible for writing secure software.

---

# 4. Security by Design

Applications must follow these principles:

- Least Privilege
- Defense in Depth
- Zero Trust
- Fail Secure
- Secure by Default
- Complete Mediation
- Minimize Attack Surface

Security reviews should occur during:

- Design
- Development
- Code Review
- Testing
- Deployment

---

# 5. Authentication Standards

All protected resources must require authentication.

Preferred authentication mechanisms:

- JWT
- OAuth 2.0
- OpenID Connect
- SAML (Enterprise Integrations)

Authentication must:

- verify user identity
- expire inactive sessions
- prevent replay attacks

Never implement custom authentication algorithms.

---

# 6. Authorization Standards

Authentication identifies the user.

Authorization determines what the user can do.

Every protected API must verify:

- User Role
- Permission
- Resource Ownership
- Business Rules

Never trust frontend authorization.

Always validate authorization on the server.

---

# 7. Password Management

Passwords must:

- Never be stored in plain text
- Never be logged
- Never be emailed

Password hashing:

Recommended:

- Argon2
- bcrypt

Minimum Requirements:

- Minimum length: 8 characters
- Strong password policy
- Password history (where applicable)

Never implement your own hashing algorithm.

---

# 8. Session Management

Sessions should:

- Expire automatically
- Support logout
- Support refresh token rotation
- Be invalidated after password reset

Cookies should be:

- HttpOnly
- Secure
- SameSite=Lax or Strict

---

# 9. JWT Standards

JWT should:

- Have expiration
- Be signed
- Be validated
- Use secure algorithms

Claims should include:

- User ID
- Role
- Expiration
- Issued At

Never store sensitive information inside JWT payloads.

---

# 10. Input Validation

All user input is considered untrusted.

Validate:

- Type
- Length
- Format
- Range
- Business Rules
- Enum values

Perform validation:

- Frontend
- Backend

Backend validation is mandatory.

---

# 11. Output Encoding

All user-generated content must be safely encoded before rendering.

Protect against:

- HTML Injection
- JavaScript Injection
- Template Injection

Never trust stored data.

---

# 12. SQL Injection Prevention

Always use:

- ORM
- Prepared Statements
- Parameterized Queries

Never concatenate SQL strings.

❌ Bad

```sql
SELECT * FROM users WHERE email = '" + email + "'";
```

✅ Good

```sql
SELECT * FROM users WHERE email = ?
```

---

# 13. Cross Site Scripting (XSS)

Prevent XSS by:

- Escaping HTML
- Encoding output
- Sanitizing input
- Using Content Security Policy (CSP)

Never render raw HTML unless explicitly required and sanitized.

---

# 14. Cross Site Request Forgery (CSRF)

Protect state-changing requests using:

- CSRF Tokens
- SameSite Cookies
- Origin Validation

Avoid disabling CSRF protection unless justified.

---

# 15. File Upload Security

Validate:

- MIME Type
- Extension
- Maximum Size
- Virus Scan (if applicable)

Store uploaded files:

- Outside application source
- With randomized names

Never execute uploaded files.

---

# 16. API Security

Every protected API should implement:

- Authentication
- Authorization
- Validation
- Rate Limiting
- Logging
- HTTPS
- CORS

Sensitive APIs should support:

- Audit Logging
- Idempotency
- Request Correlation IDs

---

# 17. Sensitive Data Protection

Sensitive information includes:

- Passwords
- Tokens
- API Keys
- Personal Information
- Financial Data
- Health Data

Never expose sensitive data through:

- API responses
- Logs
- Browser Console
- Source Code

Mask sensitive values whenever displayed.

---

# 18. Secrets Management

Never store secrets inside:

- Source Code
- Git Repository
- Documentation

Use:

- Environment Variables
- Secret Managers
- Vault Services

Examples:

```
DATABASE_URL

JWT_SECRET

AWS_SECRET_KEY

SMTP_PASSWORD
```

---

# 19. Logging & Auditing

Log:

- Login
- Logout
- Password Reset
- Role Changes
- Permission Changes
- Failed Authentication
- Critical Business Events

Never log:

- Passwords
- JWT Tokens
- Secrets
- API Keys
- Credit Card Numbers

Logs should support auditing and incident investigation.

---

# 20. Database Security

Database access should follow least privilege.

Recommendations:

- Separate application users
- Read-only accounts where applicable
- Encrypted backups
- Connection encryption (TLS)
- Database firewall

Never expose databases publicly.

---

# 21. Infrastructure Security

Infrastructure should enforce:

- HTTPS
- Firewall Rules
- Network Segmentation
- Automatic Security Updates
- Monitoring
- Backup Strategy

Disable:

- Unused Ports
- Unused Services
- Default Credentials

---

# 22. Dependency Security

Before adding any dependency:

Verify:

- Maintenance status
- Security vulnerabilities
- Community support
- License compatibility

Run dependency scanning regularly.

Remove unused packages.

---

# 23. Secure Coding Practices

Always:

- Validate Input
- Sanitize Output
- Handle Errors Securely
- Encrypt Sensitive Data
- Follow Principle of Least Privilege
- Use HTTPS
- Implement Rate Limiting
- Review Third-party Libraries

Never:

- Hardcode Secrets
- Disable Security Checks
- Trust Client Input
- Expose Internal Errors
- Ignore Security Warnings

---

# 24. AI Engineering Security Guidelines

When using AI coding assistants:

AI SHOULD:

- Generate secure code
- Validate input
- Use parameterized queries
- Follow authentication standards
- Follow authorization standards
- Use secure libraries
- Generate security tests

AI SHOULD NOT:

- Hardcode secrets
- Disable validation
- Disable authentication
- Ignore authorization
- Generate insecure SQL
- Return stack traces
- Use deprecated libraries

Developers are responsible for reviewing all AI-generated code.

---

# 25. Security Review Checklist

Before merging code verify:

- Authentication implemented.
- Authorization verified.
- Input validation completed.
- Output encoding applied.
- SQL Injection prevented.
- XSS protection implemented.
- CSRF protection implemented.
- File uploads secured.
- Secrets protected.
- Logging implemented.
- Sensitive data masked.
- Dependency scan completed.
- HTTPS enforced.
- Security tests passed.

---

# 26. Common Security Anti-Patterns

Never:

- Hardcode Passwords
- Store Plain Text Passwords
- Disable Authentication
- Disable Authorization
- Trust Client Validation
- Use Dynamic SQL
- Log Sensitive Data
- Commit Secrets to Git
- Ignore Security Warnings
- Disable SSL Verification
- Return Stack Traces
- Use Default Credentials
- Ignore Dependency Vulnerabilities

---

# 27. OWASP Top 10 Reference

Developers must understand and actively prevent vulnerabilities described in the OWASP Top 10, including:

- Broken Access Control
- Cryptographic Failures
- Injection
- Insecure Design
- Security Misconfiguration
- Vulnerable Components
- Authentication Failures
- Software & Data Integrity Failures
- Logging & Monitoring Failures
- Server-Side Request Forgery (SSRF)

Security reviews should include verification against these categories.

---

# Final Security Principles

Every application should be:

- Secure by Design
- Secure by Default
- Least Privileged
- Properly Authenticated
- Properly Authorized
- Fully Validated
- Fully Auditable
- Resilient Against Common Attacks
- AI-Assisted but Human Reviewed

Security is not a feature.

Security is a fundamental engineering responsibility that must be considered in every design decision, every line of code, every deployment, and every code review.

---

# Version History

| Version | Date | Description | Author |
|----------|------|-------------|--------|
| 1.0 | July 2026 | Initial Release | VP Technology |

---

# End of Document

**Company Engineering Standard — CES-005**  
**Security Standards**  
**Version 1.0**