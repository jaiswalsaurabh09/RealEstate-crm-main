# Quick Start — Silverland CRM

1. Push this repository to GitHub.
2. Create a Supabase PostgreSQL project.
3. Create the Render API Web Service using the backend Dockerfile.
4. Add production environment variables.
5. Create the Render Static Site using `frontend`.
6. Add `VITE_API_URL`.
7. Run database migrations.
8. Test `/health`.
9. Test the CRM.

Free Render services may sleep when inactive, so the first request after inactivity can take longer.

For company data, keep a separate database backup/export strategy.
