# Inventory Hold Microservice

## Overview
This repository contains a full-stack inventory hold microservice with:
- .NET 10 API using Domain-Driven Design
- MongoDB for inventory and holds persistence
- Redis caching for inventory read performance
- RabbitMQ event publishing
- React + TypeScript frontend using Vite
- Unit test coverage with NUnit and Moq
- Docker Compose one-command startup

## Run locally
From the repository root:

```bash
docker-compose up --build
```

Then open:
- Frontend: http://localhost:4173
- Backend API: http://localhost:5000/swagger
- RabbitMQ management UI: http://localhost:15672 (guest/guest)

## Backend architecture
- `Backend/src/InventoryHold.Contracts` — API DTOs and response models
- `Backend/src/InventoryHold.Domain` — domain entities, repository/service interfaces, business logic
- `Backend/src/InventoryHold.Infrastructure` — MongoDB/Redis/RabbitMQ implementations and background expiry service
- `Backend/src/InventoryHold.WebApi` — controllers, DI, and startup
- `Backend/src/InventoryHold.UnitTests` — unit tests with mocked dependencies

## API endpoints
- `POST /api/holds` — create a new hold
- `GET /api/holds/{holdId}` — retrieve a hold
- `DELETE /api/holds/{holdId}` — release a hold
- `GET /api/inventory` — view inventory levels

## Frontend
The React frontend provides:
- Inventory dashboard
- Create hold form
- Active hold list with release support

## Notes
- Inventory read responses are cached in Redis with a TTL and cache invalidation on hold mutations.
- Holds are expired automatically by a background hosted service and also detected on access.
- MongoDB stock adjustments are performed with atomic update filters.
