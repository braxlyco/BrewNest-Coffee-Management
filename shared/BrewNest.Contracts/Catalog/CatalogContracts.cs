namespace BrewNest.Contracts.Catalog;

/// <summary>Unit-of-measure master data.</summary>
public sealed record UnitOfMeasureResponse(Guid Id, string Code, string Name, string Symbol, string Category, int DecimalPlaces, bool IsActive, string Version);
/// <summary>Creates a unit of measure.</summary>
public sealed record CreateUnitOfMeasureRequest(string Code, string Name, string Symbol, string Category, int DecimalPlaces);
/// <summary>Updates a unit of measure.</summary>
public sealed record UpdateUnitOfMeasureRequest(string Code, string Name, string Symbol, string Category, int DecimalPlaces, bool IsActive, string Version, string Reason);

/// <summary>Item-group master data.</summary>
public sealed record ItemGroupResponse(Guid Id, string Code, string Name, string AppliesTo, bool RequiresInspection, bool IsActive, string Version);
/// <summary>Creates an item group.</summary>
public sealed record CreateItemGroupRequest(string Code, string Name, string AppliesTo, bool RequiresInspection);
/// <summary>Updates an item group.</summary>
public sealed record UpdateItemGroupRequest(string Code, string Name, string AppliesTo, bool RequiresInspection, bool IsActive, string Version, string Reason);

/// <summary>Menu, raw-material, or packaging-material master data.</summary>
public sealed record CatalogItemResponse(Guid Id, string Code, string Name, string Description, string ItemType, Guid UnitOfMeasureId, Guid? ItemGroupId, decimal? SalesPrice, decimal StandardCost, int? ShelfLifeDays, bool RequiresInspection, bool IsActive, string Version);
/// <summary>Creates catalog item master data.</summary>
public sealed record CreateCatalogItemRequest(string Code, string Name, string Description, string ItemType, Guid UnitOfMeasureId, Guid? ItemGroupId, decimal? SalesPrice, decimal StandardCost, int? ShelfLifeDays, bool RequiresInspection);
/// <summary>Updates catalog item master data.</summary>
public sealed record UpdateCatalogItemRequest(string Code, string Name, string Description, Guid UnitOfMeasureId, Guid? ItemGroupId, decimal? SalesPrice, decimal StandardCost, int? ShelfLifeDays, bool RequiresInspection, bool IsActive, string Version, string Reason);

/// <summary>Customer master data.</summary>
public sealed record CustomerResponse(Guid Id, string Code, string Name, string Phone, string Email, string Address, bool IsActive, string Version);
/// <summary>Creates a customer.</summary>
public sealed record CreateCustomerRequest(string Code, string Name, string Phone, string Email, string Address);
/// <summary>Updates a customer.</summary>
public sealed record UpdateCustomerRequest(string Code, string Name, string Phone, string Email, string Address, bool IsActive, string Version, string Reason);

/// <summary>Vendor master data.</summary>
public sealed record VendorResponse(Guid Id, string Code, string Name, string ContactPerson, string Phone, string Email, string Address, string TaxRegistrationNumber, bool IsActive, string Version);
/// <summary>Creates a vendor.</summary>
public sealed record CreateVendorRequest(string Code, string Name, string ContactPerson, string Phone, string Email, string Address, string TaxRegistrationNumber);
/// <summary>Updates a vendor.</summary>
public sealed record UpdateVendorRequest(string Code, string Name, string ContactPerson, string Phone, string Email, string Address, string TaxRegistrationNumber, bool IsActive, string Version, string Reason);

/// <summary>Dining-table master data.</summary>
public sealed record DiningTableResponse(Guid Id, string Code, string Name, int Capacity, string Area, bool IsActive, string Version);
/// <summary>Creates a dining table.</summary>
public sealed record CreateDiningTableRequest(string Code, string Name, int Capacity, string Area);
/// <summary>Updates a dining table.</summary>
public sealed record UpdateDiningTableRequest(string Code, string Name, int Capacity, string Area, bool IsActive, string Version, string Reason);

