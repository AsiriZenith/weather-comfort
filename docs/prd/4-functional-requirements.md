# Functional Requirements

## 4.1 City Data Handling
- The system shall read city codes from a `cities.json` file.
- A minimum of **10 cities** must be processed.
- City codes shall be used to retrieve weather data from OpenWeatherMap.

---

## 4.2 Weather Data Retrieval
- The backend shall call OpenWeatherMap's **current weather API** using city IDs.
- API calls must be executed server-side.
- Temperature values returned in Kelvin must be converted to **Celsius** before processing or display.

---

## 4.3 Comfort Index Calculation
- The backend shall compute a **Comfort Index score between 0 and 100**.
- The Comfort Index must be calculated using **at least three weather parameters**, such as:
  - Temperature
  - Humidity
  - Wind speed
  - Cloudiness
- The Comfort Index logic must reside **only on the backend**.
- The Comfort Index formula and reasoning must be documented in the README.

---

## 4.4 Ranking Logic
- Cities shall be ranked from:
  - **Most Comfortable → Least Comfortable**
- Rankings shall be recalculated whenever fresh weather data is fetched.

---

## 4.5 Caching
- Raw OpenWeatherMap API responses shall be cached for **5 minutes**.
- Processed Comfort Index results may be cached separately.
- A debug endpoint shall expose cache status:
  - `HIT`
  - `MISS`

---

## 4.6 Authentication & Authorization
- Only authenticated users may access the Comfort Index dashboard.
- Authentication shall be implemented using **Auth0**.
- Multi-factor authentication (MFA) via email verification shall be enabled.
- Public signups shall be disabled.
- Only whitelisted users shall be allowed to log in.

---

## 4.7 User Interface
- The UI shall display the following per city:
  - City name
  - Weather description
  - Temperature (°C)
  - Comfort Index score
  - Rank position
- The UI must be responsive and usable on:
  - Desktop
  - Mobile devices

