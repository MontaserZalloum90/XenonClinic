namespace XenonClinic.Web.Models;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public bool IsActive { get; set; }
    public int ExpenseCount { get; set; }
}
