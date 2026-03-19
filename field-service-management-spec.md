# FieldOps — Field Service Management System

## Overview

A production-grade field service management platform that handles job dispatching,
technician scheduling, real-time status tracking, and SLA enforcement for organisations
that deploy workers to physical locations. Built to reflect the operational reality of
SA companies in telecoms, facilities management, utilities, and infrastructure
maintenance — where jobs must be completed within contractual time windows, technicians
operate in low-connectivity environments, and SLA breaches carry financial penalties.

---

## Problem Statement

Companies that dispatch field technicians — whether fixing fibre lines, servicing HVAC
systems, or maintaining ATMs — share a common set of operational problems:

A job comes in. Someone needs to decide which technician to assign based on location,
skill set, and current workload. The technician needs to know where to go and what to
do. The back office needs to know if the job is on track. If the job is taking too long,
a manager needs to know before the SLA is breached, not after. When a technician is in
a dead zone, their status updates need to queue and sync when connectivity returns.

Most companies manage this with spreadsheets, WhatsApp groups, and phone calls. The
ones that don't are running systems like this.

---

## Goals

- Manage the full job lifecycle from creation to closure
- Assign jobs to technicians based on skill, location, and availability
- Track SLAs in real time and escalate proactively before breach
- Handle technician status updates with offline tolerance
- Expose a clean API for integration with client systems
- Support multiple client organisations (multi-tenant)
- Be fully observable with metrics, alerts, and audit trails

---

## Non-Goals

- Route optimisation (turn-by-turn navigation) is out of scope
- Native mobile app is out of scope (API-first; a mobile team consumes the API)
- Billing and invoicing are out of scope
- IoT sensor integration is out of scope

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Real-time | SignalR (job status updates to dashboards) |
| Messaging | Azure Service Bus |
| Background Jobs | Hangfire |
| Cache | Redis (technician availability, SLA timers) |
| Auth | Azure AD / JWT Bearer |
| Observability | OpenTelemetry + Prometheus + Serilog |
| Testing | xUnit + Testcontainers |
| Containerisation | Docker + Docker Compose |

---

## Domain Model

### Client (Tenant)

The organisation that contracts field service work — a telecoms company, a bank
(ATM maintenance), a facilities manager.

```
Client
├── Id (GUID)
├── Name (string)
├── ContractReference (string)
├── DefaultSlaConfig (JSON) — fallback SLA if not set per job type
├── Status (enum: Active | Suspended | Terminated)
├── CreatedAt (DateTimeOffset)
└── UpdatedAt (DateTimeOffset)
```

### SlaConfig

Defines the time windows within which jobs of a given type must be completed.
Stored per client, per job type. This is the contractual backbone of the system.

```
SlaConfig
├── Id (GUID)
├── ClientId (GUID, FK)
├── JobType (string) — e.g. "FibreRepair", "AtmMaintenance", "HvacService"
├── Priority (enum: Critical | High | Medium | Low)
├── ResponseTimeMinutes (int) — time from job creation to technician on-site
├── ResolutionTimeMinutes (int) — time from job creation to job closed
├── EscalationThresholdPercent (int) — escalate when X% of resolution time consumed
├── PenaltyPerBreachZAR (decimal?) — financial penalty per breach if applicable
└── IsActive (bool)
```

### Region

Geographic area used for technician assignment scoping.

```
Region
├── Id (GUID)
├── ClientId (GUID, FK)
├── Name (string) — e.g. "Johannesburg North", "Cape Town CBD"
├── Boundaries (JSON) — GeoJSON polygon (for future geo-query support)
└── IsActive (bool)
```

### Technician

A field worker who executes jobs.

```
Technician
├── Id (GUID)
├── ClientId (GUID, FK)
├── EmployeeNumber (string)
├── FirstName (string)
├── LastName (string)
├── Email (string)
├── Phone (string)
├── RegionId (GUID, FK) — home region
├── Skills (JSON array) — e.g. ["FibreRepair", "NetworkConfig", "HvacLevel2"]
├── Status (enum: Offline | Available | EnRoute | OnSite | OnBreak)
├── LastKnownLatitude (decimal?)
├── LastKnownLongitude (decimal?)
├── LastLocationUpdatedAt (DateTimeOffset?)
├── CurrentJobId (GUID?) — FK to active Job
├── IsActive (bool)
├── CreatedAt (DateTimeOffset)
└── UpdatedAt (DateTimeOffset)
```

