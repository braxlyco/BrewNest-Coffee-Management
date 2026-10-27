# Catalog frontend feature

Permission-aware Blazor pages for item/unit/group masters, customers/vendors, dining tables, warehouses, riders, recipe/BOM definitions, and configurable quality-inspection criteria. Components call the typed `CatalogApiClient`; domain validation and persistence behavior remain in the backend module.

Phone fields use a leading `+` country code and 8–15 digits (for example `+923001234567`); customer and vendor phones are optional, while rider phones are required. Item group options follow the selected item type. Only active Catalog references appear in item and recipe selectors.

Contains the Blazor UI for the Catalog module. Keep pages, components, API services, view models, validation, and state in their matching subfolders. UI code calls typed services and must not contain business rules.
