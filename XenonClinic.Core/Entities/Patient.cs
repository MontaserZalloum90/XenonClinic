using System;
using System.Collections.Generic;

namespace XenonClinic.Core.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        public int BranchId { get; set; }

        public string EmiratesId { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public string? FullNameAr { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "M";
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? HearingLossType { get; set; }
        public string? Notes { get; set; }

        public Branch? Branch { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<AudiologyVisit> Visits { get; set; } = new List<AudiologyVisit>();
        public ICollection<HearingDevice> Devices { get; set; } = new List<HearingDevice>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
