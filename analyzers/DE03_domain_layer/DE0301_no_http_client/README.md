# NoHttpClient (DE0301)

## Problem it prevents

Direct use of **`System.Net.Http.HttpClient`**: construction, fields, parameters, locals, and calls on `HttpClient` instances.

## Why that’s bad

- **Lifecycle and sockets** — Creating `HttpClient` per call (or misusing lifetimes) exhausts sockets and causes subtle production failures under load.
- **Untestable domain** — Domain code that new’s up `HttpClient` is tightly coupled to the network stack; unit tests become slow, flaky, or full of brittle mocks.
- **Wrong layer** — The domain should express *what* is needed (e.g. “fetch price”), not *how* HTTP is performed. Infrastructure (factories, typed clients, gateways) belongs outside the domain.

Blocking `HttpClient` in domain assemblies keeps HTTP details in the composition root and infrastructure layer.
