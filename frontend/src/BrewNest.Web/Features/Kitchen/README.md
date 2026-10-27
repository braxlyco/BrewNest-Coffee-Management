# Kitchen frontend feature

Contains the Blazor UI for the Kitchen module. Keep pages, components, API services, view models, validation, and state in their matching subfolders. UI code calls typed services and must not contain business rules.

Routes:

- `/kitchen` — permission-aware feature landing page.
- `/kitchen/queue` — live KOT generation and preparation board.
- `/kitchen/history` — append-only status history.

`KitchenApiClient` owns HTTP communication, `KitchenQueueState` owns the current projection, and `BrewNestRealtimeClient` triggers an API reload after authenticated `KitchenQueueChanged` notifications.