### Job

The central entity. Represents a unit of field work that must be completed.

```
Job
├── Id (GUID)
├── ClientId (GUID, FK)
├── Reference (string) — human-readable, e.g. "JOB-2025-004821"
├── JobType (string)
├── Priority (enum: Critical | High | Medium | Low)
├── Title (string)
├── Description (string)
├── SiteAddress (Address value object)
├── SiteLatitude (decimal?)
├── SiteLongitude (decimal?)
├── ContactName (string)
├── ContactPhone (string)
├── Status (enum — see Job Lifecycle)
├── AssignedTechnicianId (GUID?)
├── SlaConfigId (GUID, FK)
├── SlaResponseDeadline (DateTimeOffset) — computed on creation
├── SlaResolutionDeadline (DateTimeOffset) — computed on creation
├── SlaResponseMet (bool?)
├── SlaResolutionMet (bool?)
├── EscalationSentAt (DateTimeOffset?)
├── RequiredSkills (JSON array)
├── Notes (string?)
├── Metadata (JSON)
├── CreatedAt (DateTimeOffset)
├── UpdatedAt (DateTimeOffset)
├── AcknowledgedAt (DateTimeOffset?)
├── EnRouteAt (DateTimeOffset?)
├── OnSiteAt (DateTimeOffset?)
└── ClosedAt (DateTimeOffset?)
```

### JobNote

Technicians and dispatchers can add notes throughout job execution.

```
JobNote
├── Id (GUID)
├── JobId (GUID, FK)
├── ClientId (GUID, FK)
├── AuthorId (string)
├── AuthorType (enum: Dispatcher | Technician | System)
├── Content (string)
├── Attachments (JSON array) — Azure Blob references
├── IsOfflineSynced (bool) — true if submitted while technician was offline
└── CreatedAt (DateTimeOffset)
```

### JobStatusEvent

Immutable log of every status transition a job goes through. Never updated or deleted.

```
JobStatusEvent
├── Id (GUID)
├── JobId (GUID, FK)
├── ClientId (GUID, FK)
├── PreviousStatus (enum?)
├── NewStatus (enum)
├── TriggeredBy (string) — technician ID, dispatcher ID, or "system"
├── TriggerSource (enum: Api | OfflineSync | SystemJob)
├── OccurredAt (DateTimeOffset) — WHEN the transition happened (business time)
├── ReceivedAt (DateTimeOffset) — when the system processed it
├── Latitude (decimal?) — technician location at time of update
├── Longitude (decimal?)
├── Notes (string?)
└── CorrelationId (GUID)
```

### SlaBreachRecord

Created whenever a job misses its response or resolution deadline.

```
SlaBreachRecord
├── Id (GUID)
├── JobId (GUID, FK)
├── ClientId (GUID, FK)
├── BreachType (enum: ResponseTime | ResolutionTime)
├── Deadline (DateTimeOffset)
├── ActualTime (DateTimeOffset?)
├── OverdueMinutes (int)
├── TechnicianId (GUID?)
├── PenaltyAmountZAR (decimal?)
├── AcknowledgedBy (string?)
└── CreatedAt (DateTimeOffset)
```

### OfflineSyncBatch

When a technician's device reconnects after operating offline, their queued updates
are submitted as a batch.

```
OfflineSyncBatch
├── Id (GUID)
├── TechnicianId (GUID, FK)
├── ClientId (GUID, FK)
├── DeviceId (string)
├── SubmittedAt (DateTimeOffset) — when batch arrived at server
├── EventCount (int)
├── Status (enum: Pending | Processing | Completed | PartiallyFailed)
├── FailedEventCount (int)
└── ProcessedAt (DateTimeOffset?)
```

---

## Job Lifecycle (State Machine)

Every job moves through an explicit state machine. Invalid transitions are rejected
at the domain level.

