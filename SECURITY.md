# Security Policy

## Supported Versions

| Version | Supported |
|---|---|
| 1.0.x (beta) | ✅ |

## Reporting a Vulnerability

**Do NOT open a public GitHub issue for security vulnerabilities.**

Please email the maintainers directly with:
1. A description of the vulnerability
2. Steps to reproduce
3. Potential impact

We will respond within 48 hours and coordinate a fix before any public disclosure.

## Security Notes

- All JWT secrets in `docker-compose.yml` are for **local development only**
- Before deploying to production, replace all secrets with environment-specific values via a secrets manager (e.g. AWS Secrets Manager, Azure Key Vault)
- Never commit `appsettings.Development.json` or `.env` files containing real credentials
