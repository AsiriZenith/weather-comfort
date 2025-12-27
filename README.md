# Weather Comfort Analytics Dashboard

## Overview

A secure, responsive web application that retrieves live weather data for a predefined list of cities, computes a custom Comfort Index (0–100) on the backend, and presents ranked comfort insights to authenticated users.

## Comfort Index Formula

The Comfort Index is a custom numerical score (0–100) designed to represent how comfortable the current weather conditions are for a human, based on commonly understood environmental factors.

### Design Principles

1. **Human-centric** – prioritizes conditions most people associate with comfort
2. **Explainable** – avoids complex or opaque formulas
3. **Deterministic** – same input always yields the same score
4. **Extensible** – parameters and weights can be adjusted easily

### Ideal Conditions

The Comfort Index starts from a base score of **100** and applies penalties based on deviations from ideal conditions:

- **Temperature**: 22°C (optimal room temperature)
- **Humidity**: 40–60% (comfortable range)
- **Wind Speed**: ≤ 5 m/s (gentle breeze)
- **Cloudiness**: ≤ 30% (mostly clear skies)

### Formula Structure

```
Comfort Index = 100 - Temperature Penalty - Humidity Penalty - Wind Penalty - Cloudiness Penalty
```

The final score is clamped to ensure it always remains between 0 and 100.

### Penalty Calculations

#### Temperature Penalty
- **Weight**: 1.5 points per °C deviation from 22°C
- **Calculation**: `|Temperature - 22°C| × 1.5`
- **Rationale**: Temperature has a significant impact on comfort. Both very hot and very cold conditions are uncomfortable, so we use absolute deviation.

**Examples:**
- 22°C → 0 points penalty (ideal)
- 25°C → (25 - 22) × 1.5 = 4.5 points penalty
- 30°C → (30 - 22) × 1.5 = 12 points penalty
- 15°C → |15 - 22| × 1.5 = 10.5 points penalty
- 0°C → |0 - 22| × 1.5 = 33 points penalty

#### Humidity Penalty
- **Weight**: 0.5 points per % deviation outside the ideal range (40–60%)
- **Calculation**: 
  - If humidity < 40%: `(40 - Humidity) × 0.5`
  - If humidity > 60%: `(Humidity - 60) × 0.5`
  - If 40% ≤ humidity ≤ 60%: 0 points (within ideal range)
- **Rationale**: Moderate humidity is comfortable. Very dry or very humid air can be uncomfortable, but the impact is less severe than temperature.

**Examples:**
- 50% → 0 points penalty (within ideal range)
- 30% → (40 - 30) × 0.5 = 5 points penalty
- 80% → (80 - 60) × 0.5 = 10 points penalty
- 100% → (100 - 60) × 0.5 = 20 points penalty

#### Wind Speed Penalty
- **Weight**: 2.0 points per m/s above 5 m/s
- **Calculation**: `max(0, WindSpeed - 5) × 2.0`
- **Rationale**: Gentle breeze (≤5 m/s) is pleasant, but strong winds can be uncomfortable and even dangerous. Wind speed above the ideal threshold has a significant impact.

**Examples:**
- 3 m/s → 0 points penalty (within ideal range)
- 5 m/s → 0 points penalty (at ideal threshold)
- 10 m/s → (10 - 5) × 2.0 = 10 points penalty
- 20 m/s → (20 - 5) × 2.0 = 30 points penalty

#### Cloudiness Penalty
- **Weight**: 0.3 points per % above 30%
- **Calculation**: `max(0, Cloudiness - 30) × 0.3`
- **Rationale**: Clear or mostly clear skies are generally preferred. Cloudiness has a moderate impact on comfort, affecting mood and perceived temperature.

**Examples:**
- 20% → 0 points penalty (within ideal range)
- 30% → 0 points penalty (at ideal threshold)
- 50% → (50 - 30) × 0.3 = 6 points penalty
- 100% → (100 - 30) × 0.3 = 21 points penalty