```
                    ┌─────────────┐
                    │   Created   │ ◄── Job logged, SLA clock starts
                    └──────┬──────┘
                           │  Technician assigned
                           ▼
                    ┌─────────────┐
                    │  Assigned   │
                    └──────┬──────┘
                           │  Technician acknowledges job
                           ▼
                    ┌──────────────┐
                    │ Acknowledged │ ◄── Response SLA check point
                    └──────┬───────┘
                           │  Technician departs for site
                           ▼
                    ┌─────────────┐
                    │  En Route   │
                    └──────┬──────┘
                           │  Technician arrives on site
                           ▼
                    ┌─────────────┐
                    │   On Site   │
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
              ▼            ▼            ▼
       ┌──────────┐  ┌──────────┐  ┌──────────────┐
       │  Closed  │  │  Paused  │  │  Escalated   │
       │(Resolved)│  │(Waiting  │  │(Cannot       │
       └──────────┘  │for parts)│  │ resolve)     │
                     └────┬─────┘  └──────┬───────┘
                          │               │
                    Parts arrive    Reassigned /
                          │         Senior tech
                          ▼         dispatched
                     ┌─────────┐         │
                     │ On Site │◄────────┘
                     └─────────┘

              ┌──────────────────────────────┐
              │ From any non-terminal state: │
              └──────────────────────────────┘
                    Cancelled → terminal
```

### Valid Transitions

| From | To | Trigger |
|---|---|---|
| Created | Assigned | Dispatcher assigns technician |
| Created | Cancelled | Dispatcher cancels |
| Assigned | Acknowledged | Technician acknowledges |
| Assigned | Cancelled | Dispatcher cancels before acknowledgement |
| Acknowledged | EnRoute | Technician marks as en route |
| EnRoute | OnSite | Technician marks arrival |
| OnSite | Closed | Technician closes with resolution notes |
| OnSite | Paused | Technician pauses (awaiting parts, access) |
| OnSite | Escalated | Technician cannot resolve, escalates |
| Paused | OnSite | Technician resumes |
| Escalated | Assigned | Dispatcher reassigns to senior technician |
| Any non-terminal | Cancelled | Dispatcher with reason |

---

## SLA Engine

This is the operational core of the system. SLA management is what separates a
professional field service platform from a basic job tracker.

### SLA Deadline Calculation

On job creation, two deadlines are computed immediately and stored on the job:

```csharp
SlaResponseDeadline = job.CreatedAt + slaConfig.ResponseTimeMinutes
SlaResolutionDeadline = job.CreatedAt + slaConfig.ResolutionTimeMinutes
```

Deadlines are immutable once set. If an SLA config changes, existing jobs are
not affected — they run on the config that was active when they were created.
This mirrors how contractual SLAs actually work.

### Escalation — Proactive, Not Reactive

The system escalates **before** breach, not after. When a job reaches
`EscalationThresholdPercent` of its resolution time without being closed:

```
EscalationTriggerTime = CreatedAt + (ResolutionTimeMinutes × EscalationThresholdPercent / 100)
```

Example: a job with a 4-hour resolution SLA and 75% escalation threshold triggers
an escalation alert at the 3-hour mark — giving the dispatcher a 1-hour window to
intervene before the breach.

Escalation:
1. Sets `Job.EscalationSentAt`
2. Publishes an `EscalationTriggered` event to Azure Service Bus
3. Notifies assigned dispatcher via SignalR (real-time dashboard alert)
4. Logs a `JobStatusEvent` with `TriggeredBy = "system"`

### SLA Breach Recording

At each deadline, `SlaMonitorJob` checks all open jobs:

- If `SlaResponseDeadline` has passed and job is still `Assigned` or `Created` →
  `SlaResponseMet = false`, `SlaBreachRecord` created
- If `SlaResolutionDeadline` has passed and job is not `Closed` or `Cancelled` →
  `SlaResolutionMet = false`, `SlaBreachRecord` created with `OverdueMinutes`
  calculated and penalty amount applied if configured

### SLA Compliance Reporting

```
GET /api/v1/sla/compliance
  Query: clientId, from, to, jobType, priority
  Returns:
    {
      totalJobs,
      responseSlaMet: { count, percent },
      resolutionSlaMet: { count, percent },
      breaches: { count, totalPenaltyZAR },
      byPriority: { Critical: {...}, High: {...}, ... },
      byJobType: { FibreRepair: {...}, ... }
    }
```

This report is what a client's operations manager looks at every Monday morning.
Building it into the spec shows you understand who actually uses the system.

---

## Technician Assignment

### Manual Assignment

Dispatcher selects a technician from the available pool. The API provides a ranked
candidate list to assist the decision:

