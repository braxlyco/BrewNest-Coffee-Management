# Returns frontend feature

Contains the Blazor management UI for return/RMA requests and void requests. The feature uses `ReturnsApiClient`, permission-aware navigation, frontend validation, optimistic-concurrency versions, and API Problem Details. Authoritative approval, inspection, restocking, refund, and void rules remain in the backend application service.

Return entry selects a Sales bill and one of its active lines. Void entry selects an eligible Sales order. Restocking inspection selects an active Catalog warehouse. Those dropdowns need the respective Sales read and Catalog warehouse read permissions in addition to Returns permissions.

Routes:

- `/returns` - Returns workspace
- `/returns/requests` - return/RMA entry, approval, inspection/disposition, and history
- `/returns/voids` - before/after-payment void requests and approval
