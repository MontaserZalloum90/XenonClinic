using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class ExpenseCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? AccountId { get; set; }
    public Account? Account { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
