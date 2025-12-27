# Architecture Overview

The Weather Comfort Analytics Dashboard is a **two-tier web application** consisting of:

- **Frontend**: Angular (client-side SPA)
- **Backend**: ASP.NET Core Web API
- **External Services**:
  - OpenWeatherMap API (weather data)
  - Auth0 (authentication & authorization)

The system is designed using a **pragmatic layered architecture**, intentionally avoiding heavy architectural patterns (e.g., full Clean Architecture) due to the project's limited scope and time constraints.

---

