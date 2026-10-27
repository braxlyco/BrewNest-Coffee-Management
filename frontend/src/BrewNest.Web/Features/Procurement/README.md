# Procurement frontend feature

Contains the dashboard and feature-local pages for requisitions, sourcing/quotation comparison, purchase-order approval, and receipt/inspection review. `ProcurementApiClient` is the typed HTTP boundary; pages contain presentation state only and rely on backend validation and authorization.

Routes begin at `/procurement`. Keep future components, validation, and state in this feature rather than the global services folder.
