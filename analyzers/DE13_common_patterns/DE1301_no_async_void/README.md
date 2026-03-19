# NoAsyncVoid (DE1301)

## Problem it prevents

**`async void`** in any form: instance/static methods, local functions, and lambdas (e.g. `async () => { }`).

## Why that’s bad

- **Exceptions can’t be observed** — Failures become unhandled exceptions on the synchronization context; callers cannot `await` or catch them reliably.
- **No composable task** — There is no `Task` to return, so composition, cancellation, and sequencing with other async work break down.
- **Misleading for “fire and forget”** — Teams often use `async void` for background work; that pattern loses error reporting and is easy to get wrong.

Use **`async Task`**, **`async Task<T>`**, or **`async ValueTask`** so callers can await and exceptions propagate predictably.
