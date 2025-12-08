using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class ProcurementController : Controller
{
    private readonly ClinicDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProcurementController(ClinicDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var viewModel = new ProcurementDashboardViewModel
        {
            TotalPurchaseOrders = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId)
                .CountAsync(),

            PendingApprovals = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId && p.Status == PurchaseOrderStatus.Submitted)
                .CountAsync(),

            ActivePurchaseOrders = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId &&
                       (p.Status == PurchaseOrderStatus.Approved ||
                        p.Status == PurchaseOrderStatus.Ordered ||
                        p.Status == PurchaseOrderStatus.PartiallyReceived))
                .CountAsync(),

            TotalPurchaseValue = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId && p.Status != PurchaseOrderStatus.Cancelled)
                .SumAsync(p => p.Total),

            OutstandingPayments = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId && p.Status != PurchaseOrderStatus.Cancelled)
                .SumAsync(p => p.Balance),

            TotalSuppliers = await _context.Suppliers
                .Where(s => s.BranchId == branchId)
                .CountAsync(),

            ActiveSuppliers = await _context.Suppliers
                .Where(s => s.BranchId == branchId && s.IsActive)
                .CountAsync(),

            GoodsReceiptsThisMonth = await _context.GoodsReceipts
                .Where(g => g.BranchId == branchId && g.ReceiptDate >= firstDayOfMonth)
                .CountAsync(),

            RecentPurchaseOrders = await _context.PurchaseOrders
                .Where(p => p.BranchId == branchId)
                .OrderByDescending(p => p.OrderDate)
                .Take(10)
                .Select(p => new PurchaseOrderDto
                {
                    Id = p.Id,
                    OrderNumber = p.OrderNumber,
                    OrderDate = p.OrderDate,
                    ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                    Status = p.Status,
                    SupplierName = p.Supplier!.Name,
                    SupplierId = p.SupplierId,
                    Total = p.Total,
                    ReceivedAmount = p.ReceivedAmount,
                    Balance = p.Balance,
                    IsFullyReceived = p.IsFullyReceived,
                    SupplierInvoiceNumber = p.SupplierInvoiceNumber
                })
                .ToListAsync(),

            RecentGoodsReceipts = await _context.GoodsReceipts
                .Where(g => g.BranchId == branchId)
                .OrderByDescending(g => g.ReceiptDate)
                .Take(10)
                .Select(g => new GoodsReceiptDto
                {
                    Id = g.Id,
                    ReceiptNumber = g.ReceiptNumber,
                    ReceiptDate = g.ReceiptDate,
                    Status = g.Status,
                    SupplierName = g.Supplier!.Name,
                    PurchaseOrderNumber = g.PurchaseOrder != null ? g.PurchaseOrder.OrderNumber : null,
                    PurchaseOrderId = g.PurchaseOrderId,
                    SupplierInvoiceNumber = g.SupplierInvoiceNumber,
                    ReceivedBy = g.ReceivedBy,
                    ItemCount = g.Items.Count
                })
                .ToListAsync(),

            TopSuppliers = await _context.Suppliers
                .Where(s => s.BranchId == branchId && s.IsActive)
                .OrderBy(s => s.Name)
                .Take(10)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    Mobile = s.Mobile,
                    Address = s.Address,
                    City = s.City,
                    Country = s.Country,
                    Currency = s.Currency,
                    PaymentTermsDays = s.PaymentTermsDays,
                    CreditLimit = s.CreditLimit,
                    IsActive = s.IsActive,
                    TotalPurchaseOrders = s.PurchaseOrders.Count
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // Suppliers
    public async Task<IActionResult> Suppliers()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var suppliers = await _context.Suppliers
            .Where(s => s.BranchId == branchId)
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Mobile = s.Mobile,
                Address = s.Address,
                City = s.City,
                Country = s.Country,
                Currency = s.Currency,
                PaymentTermsDays = s.PaymentTermsDays,
                CreditLimit = s.CreditLimit,
                IsActive = s.IsActive,
                TotalPurchaseOrders = s.PurchaseOrders.Count,
                OutstandingBalance = s.PurchaseOrders
                    .Where(p => p.Status != PurchaseOrderStatus.Cancelled)
                    .Sum(p => p.Balance)
            })
            .ToListAsync();

        return View(suppliers);
    }

    [HttpGet]
    public IActionResult CreateSupplier()
    {
        return View(new CreateSupplierViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(CreateSupplierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var supplier = new Supplier
        {
            Name = model.Name,
            Code = model.Code,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Mobile = model.Mobile,
            Fax = model.Fax,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            PostalCode = model.PostalCode,
            Website = model.Website,
            TaxNumber = model.TaxNumber,
            PaymentTermsDays = model.PaymentTermsDays,
            Currency = model.Currency,
            CreditLimit = model.CreditLimit,
            IsActive = model.IsActive,
            Notes = model.Notes,
            BranchId = branchId,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedDate = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Suppliers));
    }

    // Purchase Orders
    public async Task<IActionResult> PurchaseOrders(PurchaseOrderStatus? status, int? supplierId, DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.PurchaseOrders
            .Where(p => p.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.OrderDate <= toDate.Value);

        var purchaseOrders = await query
            .OrderByDescending(p => p.OrderDate)
            .Select(p => new PurchaseOrderDto
            {
                Id = p.Id,
                OrderNumber = p.OrderNumber,
                OrderDate = p.OrderDate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                Status = p.Status,
                SupplierName = p.Supplier!.Name,
                SupplierId = p.SupplierId,
                Total = p.Total,
                ReceivedAmount = p.ReceivedAmount,
                Balance = p.Balance,
                IsFullyReceived = p.IsFullyReceived,
                SupplierInvoiceNumber = p.SupplierInvoiceNumber
            })
            .ToListAsync();

        await PopulateSuppliersDropdown(branchId);
        return View(purchaseOrders);
    }

    [HttpGet]
    public async Task<IActionResult> CreatePurchaseOrder()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        await PopulateSuppliersDropdown(branchId);
        await PopulateInventoryItemsDropdown(branchId);

        return View(new CreatePurchaseOrderViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseOrder(CreatePurchaseOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user1 = await _userManager.GetUserAsync(User);
            var branchId1 = user1?.PrimaryBranchId ?? 0;
            await PopulateSuppliersDropdown(branchId1);
            await PopulateInventoryItemsDropdown(branchId1);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var orderNumber = await GeneratePurchaseOrderNumber(branchId);

        var purchaseOrder = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            OrderDate = model.OrderDate,
            ExpectedDeliveryDate = model.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            SupplierId = model.SupplierId,
            BranchId = branchId,
            DiscountPercentage = model.DiscountPercentage,
            DiscountAmount = model.DiscountAmount,
            TaxPercentage = model.TaxPercentage,
            ShippingCost = model.ShippingCost,
            Notes = model.Notes,
            Terms = model.Terms,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate totals
        decimal subtotal = 0;
        foreach (var itemModel in model.Items)
        {
            var itemSubtotal = itemModel.OrderedQuantity * itemModel.UnitPrice;
            var itemDiscount = itemModel.DiscountAmount ?? (itemSubtotal * (itemModel.DiscountPercentage ?? 0) / 100);
            var taxableAmount = itemSubtotal - itemDiscount;
            var itemTax = taxableAmount * (itemModel.TaxPercentage ?? 0) / 100;
            var itemTotal = taxableAmount + itemTax;

            var poItem = new PurchaseOrderItem
            {
                InventoryItemId = itemModel.InventoryItemId,
                ItemName = itemModel.ItemName,
                ItemDescription = itemModel.ItemDescription,
                ItemCode = itemModel.ItemCode,
                OrderedQuantity = itemModel.OrderedQuantity,
                UnitPrice = itemModel.UnitPrice,
                DiscountPercentage = itemModel.DiscountPercentage,
                DiscountAmount = itemDiscount,
                TaxPercentage = itemModel.TaxPercentage,
                TaxAmount = itemTax,
                Total = itemTotal,
                Notes = itemModel.Notes
            };

            purchaseOrder.Items.Add(poItem);
            subtotal += itemSubtotal;
        }

        purchaseOrder.SubTotal = subtotal;
        var poDiscount = model.DiscountAmount ?? (subtotal * (model.DiscountPercentage ?? 0) / 100);
        var taxableAmount = subtotal - poDiscount;
        var tax = taxableAmount * (model.TaxPercentage ?? 0) / 100;
        purchaseOrder.DiscountAmount = poDiscount;
        purchaseOrder.TaxAmount = tax;
        purchaseOrder.Total = taxableAmount + tax + (model.ShippingCost ?? 0);

        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PurchaseOrders));
    }

    [HttpPost]
    public async Task<IActionResult> ApprovePurchaseOrder(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var purchaseOrder = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == id && p.BranchId == branchId);

        if (purchaseOrder == null)
            return NotFound();

        if (purchaseOrder.Status != PurchaseOrderStatus.Submitted)
            return BadRequest("Only submitted purchase orders can be approved");

        purchaseOrder.Status = PurchaseOrderStatus.Approved;
        purchaseOrder.ApprovedBy = user?.Id;
        purchaseOrder.ApprovedDate = DateTime.UtcNow;
        purchaseOrder.LastModifiedBy = user?.Id;
        purchaseOrder.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PurchaseOrders));
    }

    // Goods Receipts
    public async Task<IActionResult> GoodsReceipts(GoodsReceiptStatus? status, int? supplierId, DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.GoodsReceipts
            .Where(g => g.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        if (supplierId.HasValue)
            query = query.Where(g => g.SupplierId == supplierId.Value);

        if (fromDate.HasValue)
            query = query.Where(g => g.ReceiptDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(g => g.ReceiptDate <= toDate.Value);

        var goodsReceipts = await query
            .OrderByDescending(g => g.ReceiptDate)
            .Select(g => new GoodsReceiptDto
            {
                Id = g.Id,
                ReceiptNumber = g.ReceiptNumber,
                ReceiptDate = g.ReceiptDate,
                Status = g.Status,
                SupplierName = g.Supplier!.Name,
                PurchaseOrderNumber = g.PurchaseOrder != null ? g.PurchaseOrder.OrderNumber : null,
                PurchaseOrderId = g.PurchaseOrderId,
                SupplierInvoiceNumber = g.SupplierInvoiceNumber,
                ReceivedBy = g.ReceivedBy,
                ItemCount = g.Items.Count
            })
            .ToListAsync();

        await PopulateSuppliersDropdown(branchId);
        return View(goodsReceipts);
    }

    [HttpGet]
    public async Task<IActionResult> CreateGoodsReceipt(int? purchaseOrderId)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var model = new CreateGoodsReceiptViewModel();

        if (purchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId.Value && p.BranchId == branchId);

            if (po != null)
            {
                model.PurchaseOrderId = po.Id;
                model.SupplierId = po.SupplierId;
                model.SupplierInvoiceNumber = po.SupplierInvoiceNumber;
                model.SupplierInvoiceDate = po.SupplierInvoiceDate;

                foreach (var item in po.Items.Where(i => i.RemainingQuantity > 0))
                {
                    model.Items.Add(new GoodsReceiptItemViewModel
                    {
                        PurchaseOrderItemId = item.Id,
                        InventoryItemId = item.InventoryItemId,
                        ItemName = item.ItemName,
                        ItemDescription = item.ItemDescription,
                        ItemCode = item.ItemCode,
                        ReceivedQuantity = item.RemainingQuantity,
                        AcceptedQuantity = item.RemainingQuantity,
                        RejectedQuantity = 0,
                        UnitPrice = item.UnitPrice
                    });
                }
            }
        }

        await PopulateSuppliersDropdown(branchId);
        await PopulatePurchaseOrdersDropdown(branchId);
        await PopulateInventoryItemsDropdown(branchId);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoodsReceipt(CreateGoodsReceiptViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user1 = await _userManager.GetUserAsync(User);
            var branchId1 = user1?.PrimaryBranchId ?? 0;
            await PopulateSuppliersDropdown(branchId1);
            await PopulatePurchaseOrdersDropdown(branchId1);
            await PopulateInventoryItemsDropdown(branchId1);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var receiptNumber = await GenerateGoodsReceiptNumber(branchId);

        var goodsReceipt = new GoodsReceipt
        {
            ReceiptNumber = receiptNumber,
            ReceiptDate = model.ReceiptDate,
            Status = GoodsReceiptStatus.Completed,
            PurchaseOrderId = model.PurchaseOrderId,
            SupplierId = model.SupplierId,
            BranchId = branchId,
            SupplierInvoiceNumber = model.SupplierInvoiceNumber,
            SupplierInvoiceDate = model.SupplierInvoiceDate,
            DeliveryNoteNumber = model.DeliveryNoteNumber,
            ReceivedBy = model.ReceivedBy,
            Notes = model.Notes,
            UpdateInventory = model.UpdateInventory,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemModel in model.Items)
        {
            var grItem = new GoodsReceiptItem
            {
                PurchaseOrderItemId = itemModel.PurchaseOrderItemId,
                InventoryItemId = itemModel.InventoryItemId,
                ItemName = itemModel.ItemName,
                ItemDescription = itemModel.ItemDescription,
                ItemCode = itemModel.ItemCode,
                ReceivedQuantity = itemModel.ReceivedQuantity,
                AcceptedQuantity = itemModel.AcceptedQuantity,
                RejectedQuantity = itemModel.RejectedQuantity,
                UnitPrice = itemModel.UnitPrice,
                BatchNumber = itemModel.BatchNumber,
                ExpiryDate = itemModel.ExpiryDate,
                Notes = itemModel.Notes
            };

            goodsReceipt.Items.Add(grItem);

            // Update inventory if requested
            if (model.UpdateInventory && itemModel.InventoryItemId.HasValue && itemModel.AcceptedQuantity.HasValue)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.Id == itemModel.InventoryItemId.Value);

                if (inventoryItem != null)
                {
                    inventoryItem.QuantityInStock += itemModel.AcceptedQuantity.Value;

                    // Create inventory transaction
                    var transaction = new InventoryTransaction
                    {
                        InventoryItemId = inventoryItem.Id,
                        TransactionType = TransactionType.Purchase,
                        Quantity = itemModel.AcceptedQuantity.Value,
                        UnitPrice = itemModel.UnitPrice ?? 0,
                        TotalAmount = (itemModel.UnitPrice ?? 0) * itemModel.AcceptedQuantity.Value,
                        TransactionDate = model.ReceiptDate,
                        Notes = $"Goods Receipt: {receiptNumber}",
                        CreatedBy = user?.Id ?? string.Empty
                    };

                    _context.InventoryTransactions.Add(transaction);
                }
            }

            // Update purchase order item received quantity
            if (itemModel.PurchaseOrderItemId.HasValue)
            {
                var poItem = await _context.PurchaseOrderItems
                    .FirstOrDefaultAsync(i => i.Id == itemModel.PurchaseOrderItemId.Value);

                if (poItem != null)
                {
                    poItem.ReceivedQuantity += itemModel.ReceivedQuantity;
                }
            }
        }

        // Update purchase order status
        if (model.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == model.PurchaseOrderId.Value);

            if (po != null)
            {
                var allItemsReceived = po.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
                var anyItemsReceived = po.Items.Any(i => i.ReceivedQuantity > 0);

                if (allItemsReceived)
                    po.Status = PurchaseOrderStatus.Received;
                else if (anyItemsReceived)
                    po.Status = PurchaseOrderStatus.PartiallyReceived;

                po.LastModifiedBy = user?.Id;
                po.LastModifiedAt = DateTime.UtcNow;
            }
        }

        _context.GoodsReceipts.Add(goodsReceipt);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(GoodsReceipts));
    }

    // Supplier Payments
    public async Task<IActionResult> SupplierPayments(int? supplierId, DateTime? fromDate, DateTime? toDate)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.SupplierPayments
            .Where(p => p.BranchId == branchId);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value);

        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new SupplierPaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                SupplierName = p.Supplier!.Name,
                SupplierId = p.SupplierId,
                PurchaseOrderNumber = p.PurchaseOrder != null ? p.PurchaseOrder.OrderNumber : null,
                ReferenceNumber = p.ReferenceNumber,
                PaidBy = p.PaidBy
            })
            .ToListAsync();

        await PopulateSuppliersDropdown(branchId);
        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> CreateSupplierPayment(int? purchaseOrderId)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var model = new CreateSupplierPaymentViewModel();

        if (purchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.Id == purchaseOrderId.Value && p.BranchId == branchId);

            if (po != null)
            {
                model.PurchaseOrderId = po.Id;
                model.SupplierId = po.SupplierId;
                model.Amount = po.Balance;
            }
        }

        await PopulateSuppliersDropdown(branchId);
        await PopulatePurchaseOrdersDropdown(branchId);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplierPayment(CreateSupplierPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user1 = await _userManager.GetUserAsync(User);
            var branchId1 = user1?.PrimaryBranchId ?? 0;
            await PopulateSuppliersDropdown(branchId1);
            await PopulatePurchaseOrdersDropdown(branchId1);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var paymentNumber = await GenerateSupplierPaymentNumber(branchId);

        var payment = new SupplierPayment
        {
            PaymentNumber = paymentNumber,
            PaymentDate = model.PaymentDate,
            Amount = model.Amount,
            PaymentMethod = model.PaymentMethod,
            Status = SupplierPaymentStatus.Paid,
            PurchaseOrderId = model.PurchaseOrderId,
            SupplierId = model.SupplierId,
            BranchId = branchId,
            ReferenceNumber = model.ReferenceNumber,
            BankName = model.BankName,
            ChequeNumber = model.ChequeNumber,
            ChequeDate = model.ChequeDate,
            Notes = model.Notes,
            PaidBy = user?.Id ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Update purchase order received amount
        if (model.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(p => p.Id == model.PurchaseOrderId.Value);

            if (po != null)
            {
                po.ReceivedAmount += model.Amount;
                po.LastModifiedBy = user?.Id;
                po.LastModifiedAt = DateTime.UtcNow;
            }
        }

        _context.SupplierPayments.Add(payment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(SupplierPayments));
    }

    // Helper methods
    private async Task<string> GeneratePurchaseOrderNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO-{today:yyyyMM}-";

        var lastOrder = await _context.PurchaseOrders
            .Where(p => p.BranchId == branchId && p.OrderNumber.StartsWith(prefix))
            .OrderByDescending(p => p.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var lastNumberStr = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GenerateGoodsReceiptNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"GR-{today:yyyyMM}-";

        var lastReceipt = await _context.GoodsReceipts
            .Where(g => g.BranchId == branchId && g.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(g => g.ReceiptNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastReceipt != null)
        {
            var lastNumberStr = lastReceipt.ReceiptNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GenerateSupplierPaymentNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"SP-{today:yyyyMM}-";

        var lastPayment = await _context.SupplierPayments
            .Where(p => p.BranchId == branchId && p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPayment != null)
        {
            var lastNumberStr = lastPayment.PaymentNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task PopulateSuppliersDropdown(int branchId)
    {
        var suppliers = await _context.Suppliers
            .Where(s => s.BranchId == branchId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name");
    }

    private async Task PopulatePurchaseOrdersDropdown(int branchId)
    {
        var purchaseOrders = await _context.PurchaseOrders
            .Where(p => p.BranchId == branchId &&
                   (p.Status == PurchaseOrderStatus.Approved ||
                    p.Status == PurchaseOrderStatus.Ordered ||
                    p.Status == PurchaseOrderStatus.PartiallyReceived))
            .OrderByDescending(p => p.OrderDate)
            .Select(p => new { p.Id, p.OrderNumber })
            .ToListAsync();

        ViewBag.PurchaseOrders = new SelectList(purchaseOrders, "Id", "OrderNumber");
    }

    private async Task PopulateInventoryItemsDropdown(int branchId)
    {
        var items = await _context.InventoryItems
            .Where(i => i.BranchId == branchId && i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToListAsync();

        ViewBag.InventoryItems = new SelectList(items, "Id", "Name");
    }
}
