# Backend Architecture (ASP.NET Core Web API)

## 3.1 Project Structure

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

## 3.2 Controllers

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

## 3.3 Services Layer

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

## 3.4 Infrastructure Layer

**Responsibilities**
- External integrations
- Low-level technical concerns

**Examples**
- `OpenWeatherClient`
- Auth0 configuration
- HTTP client configuration

External dependencies are isolated here to avoid coupling business logic to infrastructure concerns.

---

## 3.5 Models & DTOs

- **Models** represent internal domain concepts
- **DTOs** define API request/response contracts

This separation ensures:
- Stable API contracts
- Flexibility for internal changes

---

## 3.6 Caching Strategy

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

## 3.7 Authentication & Authorization (Auth0)

- JWT Bearer authentication
- Token validation middleware
- All API endpoints require authentication
- MFA enabled via Auth0 email verification
- Public signups disabled
- Whitelisted users only

No authentication logic exists in controllers beyond authorization attributes.

---

