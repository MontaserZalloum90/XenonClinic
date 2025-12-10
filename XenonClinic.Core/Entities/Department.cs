namespace XenonClinic.Core.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BranchId { get; set; }
    public int? ManagerId { get; set; } // Employee who manages this department
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public Employee? Manager { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
