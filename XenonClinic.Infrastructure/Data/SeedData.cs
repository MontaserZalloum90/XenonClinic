using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        // Seed License Config
        if (!await context.LicenseConfigs.AnyAsync())
        {
            context.LicenseConfigs.Add(new LicenseConfig
            {
                MaxBranches = 10,
                MaxUsers = 100,
                IsActive = true,
                LicenseKey = "XENON-ENTERPRISE",
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            });
            await context.SaveChangesAsync();
        }

        // Seed Roles - including new multi-tenancy roles
        foreach (var role in RoleConstants.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Lookup Values (system-wide defaults)
        await LookupSeeder.SeedLookupsAsync(context);

        // Seed Tenants
        if (!await context.Tenants.AnyAsync())
        {
            var tenant1 = new Tenant
            {
                Name = "Xenon Healthcare Group",
                Code = "XENON",
                Description = "Leading healthcare provider in the UAE",
                ContactEmail = "info@xenonhealthcare.ae",
                ContactPhone = "+971-4-1234567",
                Address = "Dubai Healthcare City, Dubai, UAE",
                PrimaryColor = "#5B21B6",
                SecondaryColor = "#7C3AED",
                IsActive = true,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddYears(1),
                SubscriptionPlan = "Enterprise",
                MaxCompanies = 5,
                MaxBranchesPerCompany = 10,
                MaxUsersPerTenant = 100,
                CreatedAt = DateTime.UtcNow
            };

            var tenant2 = new Tenant
            {
                Name = "Gulf Medical Services",
                Code = "GMS",
                Description = "Healthcare services across the GCC",
                ContactEmail = "admin@gulfmedical.ae",
                ContactPhone = "+971-2-7654321",
                Address = "Al Maryah Island, Abu Dhabi, UAE",
                PrimaryColor = "#0EA5E9",
                SecondaryColor = "#38BDF8",
                IsActive = true,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddYears(1),
                SubscriptionPlan = "Professional",
                MaxCompanies = 3,
                MaxBranchesPerCompany = 5,
                MaxUsersPerTenant = 50,
                CreatedAt = DateTime.UtcNow
            };

            context.Tenants.AddRange(tenant1, tenant2);
            await context.SaveChangesAsync();

            // Add tenant settings
            context.TenantSettings.AddRange(
                new TenantSettings
                {
                    TenantId = tenant1.Id,
                    DefaultLanguage = "en",
                    DefaultCurrency = "AED",
                    DefaultTimezone = "Arabian Standard Time",
                    DefaultTaxRate = 5,
                    EnableLabModule = true,
                    EnableInventoryModule = true,
                    EnableHRModule = true,
                    EnableFinanceModule = true,
                    EnableProcurementModule = true,
                    EnableSalesModule = true,
                    EnableAnalyticsModule = true
                },
                new TenantSettings
                {
                    TenantId = tenant2.Id,
                    DefaultLanguage = "en",
                    DefaultCurrency = "AED",
                    DefaultTimezone = "Arabian Standard Time",
                    DefaultTaxRate = 5,
                    EnableLabModule = true,
                    EnableInventoryModule = true,
                    EnableHRModule = true,
                    EnableFinanceModule = true
                }
            );
            await context.SaveChangesAsync();
        }

        var xenonTenant = await context.Tenants.FirstAsync(t => t.Code == "XENON");
        var gmsTenant = await context.Tenants.FirstAsync(t => t.Code == "GMS");

        // Seed Companies
        if (!await context.Companies.AnyAsync())
        {
            var company1 = new Company
            {
                TenantId = xenonTenant.Id,
                Name = "Xenon Audiology Centers",
                Code = "XAC",
                TradeLicenseNumber = "TL-2024-001234",
                TaxRegistrationNumber = "TRN-300123456789",
                Description = "Specialized audiology and hearing care centers",
                ContactEmail = "audiology@xenonhealthcare.ae",
                ContactPhone = "+971-4-1111111",
                Address = "Downtown Dubai",
                City = "Dubai",
                Country = "UAE",
                Currency = "AED",
                PrimaryColor = "#5B21B6",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var company2 = new Company
            {
                TenantId = xenonTenant.Id,
                Name = "Xenon ENT Clinics",
                Code = "XEC",
                TradeLicenseNumber = "TL-2024-001235",
                TaxRegistrationNumber = "TRN-300123456790",
                Description = "Ear, Nose, and Throat specialty clinics",
                ContactEmail = "ent@xenonhealthcare.ae",
                ContactPhone = "+971-4-2222222",
                Address = "Business Bay",
                City = "Dubai",
                Country = "UAE",
                Currency = "AED",
                PrimaryColor = "#10B981",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var company3 = new Company
            {
                TenantId = gmsTenant.Id,
                Name = "Gulf Hearing Solutions",
                Code = "GHS",
                TradeLicenseNumber = "TL-2024-002001",
                TaxRegistrationNumber = "TRN-300987654321",
                Description = "Premium hearing solutions provider",
                ContactEmail = "hearing@gulfmedical.ae",
                ContactPhone = "+971-2-3333333",
                Address = "Corniche Road",
                City = "Abu Dhabi",
                Country = "UAE",
                Currency = "AED",
                PrimaryColor = "#0EA5E9",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Companies.AddRange(company1, company2, company3);
            await context.SaveChangesAsync();
        }

        var xenonAudiology = await context.Companies.FirstAsync(c => c.Code == "XAC");
        var xenonENT = await context.Companies.FirstAsync(c => c.Code == "XEC");
        var gulfHearing = await context.Companies.FirstAsync(c => c.Code == "GHS");

        // Seed Branches (linked to companies)
        if (!await context.Branches.AnyAsync())
        {
            context.Branches.AddRange(
                // Xenon Audiology Centers branches
                new Branch
                {
                    CompanyId = xenonAudiology.Id,
                    Name = "Xenon Audiology – Dubai",
                    Code = "DXB",
                    Address = "Downtown Dubai",
                    City = "Dubai",
                    Email = "dubai@xenon.ae",
                    Phone = "+971-4-111111",
                    LogoPath = "/images/demo/dubai-logo.png",
                    PrimaryColor = "#5B21B6",
                    IsMainBranch = true,
                    IsActive = true,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    WorkingDays = "Mon,Tue,Wed,Thu,Fri,Sat",
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    CompanyId = xenonAudiology.Id,
                    Name = "Xenon Audiology – Sharjah",
                    Code = "SHJ",
                    Address = "Sharjah Corniche",
                    City = "Sharjah",
                    Email = "shj@xenon.ae",
                    Phone = "+971-6-222222",
                    LogoPath = "/images/demo/sharjah-logo.png",
                    PrimaryColor = "#0EA5E9",
                    IsMainBranch = false,
                    IsActive = true,
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(21, 0, 0),
                    WorkingDays = "Mon,Tue,Wed,Thu,Fri,Sat",
                    CreatedAt = DateTime.UtcNow
                },
                new Branch
                {
                    CompanyId = xenonAudiology.Id,
                    Name = "Xenon Audiology – Abu Dhabi",
                    Code = "AUH",
                    Address = "Al Maryah Island",
                    City = "Abu Dhabi",
                    Email = "auh@xenon.ae",
                    Phone = "+971-2-333333",
                    LogoPath = "/images/demo/auh-logo.png",
                    PrimaryColor = "#F97316",
                    IsMainBranch = false,
                    IsActive = true,
                    OpeningTime = new TimeSpan(8, 30, 0),
                    ClosingTime = new TimeSpan(20, 30, 0),
                    WorkingDays = "Mon,Tue,Wed,Thu,Fri,Sat",
                    CreatedAt = DateTime.UtcNow
                },
                // Xenon ENT branches
                new Branch
                {
                    CompanyId = xenonENT.Id,
                    Name = "Xenon ENT – Business Bay",
                    Code = "ENT-BB",
                    Address = "Business Bay Tower",
                    City = "Dubai",
                    Email = "ent-bb@xenon.ae",
                    Phone = "+971-4-444444",
                    PrimaryColor = "#10B981",
                    IsMainBranch = true,
                    IsActive = true,
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(18, 0, 0),
                    WorkingDays = "Mon,Tue,Wed,Thu,Fri",
                    CreatedAt = DateTime.UtcNow
                },
                // Gulf Hearing Solutions branches
                new Branch
                {
                    CompanyId = gulfHearing.Id,
                    Name = "Gulf Hearing – Abu Dhabi",
                    Code = "GH-AUH",
                    Address = "Corniche Road",
                    City = "Abu Dhabi",
                    Email = "auh@gulfhearing.ae",
                    Phone = "+971-2-555555",
                    PrimaryColor = "#0EA5E9",
                    IsMainBranch = true,
                    IsActive = true,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(19, 0, 0),
                    WorkingDays = "Sun,Mon,Tue,Wed,Thu",
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }

        // Get branch IDs
        var dubaiBranch = await context.Branches.FirstAsync(b => b.Code == "DXB");
        var sharjahBranch = await context.Branches.FirstAsync(b => b.Code == "SHJ");
        var auhBranch = await context.Branches.FirstAsync(b => b.Code == "AUH");
        var entBranch = await context.Branches.FirstAsync(b => b.Code == "ENT-BB");
        var gulfBranch = await context.Branches.FirstAsync(b => b.Code == "GH-AUH");

        // Helper function to ensure user exists with multi-tenancy assignments
        async Task<ApplicationUser> EnsureUserAsync(
            string email,
            string password,
            string role,
            int? tenantId = null,
            int? companyId = null,
            int? primaryBranchId = null,
            string? firstName = null,
            string? lastName = null,
            bool isSuperAdmin = false,
            List<int>? additionalBranchIds = null)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    TenantId = tenantId,
                    CompanyId = companyId,
                    PrimaryBranchId = primaryBranchId,
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = firstName != null && lastName != null ? $"{firstName} {lastName}" : null,
                    IsSuperAdmin = isSuperAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, password);
            }
            else
            {
                // Update existing user with multi-tenancy info
                user.TenantId = tenantId;
                user.CompanyId = companyId;
                user.PrimaryBranchId = primaryBranchId;
                user.IsSuperAdmin = isSuperAdmin;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            // Add primary branch assignment
            if (primaryBranchId.HasValue && !await context.UserBranches.AnyAsync(ub => ub.UserId == user.Id && ub.BranchId == primaryBranchId.Value))
            {
                context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = primaryBranchId.Value });
                await context.SaveChangesAsync();
            }

            // Add additional branch assignments
            if (additionalBranchIds != null)
            {
                foreach (var branchId in additionalBranchIds)
                {
                    if (!await context.UserBranches.AnyAsync(ub => ub.UserId == user.Id && ub.BranchId == branchId))
                    {
                        context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = branchId });
                    }
                }
                await context.SaveChangesAsync();
            }

            return user;
        }

        // Create Super Admin (system-wide)
        await EnsureUserAsync(
            "superadmin@xenon.local",
            "SuperAdmin@123!",
            "SuperAdmin",
            firstName: "System",
            lastName: "Administrator",
            isSuperAdmin: true
        );

        // Create Tenant Admins
        await EnsureUserAsync(
            "admin@xenon.local",
            "Admin@123!",
            "TenantAdmin",
            tenantId: xenonTenant.Id,
            firstName: "Xenon",
            lastName: "Admin"
        );

        await EnsureUserAsync(
            "admin@gulfmedical.local",
            "Admin@123!",
            "TenantAdmin",
            tenantId: gmsTenant.Id,
            firstName: "Gulf",
            lastName: "Admin"
        );

        // Create Company Admins
        await EnsureUserAsync(
            "audiology.admin@xenon.local",
            "Admin@123!",
            "CompanyAdmin",
            tenantId: xenonTenant.Id,
            companyId: xenonAudiology.Id,
            firstName: "Audiology",
            lastName: "Admin"
        );

        // Create Branch Admins and Staff
        await EnsureUserAsync(
            "dubai.admin@xenon.local",
            "Admin@123!",
            "BranchAdmin",
            tenantId: xenonTenant.Id,
            companyId: xenonAudiology.Id,
            primaryBranchId: dubaiBranch.Id,
            firstName: "Dubai",
            lastName: "Manager"
        );

        await EnsureUserAsync(
            "audiologist1@xenon.local",
            "Staff@123!",
            "Audiologist",
            tenantId: xenonTenant.Id,
            companyId: xenonAudiology.Id,
            primaryBranchId: dubaiBranch.Id,
            firstName: "Ahmed",
            lastName: "Khalid",
            additionalBranchIds: new List<int> { sharjahBranch.Id }
        );

        await EnsureUserAsync(
            "reception1@xenon.local",
            "Staff@123!",
            "Receptionist",
            tenantId: xenonTenant.Id,
            companyId: xenonAudiology.Id,
            primaryBranchId: sharjahBranch.Id,
            firstName: "Fatima",
            lastName: "Al-Hassan"
        );

        await EnsureUserAsync(
            "technician1@xenon.local",
            "Staff@123!",
            "Technician",
            tenantId: xenonTenant.Id,
            companyId: xenonAudiology.Id,
            primaryBranchId: auhBranch.Id,
            firstName: "Omar",
            lastName: "Said"
        );

        // Gulf Hearing Solutions staff
        await EnsureUserAsync(
            "gulf.audiologist@gulfmedical.local",
            "Staff@123!",
            "Audiologist",
            tenantId: gmsTenant.Id,
            companyId: gulfHearing.Id,
            primaryBranchId: gulfBranch.Id,
            firstName: "Sara",
            lastName: "Al-Mansouri"
        );

        // Seed Sample Data (Patients, Appointments, etc.)
        if (!await context.Patients.AnyAsync())
        {
            var random = new Random(42);
            var namesEn = new[] { "Sara Al Mansouri", "Huda Al Qassimi", "Omar Al Falahi", "Mariam Al Zaabi", "Yousef Al Neyadi", "Noura Al Shamsi", "Ahmed Al Mansoori", "Fatima Al Suwaidi", "Ali Al Nuaimi", "Laila Al Mazrouei" };
            var namesAr = new[] { "سارة المنصوري", "هدى القاسمي", "عمر الفلاحي", "مريم الزعابي", "يوسف النيادي", "نورة الشامسي", "أحمد المنصوري", "فاطمة السويدي", "علي النعيمي", "ليلى المزروعي" };
            var lossTypes = new[] { "Sensorineural", "Conductive", "Mixed", "Normal" };
            var branches = new[] { dubaiBranch.Id, sharjahBranch.Id, auhBranch.Id, gulfBranch.Id };
            var patients = new List<Patient>();

            for (int i = 0; i < 50; i++)
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
                    var status = new[] { AppointmentStatus.Confirmed, AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow }[random.Next(4)];
                    var type = new[] { Core.Enums.AppointmentType.HearingTest, Core.Enums.AppointmentType.HearingAidFitting, Core.Enums.AppointmentType.Consultation, Core.Enums.AppointmentType.HearingDeviceRepair, Core.Enums.AppointmentType.FollowUp }[random.Next(5)];
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
            foreach (var patient in patients.Take(35))
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
                    PatientId = patient.Id,
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
