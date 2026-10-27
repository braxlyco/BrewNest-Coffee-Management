# Production frontend feature

Contains the Blazor UI for the Production module. Keep pages, components, API services, view models, validation, and state in their matching subfolders. UI code calls typed services and must not contain business rules.

Routes:

- `/production` — permission-aware landing page.
- `/production/plans` — plan creation, release, and cancellation.
- `/production/orders` — execution list.
- `/production/orders/{id}` — reservation, goods issue, completion, cancellation, and variance.
