# System Architecture
## Weather Comfort Analytics Dashboard

---

## 1. Architecture Overview

The Weather Comfort Analytics Dashboard is a **two-tier web application** consisting of:

- **Frontend**: Angular (client-side SPA)
- **Backend**: ASP.NET Core Web API
- **External Services**:
  - OpenWeatherMap API (weather data)
  - Auth0 (authentication & authorization)

The system is designed using a **pragmatic layered architecture**, intentionally avoiding heavy architectural patterns (e.g., full Clean Architecture) due to the project’s limited scope and time constraints.

---

## 2. High-Level System Diagram (Conceptual)

[ Browser (Angular) ]
|
| HTTPS + JWT
v
[ ASP.NET Core Web API ]
|
| Cached HTTP Calls
v
[ OpenWeatherMap API ]


Authentication is enforced at the API boundary using **Auth0-issued JWT tokens**.

---

## 3. Backend Architecture (ASP.NET Core Web API)

### 3.1 Project Structure

server/
├─ Controllers/
├─ Services/
├─ Infrastructure/
├─ Models/
├─ DTOs/
├─ Middleware/
└─ Config/


This structure provides:
- Clear separation of responsibilities
- Minimal boilerplate
- Easy testability and explainability

---

### 3.2 Controllers

**Responsibilities**
- Expose REST endpoints
- Validate input
- Delegate work to services
- Enforce authorization

**Examples**
- `WeatherController`
- `CacheDebugController`

Controllers remain **thin** and do not contain business logic.

---

### 3.3 Services Layer

**Responsibilities**
- Core business logic
- Comfort Index calculation
- Ranking logic
- Coordination of caching and external API calls

**Key Services**
- `WeatherService`
- `ComfortIndexService`
- `CacheService`

The Comfort Index is computed **entirely on the backend** to meet assignment requirements.

---

### 3.4 Infrastructure Layer

**Responsibilities**
- External integrations
- Low-level technical concerns

**Examples**
- `OpenWeatherClient`
- Auth0 configuration
- HTTP client configuration

External dependencies are isolated here to avoid coupling business logic to infrastructure concerns.

---

### 3.5 Models & DTOs

- **Models** represent internal domain concepts
- **DTOs** define API request/response contracts

This separation ensures:
- Stable API contracts
- Flexibility for internal changes

---

### 3.6 Caching Strategy

- **Raw OpenWeatherMap responses** are cached for **5 minutes**
- Cache keys are city-based
- Processed Comfort Index results may be cached separately

Caching is implemented server-side using:
- `IMemoryCache` (default)
- Easily replaceable with Redis if required

A debug endpoint exposes cache status:
- `HIT`
- `MISS`

---

### 3.7 Authentication & Authorization (Auth0)

- JWT Bearer authentication
- Token validation middleware
- All API endpoints require authentication
- MFA enabled via Auth0 email verification
- Public signups disabled
- Whitelisted users only

No authentication logic exists in controllers beyond authorization attributes.

---

## 4. Frontend Architecture (Angular)

### 4.1 Project Structure

client/src/app/
├─ core/
│ ├─ auth/
│ ├─ guards/
│ └─ interceptors/
├─ features/
│ └─ dashboard/
├─ shared/
│ ├─ models/
│ └─ services/
└─ app-routing.module.ts


---

### 4.2 Core Module

**Responsibilities**
- Authentication integration (Auth0)
- HTTP interceptors
- Route guards

JWT tokens are automatically attached to outgoing API requests.

---

### 4.3 Features Module

- `dashboard` feature displays:
  - City name
  - Weather description
  - Temperature (°C)
  - Comfort Index
  - Rank

The UI is designed mobile-first and adapts to desktop layouts.

---

### 4.4 Shared Module

Contains reusable:
- Models
- API services
- UI utilities

This avoids duplication across features.

---

## 5. API Communication

- Angular communicates with the backend via HTTPS
- JWT tokens are included using HTTP interceptors
- Backend validates tokens using Auth0 middleware
- All sensitive logic remains server-side

---

## 6. Comfort Index Design Responsibility

- Comfort Index computation exists **only in the backend**
- Frontend receives computed scores as part of API responses
- Formula is documented in README for transparency

This design ensures:
- Consistency
- Security
- Interview-friendly explainability

---

## 7. Design Decisions & Trade-offs

### Why Not Full Clean Architecture?
- Project scope is limited
- No long-term maintenance requirement
- Overhead would reduce delivery speed

### Why This Architecture?
- Clear separation of concerns
- Easy to reason about
- Easy to extend
- Ideal for time-boxed technical assessments

---

## 8. Future Evolution

If the project were to scale:
- Introduce Redis for distributed caching
- Add persistence layer
- Introduce background jobs
- Gradually evolve toward Clean Architecture

---p

## 9. Summary

This architecture:
- Meets all assignment requirements
- Avoids unnecessary complexity
- Supports rapid development
- Aligns with BMad and Cursor.ai workflows
- Is easy to explain and modify during interviews
