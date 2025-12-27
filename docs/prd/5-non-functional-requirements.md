# Non-Functional Requirements

## Performance
- Cached responses should be served whenever available.
- Dashboard load times should remain within acceptable limits (<2 seconds typical).

## Security
- All backend endpoints must be protected by JWT validation.
- No API keys or secrets shall be exposed to the frontend.

## Maintainability
- Business logic must be separated from controllers.
- External API integrations must be isolated in dedicated services.

