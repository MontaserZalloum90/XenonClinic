# Database Migration Guide - Dynamic Lookup System

## Overview
This guide provides instructions for creating and applying the database migration for the new dynamic lookup system that replaces hard-coded enums with database-backed lookup tables.

## What Changed
The system has been refactored to eliminate all hard-coded enums and replace them with dynamic, database-backed lookup tables that can be managed through the admin interface.

### New Lookup Tables (28 total)
The following lookup entities have been created, all inheriting from `SystemLookup`:

**Appointments & Scheduling:**
- AppointmentTypeLookup
- AppointmentStatusLookup

**Case Management:**
- CasePriorityLookup
- CaseActivityTypeLookup
- CaseActivityStatusLookup
- CaseNoteTypeLookup

**HR & Employee Management:**
- LeaveTypeLookup
- LeaveStatusLookup
- EmploymentStatusLookup
- AttendanceStatusLookup

**Inventory Management:**
- InventoryCategoryLookup
- InventoryTransactionTypeLookup

**Clinical Data:**
- HearingLossTypeLookup
- SpecimenTypeLookup
- TestCategoryLookup

**Financial Management:**
- PaymentMethodLookup
- PaymentStatusLookup
- AccountTypeLookup
- ExpenseStatusLookup
- TransactionTypeLookup
- VoucherStatusLookup

**Sales & Procurement:**
- QuotationStatusLookup
- SaleStatusLookup
- PurchaseOrderStatusLookup
- GoodsReceiptStatusLookup
- SupplierPaymentStatusLookup

**Laboratory:**
- LabOrderStatusLookup
- LabResultStatusLookup

### Updated Entities
The following entities have been updated to use lookup foreign keys instead of enums:
- **Case**: Now uses `CasePriorityId` instead of `CasePriority` enum
- **CaseActivity**: Now uses `CaseActivityTypeId`, `CaseActivityStatusId`, and `CasePriorityId`
- **CaseNote**: Now uses `CaseNoteTypeId` instead of `CaseNoteType` enum
- **CaseStatus**: Category changed from enum to string field

## Prerequisites
- .NET 8.0 SDK installed
- EF Core CLI tools installed (`dotnet tool install --global dotnet-ef`)
- Database connection string configured in appsettings.json

## Step 1: Create the Migration

Navigate to the solution root directory and run:

```bash
cd XenonClinic.Infrastructure
dotnet ef migrations add AddDynamicLookupSystem --startup-project ../XenonClinic.Web
```

This will generate a new migration file in `XenonClinic.Infrastructure/Migrations/` with all the necessary schema changes.

## Step 2: Review the Migration

Open the generated migration file and verify it includes:
- CreateTable statements for all 28 lookup tables
- Foreign key constraints for Case, CaseActivity, and CaseNote entities
- Indexes for performance optimization
- Any necessary data migrations

## Step 3: Apply the Migration

To update your database with the new schema:

```bash
cd XenonClinic.Web
dotnet ef database update --project ../XenonClinic.Infrastructure
```

Or run the application - migrations will be applied automatically if `context.Database.EnsureCreatedAsync()` is called in `SeedData.InitializeAsync()`.

## Step 4: Verify Seed Data

The seed data will be automatically populated on application startup via `LookupSeeder.SeedLookupsAsync()`. Verify that:

1. All 28 lookup tables have default system values
2. Each lookup has appropriate color codes and icon classes
3. System default values are marked with `IsSystemDefault = true`
4. All values are active (`IsActive = true`)

You can verify this by:
```sql
-- Check lookup counts
SELECT 'AppointmentTypeLookup' AS TableName, COUNT(*) AS Count FROM AppointmentTypeLookups
UNION ALL
SELECT 'AppointmentStatusLookup', COUNT(*) FROM AppointmentStatusLookups
-- ... repeat for all 28 tables
```

## Step 5: Access Lookup Management UI

After migration and seeding:

