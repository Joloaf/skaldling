# Skaldling — MVP

## Overview

Skaldling is a digital adventure generator for families with school-age children. A parent designs a 5-day adventure — a set of real-world tasks they want their child to complete over the school week — and an AI weaves those tasks into a personalized narrative starring the child's customized hero. The parent reads the story to or with the child, marks tasks completed as they happen during the week, and the hero earns achievement points that persist across adventures.

The MVP is the exam scope of a system development course (Systemutvecklare .NET). The product is the vehicle for demonstrating .NET craft; the deliverables that are graded are the code base, an oral presentation, and a written project report.

The working title "Skaldling" comes from the Old Norse *skald* (poet / bard) with the diminutive suffix *-ling*, evoking a young bard-in-training. Renameable until trademark and domain checks are complete.

## Target users

Mainstream Nordic families with children aged approximately 7–9 — the sweet spot where children are transition readers (can follow a story with light parental help), engage with sprite-style character customization, and respond well to gamified routines without finding them babyish.

Two roles operate within the family: the **parent** (designs the adventure, marks tasks as completed, manages point values) and the **child** (customizes the hero, experiences the story, earns achievements). The user stories below reflect this split.

## User stories

**Parent setup**

1. As a parent, I want to design a 5-day adventure with my own choice of tasks per day, so my child has clear, structured goals for the week.
2. As a parent, I want to pick a theme, tone, and moral for the story, so the adventure reinforces values I want to share with my child.
3. As a parent, I want each task to have a difficulty rating (easy / medium / hard), so achievement points scale with the effort required.
4. As a parent, I want to set the final reward (e.g., lördagsgodis, movie night) when I create the adventure, so my child has something tangible to work toward.

**Hero customization and story**

5. As a child, I want to customize my hero's appearance (hair, eyes, face, outfit, accessory), so my hero feels uniquely mine.
6. As a parent, I want the AI to generate a personalized 5-day story starring my child's hero, so every adventure feels fresh and unique to us.
7. As a child, I want my hero to be the protagonist of every adventure, so I feel like the story is genuinely about me.

**Playing the adventure**

8. As a parent, I want to see each day's story and tasks together in one view, so I can read or recap the day's adventure with my child.
9. As a parent, I want to mark tasks as completed and adjust their point values, so my child is rewarded fairly for what they actually did.
10. As a parent, I want the next day's story to unlock once today's tasks are complete, so progress through the adventure has a sense of momentum.
11. As a parent, I want to confirm the finale reward at the end of the adventure, so my child's hero earns their completion badge and we close the week with a celebration.

**Progression across adventures**

12. As a child, I want my hero to keep their achievement points and badges between adventures, so I see proof of my progress over time.

## MVP scope

The MVP delivers the end-to-end experience: a parent customizes a hero with their child, designs a 5-day adventure, the AI generates a personalized story for that hero, the parent plays through the adventure with the child during the week, and the hero earns achievement points that persist.

**In scope**

- Hero customization with sprite-based avatar (hair, eyes, face, outfit, accessory)
- Adventure setup form (theme, tone, moral, 5 days × tasks, per-task difficulty, finale reward)
- AI story generation via Claude Sonnet 4.5
- Day-by-day play UI for parents (story display, task completion, day progression)
- Task completion with adjustable point values
- Persistent hero progression across adventures
- Achievement badges on adventure completion

**Out of scope (future versions)**

- Printable adventure maps (the original product vision; deferred to focus this exam scope on the AI story-generation integration. Returns as the headline feature in the next version.)
- Multi-child households
- Real authentication (OAuth, password recovery)
- Cloud deployment
- Swedish localization (English at MVP; Swedish at a later launch)
- Multi-day story arcs that fork across days
- Equipment and unlock mechanics
- Payment processing and monetization tiers

## Architecture overview

The system is structured around three layers:

- **Frontend (SvelteKit)** — parent-facing app with the hero builder, adventure setup form, and play UI. Serves sprite SVG assets directly from its static folder.
- **Backend (C# .NET Minimal API)** — Vertical Slice Architecture; each feature is a self-contained folder with its endpoint, handler, request/response DTOs, validator, and tests. Endpoints inject handlers directly via DI (no mediator library), prioritizing an explainable dispatch path.
- **Database (PostgreSQL via EF Core)** — relational with JSONB columns where flexibility outweighs strict referential integrity (notably the avatar composition).

The architectural centerpiece is an `IStoryGenerator` abstraction with two production implementations:

- `LlmStoryGenerator` — calls Claude Sonnet 4.5 via the Anthropic .NET SDK with structured JSON output and a retry-with-validate loop
- `FixtureStoryGenerator` — returns hand-authored adventures; used in tests and as a demo-day fallback if the API is unreachable

Swapping between them is a DI registration change with no impact on consuming code.

Hero customization is built from a sprite library: SVG files in the frontend's static folder, with metadata (including an LLM-facing description per sprite) in a `Sprite` table in Postgres. When the AI generates a story, the backend joins the hero's selected sprite IDs to the sprite descriptions and assembles a hero block that the system prompt weaves into the narrative.

The data model accommodates fork-and-converge story structure (each day can have multiple branches that rejoin), even though the MVP UI renders only the linear case. This is a deliberate architectural choice to enable the fork UI as a stretch feature without a schema migration.

## Tech stack

- Frontend: **SvelteKit**
- Backend: **C# .NET (Minimal API)**
- Database: **PostgreSQL** with **EF Core**
- LLM: **Claude Sonnet 4.5** via the **Anthropic .NET SDK** (or Microsoft.Extensions.AI abstracting Anthropic)
- Testing: **xUnit**, with **WebApplicationFactory** for integration tests
- Local development: **Docker Compose** for Postgres
- Hosting (MVP): localhost

## Timeline

The work is sized for a 2-week core build with a third week reserved for the project report, the oral presentation, and any stretch features (in that order of priority).

**Week 1 — Foundations, sprite library, adventure setup**

- Days 1–2: Solution scaffold and one feature slice end-to-end
- Days 3–4: Sprite asset library, Sprite table, asset serving
- Day 5: Avatar builder UI
- Days 6–7: Adventure entity model and setup form

**Week 2 — LLM integration, play loop, tests**

- Days 8–9: `IStoryGenerator` interface, `LlmStoryGenerator` against Claude, system prompt as a versioned artifact, structured output with retry-on-invalid
- Day 10: Hero context serialization, first end-to-end story generation
- Days 11–12: Play loop UI (parent view)
- Days 13–14: Tests, polish, begin project report writing

**Week 3 — Report, presentation, stretch**

- Project report writing (priority)
- Oral presentation preparation (12-minute presentation + 8-minute Q&A)
- Stretch features only if the graded artifacts are locked: fork-converge in the digital play UI, additional themes, more sprite variants

## Deliverables

- A working code base demonstrating the MVP scope above
- An oral presentation walking through the project
- A written project report in the course-provided Word template, in Swedish