```
GET /api/v1/jobs/{id}/assignment-candidates
  Returns: ranked list of available technicians with:
    - skillMatch: bool (has all required skills)
    - currentJobCount: int (active jobs assigned)
    - distanceKm: decimal? (if location available)
    - estimatedAvailableAt: DateTimeOffset? (based on current job ETAs)
```

Ranking algorithm (in order of priority):
1. Must have all `RequiredSkills`
2. Must be `Available` or `OnSite` with finishing soon
3. Sorted by: current job count ASC, then distance ASC if location available

### Auto-Assignment

For `Critical` priority jobs, the system can auto-assign the best candidate
without dispatcher intervention:

```
POST /api/v1/jobs/{id}/auto-assign
  Runs ranking algorithm, assigns top candidate, notifies via SignalR
  Returns: { assignedTechnicianId, reason }
```

Auto-assignment is configurable per client (opt-in, not default).

---

## Offline Tolerance

Technicians operate in areas with intermittent connectivity — basements, remote
sites, areas with poor signal. The system is designed so offline periods don't
corrupt job state.

### How It Works

The mobile client (out of scope, but the API is designed for it) queues status
updates locally with `OccurredAt` timestamps when offline. On reconnect, it
submits the queue as an `OfflineSyncBatch`.

### Batch Processing

```
POST /api/v1/sync/batch
  Body:
    {
      deviceId,
      technicianId,
      events: [
        {
          jobId,
          newStatus,
          occurredAt,    ← when it happened offline
          latitude?,
          longitude?,
          notes?
        }
      ]
    }
```

### Conflict Resolution

Each event in the batch is processed in `OccurredAt` order, not submission order.
For each event:

1. Is the transition valid from the job's current state? → Apply it
2. Is the transition already recorded (duplicate)? → Skip, log
3. Is the transition invalid (job was cancelled while offline)? →
   Reject, add to `FailedEventCount`, return conflict detail to client

The `JobStatusEvent` records both `OccurredAt` (when it happened) and `ReceivedAt`
(when it was processed), so the full timeline is always reconstructable even for
offline-synced events.

### Why This Matters

A naive system that only accepts real-time updates would show a technician as
stuck in `Acknowledged` for 3 hours because they were in a basement. When they
resurface, their batch of updates is processed in order — `EnRoute`, `OnSite`,
`Closed` — and the job history accurately reflects what actually happened.

SLA breach evaluation re-runs after batch processing using `OccurredAt` timestamps,
not `ReceivedAt`. A job closed at 14:43 (OccurredAt) but synced at 16:30 is
evaluated against SLA using 14:43 — the actual closure time.

---

## Real-Time Updates (SignalR)

Dispatchers monitoring a live dashboard receive real-time pushes:

| Event | Trigger |
|---|---|
| `job.status_changed` | Any job status transition |
| `job.escalation_triggered` | SLA escalation threshold reached |
| `job.sla_breached` | SLA deadline missed |
| `technician.status_changed` | Technician availability changes |
| `technician.location_updated` | Location ping received |

Clients subscribe to a tenant-scoped SignalR group. Technicians only receive
updates for their own jobs. Dispatchers receive all updates for their region.

---

## API Endpoints

### Jobs

```
POST   /api/v1/jobs
GET    /api/v1/jobs
  Query: status, technicianId, jobType, priority, regionId, dateFrom, dateTo,
         slaBreach (bool), cursor, limit
GET    /api/v1/jobs/{id}
PUT    /api/v1/jobs/{id}
DELETE /api/v1/jobs/{id}  — soft cancel with reason

GET    /api/v1/jobs/{id}/events          — full status history
GET    /api/v1/jobs/{id}/notes
POST   /api/v1/jobs/{id}/notes

GET    /api/v1/jobs/{id}/assignment-candidates
POST   /api/v1/jobs/{id}/assign          — manual assign
POST   /api/v1/jobs/{id}/auto-assign

POST   /api/v1/jobs/{id}/acknowledge
POST   /api/v1/jobs/{id}/en-route
POST   /api/v1/jobs/{id}/on-site
POST   /api/v1/jobs/{id}/pause           — body: { reason }
POST   /api/v1/jobs/{id}/resume
POST   /api/v1/jobs/{id}/close           — body: { resolutionNotes, workPerformed }
POST   /api/v1/jobs/{id}/escalate        — body: { reason }
POST   /api/v1/jobs/{id}/cancel          — body: { reason }
```

