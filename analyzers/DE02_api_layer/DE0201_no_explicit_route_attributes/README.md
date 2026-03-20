# NoExplicitRouteAttributes (DE0201)

## Problem it prevents

Use of **`[Route]`** / **`Microsoft.AspNetCore.Mvc.RouteAttribute`** anywhere in the project—on controllers, actions, or other declarations.

## Why that’s bad

- **Fragmented routing** — Route templates scattered on types and methods are hard to review, search, and evolve. Central configuration (endpoint mapping, conventional routing, or a single versioning policy) keeps the HTTP surface in one place.
- **Inconsistent conventions** — Mixed attribute routes and programmatic routes make it unclear how URLs are formed, which hurts onboarding and security reviews.
- **Harder automation** — OpenAPI, gateways, and AI-generated code benefit from predictable patterns; banning explicit `[Route]` nudges teams toward one approved routing model.

This analyzer fails the build when `RouteAttribute` is used so routing stays deliberate and uniform.
