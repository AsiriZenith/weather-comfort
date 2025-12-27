# Frontend Architecture (Angular)

## 4.1 Project Structure

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

## 4.2 Core Module

**Responsibilities**
- Authentication integration (Auth0)
- HTTP interceptors
- Route guards

JWT tokens are automatically attached to outgoing API requests.

---

## 4.3 Features Module

- `dashboard` feature displays:
  - City name
  - Weather description
  - Temperature (°C)
  - Comfort Index
  - Rank

The UI is designed mobile-first and adapts to desktop layouts.

---

## 4.4 Shared Module

Contains reusable:
- Models
- API services
- UI utilities

This avoids duplication across features.

---