/// <summary>Warehouse master data.</summary>
public sealed record WarehouseResponse(Guid Id, string Code, string Name, string Address, bool IsActive, string Version);
/// <summary>Creates a warehouse.</summary>
public sealed record CreateWarehouseRequest(string Code, string Name, string Address);
/// <summary>Updates a warehouse.</summary>
public sealed record UpdateWarehouseRequest(string Code, string Name, string Address, bool IsActive, string Version, string Reason);

/// <summary>Rider master data and availability.</summary>
public sealed record RiderResponse(Guid Id, string Code, string Name, string Phone, string VehicleRegistration, bool IsAvailable, bool IsActive, string Version);
/// <summary>Creates a rider.</summary>
public sealed record CreateRiderRequest(string Code, string Name, string Phone, string VehicleRegistration, bool IsAvailable);
/// <summary>Updates a rider.</summary>
public sealed record UpdateRiderRequest(string Code, string Name, string Phone, string VehicleRegistration, bool IsAvailable, bool IsActive, string Version, string Reason);

/// <summary>A recipe/BOM ingredient line.</summary>
public sealed record RecipeLineResponse(Guid Id, Guid IngredientItemId, decimal Quantity, decimal WastagePercentage, int Sequence);
/// <summary>Recipe/BOM for a menu item.</summary>
public sealed record RecipeResponse(Guid Id, string Code, string Name, Guid MenuItemId, decimal YieldQuantity, Guid YieldUnitOfMeasureId, bool IsActive, RecipeLineResponse[] Lines, string Version);
/// <summary>Recipe input line.</summary>
public sealed record RecipeLineRequest(Guid IngredientItemId, decimal Quantity, decimal WastagePercentage, int Sequence);
/// <summary>Creates a recipe/BOM.</summary>
public sealed record CreateRecipeRequest(string Code, string Name, Guid MenuItemId, decimal YieldQuantity, Guid YieldUnitOfMeasureId, RecipeLineRequest[] Lines);
/// <summary>Updates a recipe/BOM with a new active line revision.</summary>
public sealed record UpdateRecipeRequest(string Code, string Name, decimal YieldQuantity, Guid YieldUnitOfMeasureId, bool IsActive, RecipeLineRequest[] Lines, string Version, string Reason);

/// <summary>Configurable quality-inspection criterion.</summary>
public sealed record InspectionCriterionResponse(Guid Id, string Name, string ResultType, string? MinimumValue, string? MaximumValue, string? Unit, bool IsMandatory, int Sequence);
/// <summary>Item- or group-level inspection requirement.</summary>
public sealed record InspectionRequirementResponse(Guid Id, Guid? ItemId, Guid? ItemGroupId, string Name, bool IsRequired, bool IsActive, InspectionCriterionResponse[] Criteria, string Version);
/// <summary>Inspection-criterion input.</summary>
public sealed record InspectionCriterionRequest(string Name, string ResultType, string? MinimumValue, string? MaximumValue, string? Unit, bool IsMandatory, int Sequence);
/// <summary>Creates an inspection requirement.</summary>
public sealed record CreateInspectionRequirementRequest(Guid? ItemId, Guid? ItemGroupId, string Name, bool IsRequired, InspectionCriterionRequest[] Criteria);
/// <summary>Updates an inspection requirement with a new active criteria revision.</summary>
public sealed record UpdateInspectionRequirementRequest(string Name, bool IsRequired, bool IsActive, InspectionCriterionRequest[] Criteria, string Version, string Reason);

/// <summary>Append-only catalog status-history entry.</summary>
public sealed record CatalogStatusHistoryResponse(Guid Id, string EntityType, Guid EntityId, string PreviousStatus, string NewStatus, string Reason, string ChangedBy, DateTimeOffset ChangedAt);