1. Log in as SuperAdmin or TenantAdmin
2. Navigate to Admin Dashboard
3. Click on "Lookup Management"
4. You should see all 28 lookup types organized by category
5. Test CRUD operations (Create, Edit, Delete) for each lookup type

## Breaking Changes

### Code Updates Required
The following code patterns need to be updated throughout the codebase:

**Before (using enums):**
```csharp
if (appointment.Status == AppointmentStatus.Completed)
if (caseActivity.Priority == CasePriority.High)
```

**After (using lookup IDs):**
```csharp
// Option 1: Compare IDs
if (appointment.AppointmentStatusId == completedStatusId)

// Option 2: Load navigation and compare
if (appointment.AppointmentStatus.Code == "COMPLETED")

// Option 3: Use lookup service
var completedStatus = await _lookupService.GetLookupByCodeAsync<AppointmentStatusLookup>("COMPLETED");
if (appointment.AppointmentStatusId == completedStatus.Id)
```

### Service Layer Updates
Services that currently use enum comparisons need to be updated to query lookup tables:

```csharp
// Before
var pendingAppointments = appointments.Where(a => a.Status == AppointmentStatus.Pending);

// After
var pendingStatus = await _lookupService.GetLookupByCodeAsync<AppointmentStatusLookup>("PENDING");
var pendingAppointments = appointments.Where(a => a.AppointmentStatusId == pendingStatus.Id);
```

## Rollback Instructions

If you need to rollback the migration:

```bash
cd XenonClinic.Web
dotnet ef database update <PreviousMigrationName> --project ../XenonClinic.Infrastructure
```

Then remove the migration file:
```bash
cd XenonClinic.Infrastructure
dotnet ef migrations remove --startup-project ../XenonClinic.Web
```

## Testing Checklist

After applying the migration:

- [ ] All 28 lookup tables created successfully
- [ ] Foreign key constraints added to Case, CaseActivity, CaseNote
- [ ] Seed data populated for all lookup tables
- [ ] Lookup Management UI accessible and functional
- [ ] Can create new lookup values
- [ ] Can edit existing lookup values
- [ ] Cannot delete system default values
- [ ] Can deactivate/reactivate lookup values
- [ ] Tenant-scoped lookups work correctly
- [ ] Color codes and icons display properly in UI

## Support

For issues or questions:
- Check application logs in `XenonClinic.Web/Logs/`
- Review EF Core migration history: `SELECT * FROM __EFMigrationsHistory`
- Verify DbContext configuration in `ClinicDbContext.cs`

## Additional Notes

### Performance Considerations
- All lookup tables are relatively small (typically < 100 rows)
- Consider caching frequently accessed lookups
- Indexes are automatically created on foreign keys

### Future Enhancements
- Add bulk import/export for lookup values
- Implement lookup value versioning
- Add audit trail for lookup changes
- Support multi-language lookup names

## Files Modified

**Core Layer:**
- `SystemLookup.cs` - Base class for all lookups
- 28 lookup entity files in `Core/Entities/Lookups/`
- `Case.cs`, `CaseActivity.cs`, `CaseNote.cs`, `CaseStatus.cs` - Updated to use lookups
- `ILookupService.cs` - Service interface
- `RoleConstants.cs` - Centralized role constants

**Infrastructure Layer:**
- `ClinicDbContext.cs` - Added 28 DbSets
- `LookupService.cs` - Service implementation
- `LookupSeeder.cs` - Seed data for all lookups
- `SeedData.cs` - Integration of lookup seeding

**Web Layer:**
- `LookupManagementController.cs` - CRUD controller
- `LookupManagementViewModels.cs` - ViewModels
- `Index.cshtml`, `List.cshtml`, `Edit.cshtml` - Admin views
- `Admin/Index.cshtml` - Updated with navigation
- `Program.cs` - Service registration

**Configuration:**
- 50+ role string references replaced with `RoleConstants`
