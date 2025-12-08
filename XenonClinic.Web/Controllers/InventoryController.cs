using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        UserManager<ApplicationUser> userManager,
        ILogger<InventoryController> logger)
    {
        _db = db;
        _branchService = branchService;
        _userManager = userManager;
        _logger = logger;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Any())
            {
                _logger.LogWarning("User has no assigned branches for inventory access");
                return View("Error");
            }

            var itemsQuery = _db.InventoryItems
                .Include(i => i.Branch)
                .Include(i => i.Supplier)
                .Where(i => branchIds.Contains(i.BranchId) && i.IsActive);

            var model = new InventoryDashboardViewModel
            {
                TotalItems = await itemsQuery.CountAsync(),
                LowStockItems = await itemsQuery.CountAsync(i => i.QuantityOnHand <= i.ReorderLevel && i.QuantityOnHand > 0),
                OutOfStockItems = await itemsQuery.CountAsync(i => i.QuantityOnHand == 0),
                TotalInventoryValue = await itemsQuery.SumAsync(i => i.QuantityOnHand * i.CostPrice),

                RecentlyAddedItems = await itemsQuery
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(5)
                    .Select(i => new InventoryItemDto
                    {
                        Id = i.Id,
                        ItemCode = i.ItemCode,
                        Name = i.Name,
                        Description = i.Description,
                        Category = i.Category,
                        CategoryDisplay = i.Category.ToString(),
                        BranchId = i.BranchId,
                        BranchName = i.Branch.Name,
                        QuantityOnHand = i.QuantityOnHand,
                        ReorderLevel = i.ReorderLevel,
                        MaxStockLevel = i.MaxStockLevel,
                        CostPrice = i.CostPrice,
                        SellingPrice = i.SellingPrice,
                        SupplierId = i.SupplierId,
                        SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                        Location = i.Location,
                        ExpiryDate = i.ExpiryDate,
                        IsActive = i.IsActive,
                        IsLowStock = i.IsLowStock,
                        IsOutOfStock = i.IsOutOfStock,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync(),

                LowStockAlerts = await itemsQuery
                    .Where(i => i.QuantityOnHand <= i.ReorderLevel)
                    .OrderBy(i => i.QuantityOnHand)
                    .Take(10)
                    .Select(i => new InventoryItemDto
                    {
                        Id = i.Id,
                        ItemCode = i.ItemCode,
                        Name = i.Name,
                        Description = i.Description,
                        Category = i.Category,
                        CategoryDisplay = i.Category.ToString(),
                        BranchId = i.BranchId,
                        BranchName = i.Branch.Name,
                        QuantityOnHand = i.QuantityOnHand,
                        ReorderLevel = i.ReorderLevel,
                        IsLowStock = i.IsLowStock,
                        IsOutOfStock = i.IsOutOfStock
                    })
                    .ToListAsync(),

                RecentTransactions = await _db.InventoryTransactions
                    .Include(t => t.InventoryItem)
                    .ThenInclude(i => i.Branch)
                    .Include(t => t.Patient)
                    .Include(t => t.TransferToBranch)
                    .Where(t => branchIds.Contains(t.InventoryItem.BranchId))
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new InventoryTransactionDto
                    {
                        Id = t.Id,
                        InventoryItemId = t.InventoryItemId,
                        InventoryItemName = t.InventoryItem.Name,
                        ItemCode = t.InventoryItem.ItemCode,
                        TransactionType = t.TransactionType,
                        TransactionTypeDisplay = t.TransactionType.ToString(),
                        Quantity = t.Quantity,
                        UnitPrice = t.UnitPrice,
                        TotalAmount = t.TotalAmount,
                        TransactionDate = t.TransactionDate,
                        ReferenceNumber = t.ReferenceNumber,
                        PatientId = t.PatientId,
                        PatientName = t.Patient != null ? t.Patient.FullNameEn : null,
                        TransferToBranchId = t.TransferToBranchId,
                        TransferToBranchName = t.TransferToBranch != null ? t.TransferToBranch.Name : null,
                        PerformedBy = t.PerformedBy,
                        Notes = t.Notes,
                        QuantityAfterTransaction = t.QuantityAfterTransaction
                    })
                    .ToListAsync()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory dashboard");
            return View("Error");
        }
    }

    // List all items
    public async Task<IActionResult> Items(InventoryCategory? category, string? search, int pageNumber = 1)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Any())
            {
                return View("Error");
            }

            var itemsQuery = _db.InventoryItems
                .Include(i => i.Branch)
                .Include(i => i.Supplier)
                .Where(i => branchIds.Contains(i.BranchId) && i.IsActive);

            if (category.HasValue)
            {
                itemsQuery = itemsQuery.Where(i => i.Category == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                itemsQuery = itemsQuery.Where(i =>
                    i.ItemCode.Contains(search) ||
                    i.Name.Contains(search) ||
                    i.Description.Contains(search));
            }

            var items = await itemsQuery
                .OrderBy(i => i.Name)
                .Select(i => new InventoryItemDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    CategoryDisplay = i.Category.ToString(),
                    BranchId = i.BranchId,
                    BranchName = i.Branch.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    ReorderLevel = i.ReorderLevel,
                    MaxStockLevel = i.MaxStockLevel,
                    CostPrice = i.CostPrice,
                    SellingPrice = i.SellingPrice,
                    SupplierId = i.SupplierId,
                    SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                    Location = i.Location,
                    ExpiryDate = i.ExpiryDate,
                    IsActive = i.IsActive,
                    IsLowStock = i.IsLowStock,
                    IsOutOfStock = i.IsOutOfStock,
                    CreatedDate = i.CreatedDate
                })
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.Search = search;
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory items");
            return View("Error");
        }
    }

    // Create Item - GET
    public async Task<IActionResult> CreateItem()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    // Create Item - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(CreateInventoryItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        try
        {
            // Verify branch access
            if (!await _branchService.HasAccessToBranchAsync(model.BranchId))
            {
                _logger.LogWarning("User attempted to create item in unauthorized branch: {BranchId}", model.BranchId);
                return Forbid();
            }

            // Check for duplicate item code
            if (await _db.InventoryItems.AnyAsync(i => i.ItemCode == model.ItemCode))
            {
                ModelState.AddModelError("ItemCode", "Item code already exists");
                await PopulateDropdownsAsync();
                return View(model);
            }

            var item = new InventoryItem
            {
                ItemCode = model.ItemCode,
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                BranchId = model.BranchId,
                QuantityOnHand = model.QuantityOnHand,
                ReorderLevel = model.ReorderLevel,
                MaxStockLevel = model.MaxStockLevel,
                CostPrice = model.CostPrice,
                SellingPrice = model.SellingPrice,
                SupplierId = model.SupplierId,
                SupplierPartNumber = model.SupplierPartNumber,
                Barcode = model.Barcode,
                Location = model.Location,
                ExpiryDate = model.ExpiryDate,
                Notes = model.Notes,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();

            // Create initial stock transaction
            if (model.QuantityOnHand > 0)
            {
                var user = await _userManager.GetUserAsync(User);
                var transaction = new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    TransactionType = InventoryTransactionType.Purchase,
                    Quantity = model.QuantityOnHand,
                    UnitPrice = model.CostPrice,
                    TotalAmount = model.QuantityOnHand * model.CostPrice,
                    TransactionDate = DateTime.UtcNow,
                    PerformedBy = user?.Email,
                    Notes = "Initial stock",
                    QuantityAfterTransaction = model.QuantityOnHand
                };

                _db.InventoryTransactions.Add(transaction);
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Inventory item created: {ItemCode} - {Name}", item.ItemCode, item.Name);
            return RedirectToAction(nameof(Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item");
            ModelState.AddModelError("", "An error occurred while creating the item");
            await PopulateDropdownsAsync();
            return View(model);
        }
    }

    // Edit Item - GET
    public async Task<IActionResult> EditItem(int id)
    {
        var item = await _db.InventoryItems.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        if (!await _branchService.HasAccessToBranchAsync(item.BranchId))
        {
            return Forbid();
        }

        var model = new CreateInventoryItemViewModel
        {
            ItemCode = item.ItemCode,
            Name = item.Name,
            Description = item.Description,
            Category = item.Category,
            BranchId = item.BranchId,
            QuantityOnHand = item.QuantityOnHand,
            ReorderLevel = item.ReorderLevel,
            MaxStockLevel = item.MaxStockLevel,
            CostPrice = item.CostPrice,
            SellingPrice = item.SellingPrice,
            SupplierId = item.SupplierId,
            SupplierPartNumber = item.SupplierPartNumber,
            Barcode = item.Barcode,
            Location = item.Location,
            ExpiryDate = item.ExpiryDate,
            Notes = item.Notes,
            IsActive = item.IsActive
        };

        await PopulateDropdownsAsync();
        ViewBag.ItemId = id;
        return View(model);
    }

    // Edit Item - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(int id, CreateInventoryItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            ViewBag.ItemId = id;
            return View(model);
        }

        try
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            if (!await _branchService.HasAccessToBranchAsync(item.BranchId))
            {
                return Forbid();
            }

            // Check for duplicate item code (excluding current item)
            if (await _db.InventoryItems.AnyAsync(i => i.ItemCode == model.ItemCode && i.Id != id))
            {
                ModelState.AddModelError("ItemCode", "Item code already exists");
                await PopulateDropdownsAsync();
                ViewBag.ItemId = id;
                return View(model);
            }

            item.ItemCode = model.ItemCode;
            item.Name = model.Name;
            item.Description = model.Description;
            item.Category = model.Category;
            item.ReorderLevel = model.ReorderLevel;
            item.MaxStockLevel = model.MaxStockLevel;
            item.CostPrice = model.CostPrice;
            item.SellingPrice = model.SellingPrice;
            item.SupplierId = model.SupplierId;
            item.SupplierPartNumber = model.SupplierPartNumber;
            item.Barcode = model.Barcode;
            item.Location = model.Location;
            item.ExpiryDate = model.ExpiryDate;
            item.Notes = model.Notes;
            item.IsActive = model.IsActive;
            item.LastModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Inventory item updated: {ItemCode} - {Name}", item.ItemCode, item.Name);
            return RedirectToAction(nameof(Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item");
            ModelState.AddModelError("", "An error occurred while updating the item");
            await PopulateDropdownsAsync();
            ViewBag.ItemId = id;
            return View(model);
        }
    }

    // Create Transaction - GET
    public async Task<IActionResult> CreateTransaction(int? itemId)
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();

        ViewBag.Items = await _db.InventoryItems
            .Where(i => branchIds.Contains(i.BranchId) && i.IsActive)
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.ItemCode} - {i.Name} (Stock: {i.QuantityOnHand})"
            })
            .ToListAsync();

        ViewBag.Patients = await _db.Patients
            .Where(p => branchIds.Contains(p.BranchId))
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FullNameEn
            })
            .ToListAsync();

        ViewBag.Branches = await _db.Branches
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            })
            .ToListAsync();

        var model = new CreateInventoryTransactionViewModel();
        if (itemId.HasValue)
        {
            model.InventoryItemId = itemId.Value;
            var item = await _db.InventoryItems.FindAsync(itemId.Value);
            if (item != null)
            {
                model.UnitPrice = item.CostPrice;
            }
        }

        return View(model);
    }

    // Create Transaction - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(CreateInventoryTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateTransactionDropdownsAsync();
            return View(model);
        }

        try
        {
            var item = await _db.InventoryItems.FindAsync(model.InventoryItemId);
            if (item == null)
            {
                ModelState.AddModelError("", "Inventory item not found");
                await PopulateTransactionDropdownsAsync();
                return View(model);
            }

            if (!await _branchService.HasAccessToBranchAsync(item.BranchId))
            {
                return Forbid();
            }

            // Calculate quantity change based on transaction type
            int quantityChange = model.TransactionType switch
            {
                InventoryTransactionType.Purchase => model.Quantity,
                InventoryTransactionType.Sale => -model.Quantity,
                InventoryTransactionType.Return => model.Quantity,
                InventoryTransactionType.Transfer => -model.Quantity,
                InventoryTransactionType.Adjustment => model.Quantity, // Can be positive or negative
                _ => 0
            };

            // Check for sufficient stock
            if (item.QuantityOnHand + quantityChange < 0)
            {
                ModelState.AddModelError("Quantity", "Insufficient stock for this transaction");
                await PopulateTransactionDropdownsAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var transaction = new InventoryTransaction
            {
                InventoryItemId = model.InventoryItemId,
                TransactionType = model.TransactionType,
                Quantity = quantityChange,
                UnitPrice = model.UnitPrice,
                TotalAmount = Math.Abs(quantityChange) * model.UnitPrice,
                TransactionDate = model.TransactionDate,
                ReferenceNumber = model.ReferenceNumber,
                PatientId = model.PatientId,
                TransferToBranchId = model.TransferToBranchId,
                PerformedBy = user?.Email,
                Notes = model.Notes,
                QuantityAfterTransaction = item.QuantityOnHand + quantityChange
            };

            // Update item quantity
            item.QuantityOnHand += quantityChange;
            item.LastModifiedDate = DateTime.UtcNow;

            _db.InventoryTransactions.Add(transaction);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Inventory transaction created: {ItemCode} - {Type} - {Quantity}",
                item.ItemCode, model.TransactionType, quantityChange);

            return RedirectToAction(nameof(Transactions), new { itemId = model.InventoryItemId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory transaction");
            ModelState.AddModelError("", "An error occurred while creating the transaction");
            await PopulateTransactionDropdownsAsync();
            return View(model);
        }
    }

    // View Transactions
    public async Task<IActionResult> Transactions(int? itemId, DateTime? fromDate, DateTime? toDate, int pageNumber = 1)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Any())
            {
                return View("Error");
            }

            var transactionsQuery = _db.InventoryTransactions
                .Include(t => t.InventoryItem)
                .ThenInclude(i => i.Branch)
                .Include(t => t.Patient)
                .Include(t => t.TransferToBranch)
                .Where(t => branchIds.Contains(t.InventoryItem.BranchId));

            if (itemId.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.InventoryItemId == itemId.Value);
                var item = await _db.InventoryItems.FindAsync(itemId.Value);
                ViewBag.ItemName = item?.Name;
            }

            if (fromDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= toDate.Value);
            }

            var transactions = await transactionsQuery
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new InventoryTransactionDto
                {
                    Id = t.Id,
                    InventoryItemId = t.InventoryItemId,
                    InventoryItemName = t.InventoryItem.Name,
                    ItemCode = t.InventoryItem.ItemCode,
                    TransactionType = t.TransactionType,
                    TransactionTypeDisplay = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    UnitPrice = t.UnitPrice,
                    TotalAmount = t.TotalAmount,
                    TransactionDate = t.TransactionDate,
                    ReferenceNumber = t.ReferenceNumber,
                    PatientId = t.PatientId,
                    PatientName = t.Patient != null ? t.Patient.FullNameEn : null,
                    TransferToBranchId = t.TransferToBranchId,
                    TransferToBranchName = t.TransferToBranch != null ? t.TransferToBranch.Name : null,
                    PerformedBy = t.PerformedBy,
                    Notes = t.Notes,
                    QuantityAfterTransaction = t.QuantityAfterTransaction
                })
                .ToListAsync();

            ViewBag.ItemId = itemId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory transactions");
            return View("Error");
        }
    }

    // Helper methods
    private async Task PopulateDropdownsAsync()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();

        ViewBag.Branches = await _db.Branches
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            })
            .ToListAsync();

        ViewBag.Suppliers = await _db.Suppliers
            .Where(s => s.IsActive)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();
    }

    private async Task PopulateTransactionDropdownsAsync()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();

        ViewBag.Items = await _db.InventoryItems
            .Where(i => branchIds.Contains(i.BranchId) && i.IsActive)
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{i.ItemCode} - {i.Name} (Stock: {i.QuantityOnHand})"
            })
            .ToListAsync();

        ViewBag.Patients = await _db.Patients
            .Where(p => branchIds.Contains(p.BranchId))
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FullNameEn
            })
            .ToListAsync();

        ViewBag.Branches = await _db.Branches
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            })
            .ToListAsync();
    }
}