### Technicians

```
POST   /api/v1/technicians
GET    /api/v1/technicians
  Query: status, regionId, skill, available (bool)
GET    /api/v1/technicians/{id}
PUT    /api/v1/technicians/{id}
DELETE /api/v1/technicians/{id}  — deactivate

GET    /api/v1/technicians/{id}/jobs     — active and historical jobs
POST   /api/v1/technicians/{id}/location — location ping update
```

### SLA

```
GET    /api/v1/sla/compliance
GET    /api/v1/sla/breaches
  Query: clientId, from, to, breachType, acknowledged
PUT    /api/v1/sla/breaches/{id}/acknowledge

GET    /api/v1/sla/config
POST   /api/v1/sla/config
PUT    /api/v1/sla/config/{id}
```

### Sync

```
POST   /api/v1/sync/batch
GET    /api/v1/sync/batches/{id}         — batch processing status and failures
```

### Reporting

```
GET    /api/v1/reports/technician-performance
  Returns: jobs completed, avg resolution time, SLA compliance % per technician

GET    /api/v1/reports/region-summary
  Returns: open jobs, breach count, avg response time per region

GET    /api/v1/reports/job-volume
  Query: granularity (day | week | month), from, to
  Returns: job count time series by status
```

---

## Background Jobs (Hangfire)

| Job | Schedule | Description |
|---|---|---|
| `SlaMonitorJob` | Every 5 minutes | Checks all open jobs against SLA deadlines, creates breach records |
| `EscalationTriggerJob` | Every 5 minutes | Checks escalation thresholds, fires alerts |
| `AutoVoidStaleJobsJob` | Daily at 02:00 | Cancels jobs in Created state for more than 72h with no assignment |
| `TechnicianAvailabilityResetJob` | Daily at 06:00 | Resets overnight Offline technicians to Available for shift start |
| `SlaReportJob` | Weekly Monday 07:00 | Generates weekly SLA compliance report per client, stores to Blob |
| `StaleOfflineSyncAlertJob` | Every 30 minutes | Alerts if an OfflineSyncBatch has been Pending for more than 15 minutes |

---

## Observability

### Metrics

- `fieldops_jobs_created_total` — counter, labelled by jobType, priority
- `fieldops_jobs_closed_total` — counter, labelled by jobType
- `fieldops_sla_breaches_total` — counter, labelled by breachType, priority
- `fieldops_sla_compliance_rate` — gauge per client (0.0–1.0)
- `fieldops_escalations_total` — counter
- `fieldops_job_resolution_minutes` — histogram of actual resolution times
- `fieldops_offline_sync_events_total` — counter, labelled by outcome
- `fieldops_offline_sync_batch_size` — histogram
- `fieldops_technician_utilisation` — gauge (active jobs / total technicians)

### The Metric That Matters Most

`fieldops_job_resolution_minutes` as a histogram gives you p50, p95, p99 resolution
times. If p95 is 210 minutes against a 240-minute SLA, you're running close to the
wire and the ops team needs to know. This is the operational heartbeat of a field
service business.

---

## Testing Strategy

| Type | Coverage |
|---|---|
| Unit | SLA deadline calculation, escalation threshold logic, assignment ranking, state machine transitions |
| Integration | Full API via Testcontainers (SQL Server + Redis) |
| Scenario | End-to-end job lifecycle scenarios (see below) |

### Key Scenarios

**Happy path:**
Create job → assign technician → acknowledge → en route → on site → close →
SLA met flags set correctly, resolution time recorded.

**SLA breach — response time:**
Create Critical job → assign technician → no acknowledgement → response deadline
passes → SlaBreachRecord created with BreachType=ResponseTime.

**Escalation before breach:**
Create job with 4h SLA, 75% threshold → advance clock to 3h mark →
EscalationTriggerJob fires → EscalationSentAt set → SignalR event emitted.

**Offline sync — happy path:**
Technician goes offline at OnSite → closes job offline (OccurredAt = 14:43) →
reconnects at 16:30 → batch submitted → job closed → SLA evaluated using 14:43.

**Offline sync — conflict:**
Job cancelled by dispatcher while technician offline → technician syncs closure
event → conflict detected → closure rejected → FailedEventCount = 1 → conflict
detail returned to client.

