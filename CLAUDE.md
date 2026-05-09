# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Ordis** is an ASP.NET Core 9 Blazor Server app for managing tabletop RPG character sheets. Players log in via Discord OAuth2, manage characters with stats/gauges/inventory/spells, roll dice, and participate in campaigns.

## Commands

```bash
# Build
dotnet build

# Run (development)
dotnet run

# Publish (release)
dotnet publish --configuration Release -o out

# EF Core migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

The build automatically runs `npm install` then compiles `sass/app.scss` → `wwwroot/app.css` via the `sass` CLI before building.

## Configuration & Secrets

Real credentials come from the `.env` file (loaded via DotNetEnv at startup) or environment variables. `appsettings.json` contains only placeholder values. Required env vars:

- `connectionstrings__CharacterDb` — PostgreSQL connection string
- `Discord__ClientId` / `Discord__ClientSecret` — Discord OAuth2 app credentials
- `DiscordWebhookUrl` — Discord webhook for sending messages/embeds

The `.envrc` file loads `.env` automatically via direnv.

## Architecture

### Request Flow

```
Browser → Blazor Server (SignalR) → Razor Components → Services → OrdisContext (EF Core) → PostgreSQL
```

Auth is Discord OAuth2 (cookie-based). The Discord user ID is stored as the `discord_id` claim and used everywhere to identify users.

### Services (`Services/`)

- **`PlayerCharacterService`** — CRUD for `PlayerCharacter`. Dice rolling is delegated to an external service at `http://localhost:3000/roll/{characterId}/{formula}` (a separate roll service, not in this repo).
- **`CampaignService`** — CRUD for `Campaign`, including player management.
- **`DiscordService`** — Sends plain messages and rich embeds to a Discord webhook.
- **`UserState`** — Scoped per-circuit; holds the currently selected `PlayerCharacter`.

### Data Model

- **`PlayerCharacter`** — Core entity. Has `Gauges` (resource bars), `Rolls` (roll history), `Campaign` (optional), `Inventory`, `Spells`. `StatBlock` is stored as a JSON string in the `stat_block` column and deserialized on access.
- **`StatBlock`** — POCO serialized as JSON in `PlayerCharacter.StatBlockJson`. Contains level, hp, stats (`List<Stat>`), special stats, hunger, soul, speed, etc. Stats are stored as a JSON object (`{"str": 10, ...}`) and deserialized via `StatListConverter`.
- **`Gauge`** — Resource bar (health, mana, hunger, etc.) with `GaugeType` (Orb or IconBar). Keyed by `Guid`.
- **`Campaign`** — Groups `PlayerCharacter`s under a DM (`User`). Has `DefaultRollDie` and `StatModifierFormula`.
- **`User`** — Discord user, keyed by Discord ID string.
- **`RollResult`** / **`IndividualRollResult`** — Roll history stored per character.

### Blazor Components (`Components/`)

Pages live in `Components/Pages/` and use `@rendermode InteractiveServer` with `[Authorize(Policy = "HasDiscordId")]`. All data access is server-side only.

Key pages: `CharacterSelect`, `Character` (character sheet), `Campaigns`, `CampaignPage`, `DM`.

Reusable components: `GaugeDisplay`, `RollMenu`, `CharacterCRUD`, `CampaignCRUD`, `DescriptableList`, `Modal`, `Orb`, `IconBar`, `Slider`.

### `OrdisContext`

EF Core `DbContext` using Npgsql (PostgreSQL). Uses `IDbContextFactory<OrdisContext>` pattern — services call `dbFactory.CreateDbContextAsync()` to get short-lived contexts per operation.
