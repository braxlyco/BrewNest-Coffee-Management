using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Catalog.Models;

internal static class CatalogPermissionNames
{
    public const string MasterDataRead="catalog.masterdata.read"; public const string MasterDataManage="catalog.masterdata.manage";
    public const string RecipesRead="catalog.recipes.read"; public const string RecipesManage="catalog.recipes.manage";
    public const string QualityRead="catalog.quality.read"; public const string QualityManage="catalog.quality.manage";
}
internal class CodeNameForm { [Required,StringLength(30)] public string Code{get;set;}=string.Empty; [Required,StringLength(200)] public string Name{get;set;}=string.Empty; }
internal sealed class UnitForm:CodeNameForm { [Required,StringLength(20)] public string Symbol{get;set;}=string.Empty; [Required] public string Category{get;set;}="Count"; [Range(0,6)] public int DecimalPlaces{get;set;}=3; }
internal sealed class GroupForm:CodeNameForm { public string AppliesTo{get;set;}=string.Empty; public bool RequiresInspection{get;set;} }
internal sealed class ItemForm:CodeNameForm,IValidatableObject { public string Description{get;set;}=string.Empty; [Required] public string ItemType{get;set;}="MenuItem"; [Required] public Guid? UnitId{get;set;} public Guid? GroupId{get;set;} [Range(0,double.MaxValue)] public decimal? SalesPrice{get;set;} [Range(0,double.MaxValue)] public decimal StandardCost{get;set;} public int? ShelfLifeDays{get;set;} public bool RequiresInspection{get;set;}
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(ItemType=="MenuItem"&&SalesPrice is null)yield return new ValidationResult("Menu items require a sales price.",[nameof(SalesPrice)]);
        if(ItemType!="MenuItem"&&SalesPrice is not null)yield return new ValidationResult("Only menu items may have a sales price.",[nameof(SalesPrice)]);
        if(ShelfLifeDays is <=0)yield return new ValidationResult("Shelf life must be positive when supplied.",[nameof(ShelfLifeDays)]);
    }
}
internal sealed class PartyForm:CodeNameForm { public string ContactPerson{get;set;}=string.Empty; [RegularExpression(@"^\+[1-9][0-9]{7,14}$",ErrorMessage="Use country code and 8–15 digits, e.g. +923001234567.")] public string Phone{get;set;}=string.Empty; [EmailAddress] public string Email{get;set;}=string.Empty; public string Address{get;set;}=string.Empty; public string TaxNumber{get;set;}=string.Empty; }
internal sealed class TableForm:CodeNameForm { [Range(1,100)] public int Capacity{get;set;}=2; public string Area{get;set;}=string.Empty; }
internal sealed class WarehouseForm:CodeNameForm { public string Address{get;set;}=string.Empty; }
internal sealed class RiderForm:CodeNameForm { [Required,RegularExpression(@"^\+[1-9][0-9]{7,14}$",ErrorMessage="Use country code and 8–15 digits, e.g. +923001234567.")] public string Phone{get;set;}=string.Empty; public string VehicleRegistration{get;set;}=string.Empty; public bool IsAvailable{get;set;}=true; }
internal sealed class RecipeForm:CodeNameForm { [Required] public Guid? MenuItemId{get;set;} [Range(0.001,double.MaxValue)] public decimal YieldQuantity{get;set;}=1; [Required] public Guid? YieldUnitId{get;set;} [Required] public Guid? IngredientId{get;set;} [Range(0.001,double.MaxValue)] public decimal IngredientQuantity{get;set;}=1; [Range(0,100)] public decimal WastagePercentage{get;set;} }
internal sealed class InspectionForm:IValidatableObject { public Guid? ItemId{get;set;} public Guid? GroupId{get;set;} [Required,StringLength(200)] public string Name{get;set;}=string.Empty; [Required,StringLength(200)] public string CriterionName{get;set;}=string.Empty; [Required] public string ResultType{get;set;}="PassFail"; public string? MinimumValue{get;set;} public string? MaximumValue{get;set;} public string? Unit{get;set;} public bool IsMandatory{get;set;}=true;
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if((ItemId is null)==(GroupId is null))yield return new ValidationResult("Select either one item or one group.",[nameof(ItemId),nameof(GroupId)]);
    }
}