**Assignment ranking:**
3 technicians available: A has wrong skills, B has right skills and 2 active jobs,
C has right skills and 0 active jobs → C ranked first.

**State machine invalid transition:**
Attempt to close a job in Created state (not yet on site) → domain error returned,
job state unchanged, attempt logged.

**Duplicate sync event:**
Same status event submitted twice in offline batch → second ignored, not applied
twice, job state correct.

---

## Project Structure

```
FieldOps/
├── src/
│   ├── FieldOps.Api/                    # ASP.NET Core API, SignalR hubs
│   │   ├── Endpoints/
│   │   │   ├── Jobs/
│   │   │   ├── Technicians/
│   │   │   ├── Sla/
│   │   │   ├── Sync/
│   │   │   └── Reports/
│   │   ├── Hubs/
│   │   │   └── DispatchHub.cs           # SignalR hub
│   │   ├── Middleware/
│   │   └── Program.cs
│   ├── FieldOps.Application/            # MediatR handlers
│   │   ├── Jobs/
│   │   ├── Assignment/
│   │   ├── Sla/
│   │   ├── Sync/
│   │   └── Reports/
│   ├── FieldOps.Domain/                 # Entities, state machine, value objects
│   │   ├── Entities/
│   │   ├── StateMachines/
│   │   │   └── JobStateMachine.cs
│   │   └── ValueObjects/
│   │       └── SlaDeadlines.cs
│   ├── FieldOps.Infrastructure/         # EF Core, Redis, Service Bus, Hangfire
│   │   ├── Persistence/
│   │   ├── Realtime/
│   │   │   └── SignalRNotifier.cs
│   │   └── Sla/
│   │       └── SlaCalculator.cs
│   └── FieldOps.Jobs/                   # Hangfire job definitions
├── tests/
│   ├── FieldOps.UnitTests/
│   ├── FieldOps.IntegrationTests/
│   └── FieldOps.ScenarioTests/
├── docker-compose.yml
├── prometheus.yml
└── README.md
```

---

## Architecture Decisions

### Why are SLA deadlines stored on the job, not computed on query?
Computing deadlines dynamically on every query requires joining to SlaConfig on
every read — and if the SLA config ever changes, historical queries become wrong.
Storing `SlaResponseDeadline` and `SlaResolutionDeadline` as immutable columns on
the job means every job carries its own contractual obligations. It also makes
`SlaMonitorJob` a simple date comparison query with no joins.

### Why use OccurredAt for SLA evaluation in offline sync?
Because SLA measures real-world time, not server processing time. A technician who
closed a job at 14:43 but synced at 16:30 resolved the issue at 14:43. Penalising
them for the sync delay would make SLA metrics meaningless in any environment with
intermittent connectivity — which is most field environments in SA.

### Why SignalR for real-time updates instead of polling?
A dispatcher dashboard refreshing every 5 seconds is 720 HTTP requests per hour per
user. SignalR maintains a persistent connection and pushes only when something changes.
For a dispatcher watching 30–50 active jobs, this is the difference between a usable
interface and one that feels perpetually stale.

### Why Hangfire for SlaMonitorJob instead of a timer in-process?
In-process timers die when the application restarts. During a deployment, SLA
monitoring would silently stop for the duration of the restart. Hangfire persists
job schedules to SQL Server — a restart doesn't lose the next scheduled run.
For SLA monitoring, missing a single run could mean a breach goes undetected.

### Why is auto-assignment opt-in?
Different clients have different operational preferences. Some want full dispatcher
control. Others want automated assignment for critical jobs to reduce response time.
Making it opt-in per client respects that without requiring separate deployments.

---

## What This Project Demonstrates to a Hiring Manager

- Domain adaptability — same engineering rigour as the fintech projects applied
  to a completely different industry
- Real-world constraint thinking — offline tolerance, SLA enforcement, escalation
  are operational problems, not academic ones
- System design for human workflows — the assignment ranking, the dispatcher
  dashboard, the weekly SLA report exist because real people use this system
- SignalR — a technology stack most juniors haven't touched, immediately useful
  in enterprise client work
- State machine applied outside fintech — proving it's a tool, not a pattern copy
- The kind of system a Synthesis or Codehesion client would actually commission
