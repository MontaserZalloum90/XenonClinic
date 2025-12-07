using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<XenonClinicDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        if (!await context.LicenseConfigs.AnyAsync())
        {
            context.LicenseConfigs.Add(new LicenseConfig
            {
                MaxBranches = 3,
                MaxUsers = 20,
                IsActive = true,
                LicenseKey = "XENON-DEMO",
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Branches.AnyAsync())
        {
            context.Branches.AddRange(
                new Branch { Name = "Xenon Audiology – Dubai", Code = "DXB", Address = "Downtown Dubai", Email = "dubai@xenon.ae", Phone = "+971-4-111111", LogoPath = "/images/demo/dubai-logo.png", PrimaryColor = "#5B21B6" },
                new Branch { Name = "Xenon Audiology – Sharjah", Code = "SHJ", Address = "Sharjah Corniche", Email = "shj@xenon.ae", Phone = "+971-6-222222", LogoPath = "/images/demo/sharjah-logo.png", PrimaryColor = "#0EA5E9" },
                new Branch { Name = "Xenon Audiology – Abu Dhabi", Code = "AUH", Address = "Al Maryah Island", Email = "auh@xenon.ae", Phone = "+971-2-333333", LogoPath = "/images/demo/auh-logo.png", PrimaryColor = "#F97316" }
            );
            await context.SaveChangesAsync();
        }

        var roles = new[] { "Admin", "BranchAdmin", "Audiologist", "Receptionist", "Technician" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        async Task<ApplicationUser> EnsureUserAsync(string email, string password, string role, int? primaryBranchId = null)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    PrimaryBranchId = primaryBranchId
                };
                await userManager.CreateAsync(user, password);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            if (primaryBranchId.HasValue && !await context.UserBranches.AnyAsync(ub => ub.UserId == user.Id && ub.BranchId == primaryBranchId.Value))
            {
                context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = primaryBranchId.Value });
                await context.SaveChangesAsync();
            }

            return user;
        }

        var dubaiBranchId = context.Branches.First(b => b.Code == "DXB").Id;
        var sharjahBranchId = context.Branches.First(b => b.Code == "SHJ").Id;
        var auhBranchId = context.Branches.First(b => b.Code == "AUH").Id;

        await EnsureUserAsync("admin@xenon.local", "Admin@123!", "Admin");
        await EnsureUserAsync("dubai.admin@xenon.local", "Admin@123!", "BranchAdmin", dubaiBranchId);
        await EnsureUserAsync("audiologist1@xenon.local", "Admin@123!", "Audiologist", dubaiBranchId);
        await EnsureUserAsync("reception1@xenon.local", "Admin@123!", "Receptionist", sharjahBranchId);

        if (!await context.Patients.AnyAsync())
        {
            var random = new Random(42);
            var namesEn = new[] { "Sara Al Mansouri", "Huda Al Qassimi", "Omar Al Falahi", "Mariam Al Zaabi", "Yousef Al Neyadi", "Noura Al Shamsi", "Ahmed Al Mansoori", "Fatima Al Suwaidi", "Ali Al Nuaimi", "Laila Al Mazrouei" };
            var namesAr = new[] { "سارة المنصوري", "هدى القاسمي", "عمر الفلاحي", "مريم الزعابي", "يوسف النيادي", "نورة الشامسي", "أحمد المنصوري", "فاطمة السويدي", "علي النعيمي", "ليلى المزروعي" };
            var lossTypes = new[] { "Sensorineural", "Conductive", "Mixed", "Normal" };
            var branches = new[] { dubaiBranchId, sharjahBranchId, auhBranchId };
            var patients = new List<Patient>();

            for (int i = 0; i < 40; i++)
            {
                var idx = random.Next(namesEn.Length);
                patients.Add(new Patient
                {
                    BranchId = branches[random.Next(branches.Length)],
                    EmiratesId = $"784-{random.Next(1000000, 9999999)}-{random.Next(1, 9)}",
                    FullNameEn = namesEn[idx],
                    FullNameAr = namesAr[idx],
                    DateOfBirth = DateTime.UtcNow.AddYears(-random.Next(8, 75)).Date,
                    Gender = random.NextDouble() > 0.5 ? "M" : "F",
                    PhoneNumber = $"+9715{random.Next(10000000, 99999999)}",
                    Email = $"patient{i}@xenon.demo",
                    HearingLossType = lossTypes[random.Next(lossTypes.Length)],
                    Notes = "Demo seeded patient"
                });
            }

            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();

            var appointments = new List<Appointment>();
            var invoices = new List<Invoice>();
            foreach (var patient in patients)
            {
                for (int j = 0; j < 3; j++)
                {
                    var start = DateTime.UtcNow.AddDays(-random.Next(1, 60)).AddHours(random.Next(8, 18));
                    var status = new[] { "Booked", "Completed", "Cancelled", "NoShow" }[random.Next(4)];
                    var type = new[] { "HearingTest", "Fitting", "Consultation", "Repair", "FollowUp" }[random.Next(5)];
                    appointments.Add(new Appointment
                    {
                        BranchId = patient.BranchId,
                        PatientId = patient.Id,
                        StartTime = start,
                        EndTime = start.AddMinutes(45),
                        Status = status,
                        Type = type,
                        Notes = "Seeded appointment"
                    });
                }

                invoices.Add(new Invoice
                {
                    BranchId = patient.BranchId,
                    PatientId = patient.Id,
                    InvoiceDate = DateTime.UtcNow.AddDays(-random.Next(90)).Date,
                    TotalAmount = random.Next(300, 8000),
                    PaymentStatus = new[] { "Paid", "Pending", "Cancelled" }[random.Next(3)],
                    Notes = "Seeded invoice"
                });
            }

            context.Appointments.AddRange(appointments);
            context.Invoices.AddRange(invoices);
            await context.SaveChangesAsync();

            var visits = new List<AudiologyVisit>();
            var audiograms = new List<Audiogram>();
            var devices = new List<HearingDevice>();
            foreach (var patient in patients.Take(30))
            {
                var visit = new AudiologyVisit
                {
                    BranchId = patient.BranchId,
                    PatientId = patient.Id,
                    VisitDate = DateTime.UtcNow.AddDays(-random.Next(60)),
                    ChiefComplaint = "Difficulty hearing in noisy places",
                    Diagnosis = "Bilateral sensorineural hearing loss",
                    Plan = "Recommend binaural hearing aids and follow up"
                };
                visits.Add(visit);
                audiograms.Add(new Audiogram
                {
                    RawDataJson = "{\"500\":30,\"1000\":35,\"2000\":40,\"4000\":45}",
                    Notes = "Seeded audiogram",
                    Visit = visit
                });

                devices.Add(new HearingDevice
                {
                    BranchId = patient.BranchId,
                    PatientId = patient.Id,
                    ModelName = new[] { "Phonak Audeo L90", "Oticon More 1", "Signia Pure Charge&Go" }[random.Next(3)],
                    SerialNumber = $"SN-{random.Next(100000, 999999)}",
                    PurchaseDate = DateTime.UtcNow.AddDays(-random.Next(120)),
                    WarrantyExpiryDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true,
                    Notes = "Demo device"
                });
            }

            context.AudiologyVisits.AddRange(visits);
            context.Audiograms.AddRange(audiograms);
            context.HearingDevices.AddRange(devices);
            await context.SaveChangesAsync();
        }
    }
}
