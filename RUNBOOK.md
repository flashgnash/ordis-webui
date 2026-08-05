# Disposable OW3N browser-test runbook

These flake commands are for local development and automated browser testing. They do not use `.env`, deployed OW3N services, a persistent PostgreSQL cluster, or real Discord credentials.

## Start the disposable stack

```bash
XDG_CACHE_HOME=/tmp/ow3n-nix nix run .#test-serve
```

The command builds/reuses the existing flake package, then starts:

- PostgreSQL in a new temporary datadir and Unix socket. It runs the existing `initDb.sql`, creates an `ow3n` database, and OW3N applies its existing EF migrations/schema seed on startup.
- The packaged Blazor Server app on a kernel-selected, unreserved high port bound to `127.0.0.1`. The command prints the exact `URL:` after an HTTP readiness check.
- A loopback-only roll-service and Discord-webhook stub on the product's hard-coded `127.0.0.1:3000` dependency. Stub rolls deterministically return `1`; webhook posts return `204`.
- Dummy Discord client values. Real Discord authentication is intentionally unavailable; do not follow `/login` to Discord in browser tests.
- The app is started with OW3N's TEST-ONLY `--testing` auth bypass (see below), so browser tests are auto-authenticated and can reach the roll panel without Discord OAuth.

Dependencies come entirely from `flake.nix`: PostgreSQL, Python, curl, and the existing OW3N package/runtime. Nix itself is the only host prerequisite.

## TEST-ONLY authentication bypass (`--testing`)

`nix run .#test-serve` launches OW3N with the `--testing` command-line flag. This is the **only** way to enable the bypass — there is deliberately no environment-variable or configuration fallback, so it is **fail-closed** and can never be switched on in production. Without `--testing`, authentication is exactly today's normal Discord OAuth and the root page redirects to Discord login. Implementation and inline docs live in `TestingSupport.cs`.

When `--testing` is set:

- Requests are auto-authenticated as **test accounts** — any number of them. Each account is identified by an **auto-generated integer id capped at 3 digits (0-999)**. Real Discord snowflakes are 17-19 digits, so a test id can never collide with, or be mistaken for, a real Discord user id. Any 4+ digit / real-style id is **rejected** — a test account can never assume a real Discord id.
- A default set of test accounts (ids `1`, `2`, `3`) and a usable **test character** for each are seeded at startup, so the roll panel is reachable out of the box.
- With no account selected, a request authenticates as the default test account (id `1`). Hitting the app directly therefore "just works".

### Selecting / creating test accounts from a browser test

- Add `?testUser=N` (N in `0`-`999`) to any URL to authenticate as that specific account for the request, e.g. `URL/?testUser=2`.
- Visit `/testlogin?testUser=N` to sign in as account N for the whole browser session (it seeds the account on demand and sets a persistent cookie), then browse normally.
- Visit `/testlogin` with no id to **auto-generate** the next free 3-digit account, seed it, and sign in.
- A request/endpoint given a 4+ digit or non-numeric id is rejected (HTTP 401 on a protected page; HTTP 400 from `/testlogin`).

`/testlogin` is mapped **only** under `--testing`; it does not exist in a normal build.

Port `3000` must be free. The launcher refuses to start if it is occupied, so it can never connect tests to an unknown roll service. The OW3N and PostgreSQL ports are chosen dynamically; the launcher explicitly excludes `3000`, `5098`, `5099`, `5100`, `5111`, and `7000`.

Stop with **Ctrl-C** (or terminate the process). The exit trap stops OW3N, the stub, and PostgreSQL, then deletes the complete temporary runtime directory. A crash or normal process exit runs the same cleanup. No project or user database files are written.

## Build in the development sandbox

```bash
XDG_CACHE_HOME=/tmp/ow3n-nix nix run .#build
```

This wraps the repository's normal build as `dotnet build --nologo -p:UseAppHost=false`, with the flake-provided .NET SDK, Node.js, and Sass. It uses the supplied `XDG_CACHE_HOME`, or `/tmp/ow3n-build-cache` when that variable is unset. Additional `dotnet build` arguments may be appended after `--`.

The existing hot-reload workflow remains available for configured developer environments:

```bash
nix develop --command run
```

Unlike `test-serve`, that existing command loads `.env` and is not disposable; do not use it for automated browser tests.
