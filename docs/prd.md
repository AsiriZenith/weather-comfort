# Product Requirements Document (PRD)
## Weather Comfort Analytics Dashboard

---

## 1. Goals & Background

### Goal
To build a **secure, responsive web application** that retrieves live weather data for a predefined list of cities, computes a **custom Comfort Index (0–100)** on the backend, and presents **ranked comfort insights** to authenticated users.

### Background
This project demonstrates:
- External API integration
- Server-side data processing
- Custom metric design
- Caching strategies
- Secure authentication & authorization
- Responsive UI development

The Comfort Index is intentionally **custom-designed** to demonstrate analytical reasoning rather than relying on predefined formulas.

---

## 2. Target Users

### Primary Users
- Internal evaluators / interviewers
- Technical reviewers

### User Characteristics
- Authenticated users only
- Interested in comparative weather comfort insights
- Accessing via desktop or mobile devices

---

## 3. Scope

### In Scope
- Weather data retrieval from OpenWeatherMap
- Backend Comfort Index computation
- City ranking based on comfort
- Secure authentication using Auth0
- Server-side caching
- Responsive UI for desktop and mobile

### Out of Scope
- User-managed city lists
- Historical weather analytics
- Persistent database storage
- Admin or management dashboards

---

## 4. Functional Requirements

### 4.1 City Data Handling
- The system shall read city codes from a `cities.json` file.
- A minimum of **10 cities** must be processed.
- City codes shall be used to retrieve weather data from OpenWeatherMap.

---

### 4.2 Weather Data Retrieval
- The backend shall call OpenWeatherMap’s **current weather API** using city IDs.
- API calls must be executed server-side.
- Temperature values returned in Kelvin must be converted to **Celsius** before processing or display.

---

### 4.3 Comfort Index Calculation
- The backend shall compute a **Comfort Index score between 0 and 100**.
- The Comfort Index must be calculated using **at least three weather parameters**, such as:
  - Temperature
  - Humidity
  - Wind speed
  - Cloudiness
- The Comfort Index logic must reside **only on the backend**.
- The Comfort Index formula and reasoning must be documented in the README.

---

### 4.4 Ranking Logic
- Cities shall be ranked from:
  - **Most Comfortable → Least Comfortable**
- Rankings shall be recalculated whenever fresh weather data is fetched.

---

### 4.5 Caching
- Raw OpenWeatherMap API responses shall be cached for **5 minutes**.
- Processed Comfort Index results may be cached separately.
- A debug endpoint shall expose cache status:
  - `HIT`
  - `MISS`

---

### 4.6 Authentication & Authorization
- Only authenticated users may access the Comfort Index dashboard.
- Authentication shall be implemented using **Auth0**.
- Multi-factor authentication (MFA) via email verification shall be enabled.
- Public signups shall be disabled.
- Only whitelisted users shall be allowed to log in.

---

### 4.7 User Interface
- The UI shall display the following per city:
  - City name
  - Weather description
  - Temperature (°C)
  - Comfort Index score
  - Rank position
- The UI must be responsive and usable on:
  - Desktop
  - Mobile devices

---

## 5. Non-Functional Requirements

### Performance
- Cached responses should be served whenever available.
- Dashboard load times should remain within acceptable limits (<2 seconds typical).

### Security
- All backend endpoints must be protected by JWT validation.
- No API keys or secrets shall be exposed to the frontend.

### Maintainability
- Business logic must be separated from controllers.
- External API integrations must be isolated in dedicated services.

---

## 6. Assumptions & Constraints

### Assumptions
- OpenWeatherMap API availability and reliability
- Small, predefined list of cities
- Stateless backend implementation

### Constraints
- Time-boxed development
- No persistent database storage
- Comfort Index intentionally simplified for clarity

---

## 7. Success Criteria

The solution is considered successful if:
- All required features are implemented
- Comfort Index is computed server-side
- City rankings are accurate
- Caching works as intended
- Auth0 authentication is enforced
- UI is responsive and readable
- Design decisions can be clearly explained

---

## 8. Risks & Mitigations

| Risk | Mitigation |
|----|----|
| API rate limits | Server-side caching |
| Over-engineering | Pragmatic layered architecture |
| Auth complexity | Use official Auth0 SDKs |
| Interview scope changes | Modular, story-driven design |

---

## 9. Future Enhancements (Optional)
- Historical weather trends
- Graphs per city
- Dark mode
- Unit tests for Comfort Index calculation
- Redis-based caching
