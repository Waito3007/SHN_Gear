# SHNGear

SHNGear is a full-stack e-commerce platform built with ASP.NET Core and React.

It includes product/catalog management, cart, checkout, order lifecycle, PayPal integration, refund workflows, email notifications, Redis caching, and containerized local development.

## Highlights

- Layered backend architecture with service, repository, and infrastructure separation.
- Payment flow with strategy-based providers (COD and PayPal).
- Checkout and webhook idempotency safeguards to prevent duplicate processing.
- Redis-backed caching for product data and shopping cart state.
- Email workflows for OTP/auth and order/refund notifications.
- Docker Compose local stack with PostgreSQL, Redis, backend, frontend, pgAdmin, and Mailpit.

## Repository Structure

- SHNGearBE: ASP.NET Core Web API backend.
- SHNGearFE: React + TypeScript frontend (Vite).
- SHNGearBE.Tests: backend unit and integration tests.
- SHNGearMailService: reusable mail module.
- SANBGLog: background logging module.
- docker-compose.yml: local orchestration for all services.

## Tech Stack

Backend:

- .NET 8, ASP.NET Core Web API
- Entity Framework Core (PostgreSQL)
- Redis distributed cache
- JWT authentication and authorization
- PayPal REST integration

Frontend:

- React 19, TypeScript, Vite
- React Router
- Axios
- Tailwind CSS

Infra:

- Docker and Docker Compose
- PostgreSQL, Redis, pgAdmin, Mailpit

## Architecture Notes

SHNGearBE is implemented as a layered monolith.

- Controllers handle HTTP transport concerns.
- Services contain business workflows.
- Repositories encapsulate data access patterns.
- Infrastructure contains integrations such as Redis, PayPal, media storage, and mail.

This is not a strict multi-project Clean Architecture implementation. It applies clean architecture principles inside a single API project.

## Core Features

### Product and Catalog

- Product CRUD with category/brand metadata.
- Variant and price support.
- Cached product reads and list scenarios in product repository.

### Cart

- Redis-backed cart storage by account.
- Cart enrichment from DB for price, stock, and product details.
- Quantity and stock validations.

### Order and Payment

- Checkout flow with COD and PayPal strategies.
- PayPal order creation and webhook processing.
- Idempotency key support for checkout request replay protection.
- Webhook event deduplication and tracking.

### Refund Workflow

- Customer cancel logic by order state.
- Admin refund approval workflow.
- Refund record persistence and over-refund protection.

### Notifications and Email

- OTP and auth-related mail support.
- Order placed email.
- Refund completed email.

## Quick Start (Docker Recommended)

1. Clone and move to repository root.
2. Copy environment template:

   copy .env.example .env

3. Fill required credentials in .env (PayPal, email, optional Cloudinary).
4. Start all services:

   docker compose up -d --build

5. Open services:

- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- pgAdmin: http://localhost:5050
- Mailpit: http://localhost:8025

## Local Development Without Docker

### Prerequisites

- .NET SDK 8
- Node.js 20+
- PostgreSQL 16+ (or compatible)
- Redis 7+

### Backend

1. Configure SHNGearBE/appsettings.Development.json or environment variables.
2. Run backend:

   dotnet run --project SHNGearBE/SHNGearBE.csproj

Notes:

- The API applies EF migrations on startup.
- Default backend URL is http://localhost:5000.

### Frontend

1. Install dependencies:

   cd SHNGearFE
   npm ci

2. Run dev server:

   npm run dev

3. Build frontend:

   npm run build

## Configuration

Primary environment keys are documented in .env.example.

Important groups:

- Database:
  - POSTGRES_USER
  - POSTGRES_PASSWORD
  - POSTGRES_DB
  - POSTGRES_PORT

- Service ports:
  - BACKEND_PORT
  - FRONTEND_PORT
  - REDIS_PORT

- CORS:
  - CORS_ALLOWED_ORIGIN_1
  - CORS_ALLOWED_ORIGIN_2

- Email:
  - EMAIL_SMTP_HOST
  - EMAIL_SMTP_PORT
  - EMAIL_SMTP_USE_SSL
  - EMAIL_SMTP_USERNAME
  - EMAIL_SMTP_PASSWORD
  - EMAIL_FROM_ADDRESS
  - EMAIL_FROM_DISPLAY_NAME
  - EMAIL_TIMEOUT_SECONDS

- PayPal:
  - PAYPAL_BASE_URL
  - PAYPAL_CLIENT_ID
  - PAYPAL_CLIENT_SECRET
  - PAYPAL_WEBHOOK_ID
  - PAYPAL_CURRENCY_CODE
  - PAYPAL_HTTP_TIMEOUT_SECONDS
  - EXCHANGE_RATE_API_BASE_URL
  - EXCHANGE_RATE_API_KEY
  - PAYPAL_FALLBACK_VND_PER_USD_RATE

## Testing

Backend test project:

    dotnet test SHNGearBE.Tests/SHNGearBE.Tests.csproj

If integration tests require Docker/Testcontainers, ensure Docker is running.

## Logging and Observability

- Background logging is provided by SANBGLog.
- Typed log service is registered in backend startup.
- Mailpit can be used locally to inspect outgoing emails.

## Known Notes

- Some external flows require credentials (PayPal, SMTP) before full end-to-end validation.
- Frontend build tooling follows the Node version requirement from installed Vite.

## Contribution

1. Create a feature branch from main.
2. Keep architecture boundaries clear (controller, service, repository, infrastructure).
3. Add or update tests for business-critical changes.
4. Open a PR with a clear summary and verification steps.
