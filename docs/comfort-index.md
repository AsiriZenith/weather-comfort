# Comfort Index Design
## Weather Comfort Analytics Dashboard

---

## 1. Purpose

The **Comfort Index** is a custom numerical score (0–100) designed to represent how comfortable the current weather conditions are for a human, based on commonly understood environmental factors.

This index is:
- Computed **entirely on the backend**
- Derived from real-time weather data
- Designed to be **simple, explainable, and adjustable**

---

## 2. Design Principles

The Comfort Index is based on the following principles:

1. **Human-centric** – prioritizes conditions most people associate with comfort
2. **Explainable** – avoids complex or opaque formulas
3. **Deterministic** – same input always yields the same score
4. **Extensible** – parameters and weights can be adjusted easily

---

## 3. Data Sources

The Comfort Index uses data retrieved from the **OpenWeatherMap Current Weather API**, including:

| Parameter | OpenWeather Field |
|--------|------------------|
| Temperature | `main.temp` |
| Feels Like Temperature | `main.feels_like` |
| Humidity | `main.humidity` |
| Wind Speed | `wind.speed` |
| Cloudiness | `clouds.all` |

All temperature values are converted from **Kelvin to Celsius** before calculation.

---

## 4. Comfort Index Formula

The Comfort Index starts from a **base score of 100** and applies penalties based on deviations from ideal conditions.

### Ideal Conditions (Reference)
- Temperature: **22°C**
- Humidity: **40–60%**
- Wind speed: **≤ 5 m/s**
- Cloudiness: **≤ 30%**

---

### Formula (Conceptual)

```text
Comfort Index =
  100
  - Temperature Penalty
  - Humidity Penalty
  - Wind Penalty
  - Cloudiness Penalty
