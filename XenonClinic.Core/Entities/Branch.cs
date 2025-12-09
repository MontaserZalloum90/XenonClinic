namespace XenonClinic.Core.Entities;

public class Branch
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public string? Timezone { get; set; } = "Arabian Standard Time";
    public string? Currency { get; set; } = "AED";
    public bool IsActive { get; set; } = true;
    public bool IsMainBranch { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Working hours
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public string? WorkingDays { get; set; } // Comma-separated: "Mon,Tue,Wed,Thu,Fri"

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<AudiologyVisit> Visits { get; set; } = new List<AudiologyVisit>();
    public ICollection<HearingDevice> Devices { get; set; } = new List<HearingDevice>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();

    // Note: PrimaryUsers navigation is defined in Infrastructure (ApplicationUser.PrimaryBranchId)
}
