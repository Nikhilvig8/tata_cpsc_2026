# Security Policy

## Supported versions

Only the current `main` branch (the version deployed to production) receives security fixes.

## Reporting a vulnerability

Please **do not** open a public GitHub issue for security problems.

Report it privately through GitHub: **Security** tab → **Report a vulnerability**
(GitHub private vulnerability reporting). Include:

- the affected page, controller or file,
- steps to reproduce, and
- the impact you observed.

We aim to acknowledge reports within 5 working days and to share a fix timeline
once the issue is confirmed.

## Handling secrets

Connection strings and application secrets are never committed. They live in the
git-ignored `connectionStrings.config` and `secrets.config` files (templates:
`*.config.example`). If a secret is ever committed by mistake, treat it as leaked
and rotate it.

See `SECURITY_REMEDIATION.md` for the history of security fixes.
