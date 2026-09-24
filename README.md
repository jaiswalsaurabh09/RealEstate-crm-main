# Silverland CRM - Enterprise Real Estate Lead Management System

Silverland CRM is a production-ready, multi-role real estate lead lifecycle management solution built with Clean Architecture (.NET 9 Web API, Entity Framework Core 9, PostgreSQL) and a modern React 18 + TypeScript + Vite frontend.

## 🚀 Key Features

* **Dual-Role RBAC**: Enforced Admin and Employee access control levels on both backend and frontend.
* **Granular Field Auditing**: Every field mutation (Name, Mobile, Response, Employee Assignment, Enquired Project) automatically generates immutable `LeadAudit` records tracking old vs. new values.
* **4-Step Excel/CSV Import Wizard**: Upload -> Mapping -> Dry-Run Preview -> Batch Commit with duplicate detection (Skip/Update/Create) and batch tracking IDs (`IMP-20260924-001`).
* **External Integration Adapters**: Ingest leads via secure webhook tokens with `ILeadSourceProvider` pattern.
* **Indexed Mobile Search**: Fast search across historical leads and timeline change logs.

## 🔑 Demo Seed Credentials

| Role | Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@silverlandcrm.com` | `AdminPass123!` | Full System & Management Access |
| **Employee** | `employee@silverlandcrm.com` | `EmployeePass123!` | Assigned Lead Worklist & Response Updates |

## 🚦 Quick Start Guide

### 1. Run using Docker Compose (Recommended)
```bash
cp .env.example .env
docker compose up --build -d
```
- Frontend UI: http://localhost:5173
- REST Web API: http://localhost:5000/api
- Swagger OpenAPI: http://localhost:5000/swagger

### 2. Manual Local Run
```bash
# Backend
cd backend/SilverlandCRM.API
dotnet ef database update
dotnet run

# Frontend
cd frontend
npm install
npm run dev
```
