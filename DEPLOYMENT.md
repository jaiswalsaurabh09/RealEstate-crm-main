# Silverland CRM — Simple Free Deployment

This deployment is intentionally independent of CircleOra and the existing Oracle server.

## Architecture

GitHub → Render Frontend + Render .NET API → Supabase PostgreSQL

CircleOra/Oracle is not used by this deployment.

## 1. Supabase

Create a new Supabase project for CRM only.

Copy the PostgreSQL connection string and configure it in Render as:

`ConnectionStrings__DefaultConnection`

Do not commit the connection string.

## 2. Render backend

Create a Web Service from this GitHub repository.

- Name: `silverland-crm-api`
- Runtime: Docker
- Dockerfile: `backend/SilverlandCRM.API/Dockerfile`
- Plan: Free
- Health check: `/health`

Environment variables:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key` — generate a long random secret
- `Jwt__Issuer=SilverlandCRM`
- `Jwt__Audience=SilverlandCRM`
- `Cors__AllowedOrigins=https://silverland-crm.onrender.com`
- `ASPNETCORE_ENVIRONMENT=Production`

## 3. Render frontend

Create a Static Site from the same repository.

- Name: `silverland-crm`
- Root Directory: `frontend`
- Build Command: `npm ci && npm run build`
- Publish Directory: `dist`
- Plan: Free

Environment variable:

`VITE_API_URL=https://silverland-crm-api.onrender.com`

If Render assigns a different frontend URL, update `Cors__AllowedOrigins` in the backend.

## 4. Database migration

Before using the production CRM, apply the EF Core migrations to the new Supabase database.

Do not use `EnsureDeleted()` or destructive database commands in production.

## 5. Verify

Open:

`https://silverland-crm-api.onrender.com/health`

Then open:

`https://silverland-crm.onrender.com`

Test:

- Admin login
- Employee login
- Employee enable/disable
- Project management
- Lead creation
- Excel import
- Audit logs
- Role restrictions

## Important

Do NOT connect this CRM to CircleOra's database, Redis, Kafka, Elasticsearch, Docker network, environment variables, or Oracle server.
