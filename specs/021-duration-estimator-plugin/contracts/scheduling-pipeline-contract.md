# Scheduling Pipeline Contract: Duration Estimates

## Consumer

Downstream scheduling tools (Primavera P6, MS Project, or custom scheduling pipeline) consume duration estimate data to build project schedules with logic relationships and critical paths.

## Data Format

The pipeline contract is a structured JSON payload produced by the Duration Estimator and consumed by the scheduling engine (e.g., Phase 21 Relationship Generator).

### Payload Schema

```json
{
  "$schema": "https://som3a.app/schemas/duration-estimate-batch-v1.json",
  "schemaVersion": "1.0.0",
  "generatedAt": "2026-05-28T12:00:00Z",
  "sourcePlugin": "duration-estimator",
  "sourceVersion": "1.0.0",
  "projectId": "proj-001",
  "calendar": {
    "workingDays": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
    "hoursPerDay": 8.0,
    "holidays": ["2026-01-01", "2026-12-25"],
    "startDate": "2026-06-01"
  },
  "activities": [
    {
      "activityId": "ACT-001",
      "activityName": "Formwork Installation - Ground Floor",
      "wbsCode": "1.1.1",
      "quantity": 500.0,
      "unit": "m²",
      "productivityRate": 25.0,
      "crewSize": 4,
      "hoursPerDay": 8.0,
      "durationWorkingDays": 6.25,
      "calendarDurationDays": 9,
      "startDate": "2026-06-01",
      "endDate": "2026-06-11",
      "tradeCategory": "concrete",
      "modifiers": [
        {
          "type": "weather",
          "percentage": 10.0,
          "description": "Summer heat reduction"
        }
      ],
      "variance": {
        "optimistic": 5.0,
        "mostLikely": 6.25,
        "pessimistic": 8.5,
        "standardDeviation": 0.58,
        "confidence95Lower": 5.11,
        "confidence95Upper": 7.39
      },
      "isAnomaly": false,
      "anomalyReason": null
    }
  ],
  "metadata": {
    "totalActivities": 150,
    "totalDurationWorkingDays": 420.5,
    "anomalyCount": 3,
    "aiSuggestionsApplied": 12
  }
}
```

### Contract Rules

1. **activityId** must match the ActivityId from the BOQ Activity Generator (Phase 20)
2. **durationWorkingDays** is the primary scheduling value (calendarDurationDays is informational)
3. **startDate** and **endDate** are calculated using the provided calendar; if no calendar supplied, assumes Monday-Friday working days
4. **variance** is optional (null if 3-point analysis was not performed)
5. **isAnomaly** flags activities requiring manual review before scheduling
6. The **schemaVersion** field allows backward-compatible changes; consumers must reject unknown versions
7. Calendar-aware dates must be consistent: EndDate = StartDate + (workingDays - 1) working days per the calendar rules

## Transport

The pipeline payload is delivered via:
- **In-process**: Direct method call from DurationEstimator to scheduling pipeline when both plugins are loaded
- **File-based**: JSON file at `%Temp%/Som3a/Pipeline/duration-estimates.json` for cross-process or deferred consumption
- **Event**: DurationCalculated event published to EventBus for real-time scheduling updates
