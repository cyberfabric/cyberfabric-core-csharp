# NoConsoleUsage (DE1302)

## Problem it prevents

Any use of **`System.Console`** (e.g. `Console.WriteLine`, `Console.ReadLine`, `using static System.Console`).

## Why that’s bad

- **No structure or levels** — Console output bypasses log levels, correlation IDs, and scopes, so you cannot filter or trace requests in production.
- **Wrong in server apps** — In ASP.NET and most services, stdout is not a supported “user interface”; important messages are lost or mixed with host noise.
- **Operational blind spots** — Monitoring, alerting, and centralized logging depend on a proper logging pipeline (`ILogger`, Serilog, etc.). Console writes don’t integrate with that model.

Forcing logging APIs instead of `Console` keeps diagnostics consistent and production-ready.
