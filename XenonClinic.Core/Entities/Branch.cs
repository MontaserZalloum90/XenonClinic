using System.Collections.Generic;

namespace XenonClinic.Core.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoPath { get; set; }
        public string PrimaryColor { get; set; } = "#1F6FEB";
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ApplicationUser> PrimaryUsers { get; set; } = new List<ApplicationUser>();
        public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<AudiologyVisit> Visits { get; set; } = new List<AudiologyVisit>();
        public ICollection<HearingDevice> Devices { get; set; } = new List<HearingDevice>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
