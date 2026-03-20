# NoTaskResult (DE1303)

## Problem it prevents

Blocking waits on tasks:

- **`Task<TResult>.Result`**
- **`Task` / `Task<T>.GetAwaiter().GetResult()`**

## Why that’s bad

- **Deadlocks** — On contexts that capture a synchronization context (e.g. UI or some ASP.NET pipelines), blocking on an incomplete task can deadlock the thread waiting on itself.
- **Throughput** — Blocking threads ties up thread-pool threads that could serve other work, hurting scalability.
- **Async corruption** — Mixing “async all the way” with sync-over-async hides the real asynchronous flow and makes code harder to reason about and test.

Prefer **`await`**, asynchronous APIs, or explicit background execution instead of blocking on tasks.