### Example Calculations

#### Example 1: Ideal Conditions
- Temperature: 22°C
- Humidity: 50%
- Wind Speed: 5 m/s
- Cloudiness: 30%

**Calculation:**
- Temperature Penalty: 0
- Humidity Penalty: 0
- Wind Penalty: 0
- Cloudiness Penalty: 0
- **Comfort Index: 100 - 0 - 0 - 0 - 0 = 100**

#### Example 2: Pleasant Day
- Temperature: 24°C
- Humidity: 55%
- Wind Speed: 4 m/s
- Cloudiness: 25%

**Calculation:**
- Temperature Penalty: |24 - 22| × 1.5 = 3.0
- Humidity Penalty: 0 (within 40-60%)
- Wind Penalty: 0 (≤ 5 m/s)
- Cloudiness Penalty: 0 (≤ 30%)
- **Comfort Index: 100 - 3.0 - 0 - 0 - 0 = 97.0**

#### Example 3: Hot and Humid Day
- Temperature: 35°C
- Humidity: 85%
- Wind Speed: 2 m/s
- Cloudiness: 40%

**Calculation:**
- Temperature Penalty: |35 - 22| × 1.5 = 19.5
- Humidity Penalty: (85 - 60) × 0.5 = 12.5
- Wind Penalty: 0 (≤ 5 m/s)
- Cloudiness Penalty: (40 - 30) × 0.3 = 3.0
- **Comfort Index: 100 - 19.5 - 12.5 - 0 - 3.0 = 65.0**

#### Example 4: Cold and Windy Day
- Temperature: 5°C
- Humidity: 45%
- Wind Speed: 15 m/s
- Cloudiness: 80%

**Calculation:**
- Temperature Penalty: |5 - 22| × 1.5 = 25.5
- Humidity Penalty: 0 (within 40-60%)
- Wind Penalty: (15 - 5) × 2.0 = 20.0
- Cloudiness Penalty: (80 - 30) × 0.3 = 15.0
- **Comfort Index: 100 - 25.5 - 0 - 20.0 - 15.0 = 39.5**

### Reasoning Behind the Formula

1. **Base Score of 100**: Starting from a perfect score makes it intuitive - higher is better, and we subtract penalties for deviations.

2. **Temperature as Primary Factor**: Temperature has the highest weight (1.5) because it's the most immediately noticeable factor affecting comfort. Both extreme heat and cold significantly impact comfort.

3. **Humidity Range**: Rather than a single ideal value, humidity has an ideal range (40-60%) because people can be comfortable across this range. Only deviations outside this range incur penalties.

4. **Wind Speed Threshold**: Wind speed only penalizes above 5 m/s because gentle breezes are pleasant. Strong winds become uncomfortable quickly, hence the higher weight (2.0).

5. **Cloudiness as Secondary Factor**: Cloudiness has the lowest weight (0.3) because while it affects mood and perceived temperature, it's less directly impactful than temperature, humidity, or wind.

6. **Score Clamping**: The final score is always clamped between 0 and 100 to ensure consistent interpretation regardless of extreme conditions.

### Edge Case Handling

The formula handles edge cases gracefully:

- **Invalid or missing data**: Uses default values (ideal temperature for missing temp, clamps invalid humidity/cloudiness to 0-100%, uses 0 for invalid wind speed)
- **Extreme values**: Score is clamped to 0-100 range, ensuring even the worst conditions don't produce negative scores
- **NaN/Infinity values**: Invalid numeric values are detected and replaced with safe defaults

### Implementation Notes

- The Comfort Index is calculated **entirely on the backend** to ensure consistency and security
- The formula is deterministic - same inputs always produce the same output
- All calculations use double precision for accuracy
- Final scores are rounded to 2 decimal places for display
- Penalty breakdowns are optionally included in the response for transparency and debugging
