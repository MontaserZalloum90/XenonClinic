using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Seeds company types, clinic types, features, and templates
/// </summary>
public static class MultiTenantSeeder
{
    public static async Task SeedAsync(ClinicDbContext context)
    {
        await SeedCompanyTypesAsync(context);
        await SeedClinicTypesAsync(context);
        await SeedFeaturesAsync(context);
        await SeedCompanyTypeTemplatesAsync(context);
        await SeedClinicTypeTemplatesAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCompanyTypesAsync(ClinicDbContext context)
    {
        if (await context.CompanyTypes.AnyAsync()) return;

        var types = new[]
        {
            new CompanyType { Code = "CLINIC", Name = "Healthcare Clinic", Description = "Medical clinic or healthcare facility", IconName = "Stethoscope", SortOrder = 0 },
            new CompanyType { Code = "TRADING", Name = "Trading Company", Description = "Sales and trading company", IconName = "Building", SortOrder = 1 }
        };

        context.CompanyTypes.AddRange(types);
    }

    private static async Task SeedClinicTypesAsync(ClinicDbContext context)
    {
        if (await context.ClinicTypes.AnyAsync()) return;

        var types = new[]
        {
            new ClinicType { Code = "GENERAL", Name = "General Practice", Description = "General medical practice", IconName = "Stethoscope", SortOrder = 0 },
            new ClinicType { Code = "AUDIOLOGY", Name = "Audiology Clinic", Description = "Hearing and audiology services", IconName = "Ear", SortOrder = 1 },
            new ClinicType { Code = "DENTAL", Name = "Dental Clinic", Description = "Dental and oral health services", IconName = "Smile", SortOrder = 2 },
            new ClinicType { Code = "VET", Name = "Veterinary Clinic", Description = "Animal healthcare services", IconName = "PawPrint", SortOrder = 3 },
            new ClinicType { Code = "DERMATOLOGY", Name = "Dermatology Clinic", Description = "Skin care and dermatology", IconName = "Sparkles", SortOrder = 4 },
            new ClinicType { Code = "OPHTHALMOLOGY", Name = "Ophthalmology Clinic", Description = "Eye care and vision services", IconName = "Eye", SortOrder = 5 }
        };

        context.ClinicTypes.AddRange(types);
    }

    private static async Task SeedFeaturesAsync(ClinicDbContext context)
    {
        if (await context.Features.AnyAsync()) return;

        var features = new[]
        {
            // Core features
            new Feature { Code = "dashboard", Name = "Dashboard", Category = "core", IconName = "LayoutDashboard", DefaultRoute = "/", SortOrder = 0 },
            new Feature { Code = "patients", Name = "Patients", Category = "core", IconName = "Users", DefaultRoute = "/patients", SortOrder = 10 },
            new Feature { Code = "appointments", Name = "Appointments", Category = "core", IconName = "Calendar", DefaultRoute = "/appointments", SortOrder = 20 },
            new Feature { Code = "visits", Name = "Visits", Category = "core", IconName = "ClipboardList", DefaultRoute = "/visits", SortOrder = 30 },
            new Feature { Code = "billing", Name = "Billing", Category = "core", IconName = "Receipt", DefaultRoute = "/billing", SortOrder = 40 },
            new Feature { Code = "payments", Name = "Payments", Category = "core", IconName = "CreditCard", DefaultRoute = "/payments", SortOrder = 50 },
            new Feature { Code = "inventory", Name = "Inventory", Category = "core", IconName = "Package", DefaultRoute = "/inventory", SortOrder = 60 },
            new Feature { Code = "laboratory", Name = "Laboratory", Category = "core", IconName = "FlaskConical", DefaultRoute = "/laboratory", SortOrder = 70 },
            new Feature { Code = "reports", Name = "Reports", Category = "core", IconName = "BarChart", DefaultRoute = "/reports", SortOrder = 80 },
            new Feature { Code = "hr", Name = "Human Resources", Category = "core", IconName = "UserCog", DefaultRoute = "/hr", SortOrder = 90 },
            new Feature { Code = "settings", Name = "Settings", Category = "admin", IconName = "Settings", DefaultRoute = "/settings", SortOrder = 100 },

            // Trading features
            new Feature { Code = "customers", Name = "Customers", Category = "trading", IconName = "Users", DefaultRoute = "/customers", SortOrder = 10 },
            new Feature { Code = "leads", Name = "Leads", Category = "trading", IconName = "Target", DefaultRoute = "/leads", SortOrder = 15 },
            new Feature { Code = "products", Name = "Products", Category = "trading", IconName = "Package", DefaultRoute = "/products", SortOrder = 20 },
            new Feature { Code = "orders", Name = "Orders", Category = "trading", IconName = "ShoppingCart", DefaultRoute = "/orders", SortOrder = 25 },
            new Feature { Code = "quotations", Name = "Quotations", Category = "trading", IconName = "FileText", DefaultRoute = "/quotations", SortOrder = 30 },
            new Feature { Code = "salesPipeline", Name = "Sales Pipeline", Category = "trading", IconName = "TrendingUp", DefaultRoute = "/pipeline", SortOrder = 35 },

            // Audiology features
            new Feature { Code = "audiogram", Name = "Audiograms", Category = "clinical", IconName = "Activity", DefaultRoute = "/audiology/audiograms", SortOrder = 100 },
            new Feature { Code = "hearingDevices", Name = "Hearing Devices", Category = "clinical", IconName = "Headphones", DefaultRoute = "/audiology/devices", SortOrder = 101 },
            new Feature { Code = "fittingSessions", Name = "Fitting Sessions", Category = "clinical", IconName = "Wrench", DefaultRoute = "/audiology/fittings", SortOrder = 102 },
            new Feature { Code = "speechAudiometry", Name = "Speech Audiometry", Category = "clinical", IconName = "Mic", DefaultRoute = "/audiology/speech", SortOrder = 103 },

            // Dental features
            new Feature { Code = "toothChart", Name = "Tooth Chart", Category = "clinical", IconName = "Grid3x3", DefaultRoute = "/dental/chart", SortOrder = 110 },
            new Feature { Code = "treatmentPlan", Name = "Treatment Plans", Category = "clinical", IconName = "ListChecks", DefaultRoute = "/dental/plans", SortOrder = 111 },
            new Feature { Code = "dentalProcedures", Name = "Dental Procedures", Category = "clinical", IconName = "Stethoscope", DefaultRoute = "/dental/procedures", SortOrder = 112 },
            new Feature { Code = "xrays", Name = "X-Rays", Category = "clinical", IconName = "Scan", DefaultRoute = "/dental/xrays", SortOrder = 113 },
            new Feature { Code = "periodontalCharting", Name = "Periodontal Charting", Category = "clinical", IconName = "LineChart", DefaultRoute = "/dental/perio", SortOrder = 114 },

            // Vet features
            new Feature { Code = "petManagement", Name = "Pet Management", Category = "clinical", IconName = "PawPrint", DefaultRoute = "/pets", SortOrder = 120 },
            new Feature { Code = "vaccinations", Name = "Vaccinations", Category = "clinical", IconName = "Syringe", DefaultRoute = "/vaccinations", SortOrder = 121 },
            new Feature { Code = "petOwners", Name = "Pet Owners", Category = "clinical", IconName = "Users", DefaultRoute = "/owners", SortOrder = 122 },
            new Feature { Code = "grooming", Name = "Grooming", Category = "clinical", IconName = "Scissors", DefaultRoute = "/grooming", SortOrder = 123 },
            new Feature { Code = "boarding", Name = "Boarding", Category = "clinical", IconName = "Home", DefaultRoute = "/boarding", SortOrder = 124 }
        };

        context.Features.AddRange(features);
    }

    private static async Task SeedCompanyTypeTemplatesAsync(ClinicDbContext context)
    {
        if (await context.CompanyTypeTemplates.AnyAsync()) return;

        // CLINIC base template
        var clinicTemplate = new CompanyTypeTemplate
        {
            CompanyTypeCode = "CLINIC",
            IsDefault = true,
            FeaturesJson = @"[""dashboard"",""patients"",""appointments"",""visits"",""billing"",""payments"",""inventory"",""laboratory"",""reports"",""hr"",""settings""]",
            TerminologyJson = GetClinicTerminologyJson(),
            NavigationJson = GetClinicNavigationJson(),
            UISchemasJson = GetClinicUISchemasJson(),
            FormLayoutsJson = GetClinicFormLayoutsJson(),
            ListLayoutsJson = GetClinicListLayoutsJson()
        };

        // TRADING template
        var tradingTemplate = new CompanyTypeTemplate
        {
            CompanyTypeCode = "TRADING",
            IsDefault = true,
            FeaturesJson = @"[""dashboard"",""customers"",""leads"",""products"",""orders"",""quotations"",""inventory"",""billing"",""payments"",""reports"",""hr"",""settings""]",
            TerminologyJson = GetTradingTerminologyJson(),
            NavigationJson = GetTradingNavigationJson(),
            UISchemasJson = GetTradingUISchemasJson(),
            FormLayoutsJson = GetTradingFormLayoutsJson(),
            ListLayoutsJson = GetTradingListLayoutsJson()
        };

        context.CompanyTypeTemplates.AddRange(clinicTemplate, tradingTemplate);
    }

    private static async Task SeedClinicTypeTemplatesAsync(ClinicDbContext context)
    {
        if (await context.ClinicTypeTemplates.AnyAsync()) return;

        var audiologyTemplate = new ClinicTypeTemplate
        {
            ClinicTypeCode = "AUDIOLOGY",
            IsDefault = true,
            FeaturesJson = @"[""audiogram"",""hearingDevices"",""fittingSessions"",""speechAudiometry""]",
            TerminologyJson = GetAudiologyTerminologyJson(),
            NavigationJson = GetAudiologyNavigationJson(),
            UISchemasJson = GetAudiologyUISchemasJson(),
            FormLayoutsJson = GetAudiologyFormLayoutsJson()
        };

        var dentalTemplate = new ClinicTypeTemplate
        {
            ClinicTypeCode = "DENTAL",
            IsDefault = true,
            FeaturesJson = @"[""toothChart"",""treatmentPlan"",""dentalProcedures"",""xrays"",""periodontalCharting""]",
            TerminologyJson = GetDentalTerminologyJson(),
            NavigationJson = GetDentalNavigationJson(),
            UISchemasJson = GetDentalUISchemasJson()
        };

        var vetTemplate = new ClinicTypeTemplate
        {
            ClinicTypeCode = "VET",
            IsDefault = true,
            FeaturesJson = @"[""petManagement"",""vaccinations"",""petOwners"",""grooming"",""boarding""]",
            TerminologyJson = GetVetTerminologyJson(),
            NavigationJson = GetVetNavigationJson(),
            UISchemasJson = GetVetUISchemasJson(),
            FormLayoutsJson = GetVetFormLayoutsJson()
        };

        context.ClinicTypeTemplates.AddRange(audiologyTemplate, dentalTemplate, vetTemplate);
    }

    #region Clinic Templates

    private static string GetClinicTerminologyJson() => @"{
  ""entity.patient.singular"": ""Patient"",
  ""entity.patient.plural"": ""Patients"",
  ""entity.visit.singular"": ""Visit"",
  ""entity.visit.plural"": ""Visits"",
  ""entity.appointment.singular"": ""Appointment"",
  ""entity.appointment.plural"": ""Appointments"",
  ""entity.invoice.singular"": ""Invoice"",
  ""entity.invoice.plural"": ""Invoices"",
  ""entity.encounter.singular"": ""Encounter"",
  ""entity.encounter.plural"": ""Encounters"",
  ""role.doctor"": ""Doctor"",
  ""role.nurse"": ""Nurse"",
  ""role.receptionist"": ""Receptionist"",
  ""role.labTechnician"": ""Lab Technician"",
  ""nav.patients"": ""Patients"",
  ""nav.appointments"": ""Appointments"",
  ""nav.visits"": ""Visits"",
  ""nav.billing"": ""Billing"",
  ""nav.laboratory"": ""Laboratory"",
  ""nav.inventory"": ""Inventory"",
  ""nav.reports"": ""Reports"",
  ""nav.hr"": ""HR"",
  ""nav.settings"": ""Settings"",
  ""page.patients.title"": ""Patient Management"",
  ""page.patients.subtitle"": ""Manage patient records and medical history"",
  ""page.patients.addNew"": ""New Patient"",
  ""page.patients.empty"": ""No patients found"",
  ""page.appointments.title"": ""Appointment Schedule"",
  ""page.visits.title"": ""Patient Visits"",
  ""form.patient.firstName"": ""First Name"",
  ""form.patient.lastName"": ""Last Name"",
  ""form.patient.dateOfBirth"": ""Date of Birth"",
  ""form.patient.gender"": ""Gender"",
  ""form.patient.phone"": ""Phone"",
  ""form.patient.email"": ""Email"",
  ""form.patient.emiratesId"": ""Emirates ID"",
  ""form.patient.passport"": ""Passport Number"",
  ""form.patient.nationality"": ""Nationality"",
  ""form.patient.address"": ""Address"",
  ""form.patient.bloodType"": ""Blood Type"",
  ""form.patient.allergies"": ""Allergies"",
  ""form.patient.medicalHistory"": ""Medical History"",
  ""action.addPatient"": ""Add Patient"",
  ""action.editPatient"": ""Edit Patient"",
  ""action.viewHistory"": ""View History"",
  ""action.newAppointment"": ""New Appointment"",
  ""section.basicInfo"": ""Basic Information"",
  ""section.contactInfo"": ""Contact Information"",
  ""section.medicalInfo"": ""Medical Information"",
  ""section.emergencyInfo"": ""Emergency Contact"",
  ""section.identification"": ""Identification""
}";

    private static string GetClinicNavigationJson() => @"[
  {""id"":""dashboard"",""label"":""nav.dashboard"",""icon"":""LayoutDashboard"",""route"":""/"",""featureCode"":""dashboard"",""sortOrder"":0},
  {""id"":""patients"",""label"":""nav.patients"",""icon"":""Users"",""route"":""/patients"",""featureCode"":""patients"",""sortOrder"":10},
  {""id"":""appointments"",""label"":""nav.appointments"",""icon"":""Calendar"",""route"":""/appointments"",""featureCode"":""appointments"",""sortOrder"":20,""badge"":{""type"":""count"",""countKey"":""todayAppointments""}},
  {""id"":""visits"",""label"":""nav.visits"",""icon"":""ClipboardList"",""route"":""/visits"",""featureCode"":""visits"",""sortOrder"":30},
  {""id"":""laboratory"",""label"":""nav.laboratory"",""icon"":""FlaskConical"",""route"":""/laboratory"",""featureCode"":""laboratory"",""sortOrder"":40,""requiredRoles"":[""Admin"",""Doctor"",""LabTechnician""]},
  {""id"":""inventory"",""label"":""nav.inventory"",""icon"":""Package"",""route"":""/inventory"",""featureCode"":""inventory"",""sortOrder"":50},
  {""id"":""billing"",""label"":""nav.billing"",""icon"":""Receipt"",""route"":""/billing"",""featureCode"":""billing"",""sortOrder"":60},
  {""id"":""hr"",""label"":""nav.hr"",""icon"":""UserCog"",""route"":""/hr"",""featureCode"":""hr"",""sortOrder"":70,""requiredRoles"":[""Admin"",""HRManager""]},
  {""id"":""reports"",""label"":""nav.reports"",""icon"":""BarChart"",""route"":""/reports"",""featureCode"":""reports"",""sortOrder"":80},
  {""id"":""settings"",""label"":""nav.settings"",""icon"":""Settings"",""route"":""/settings"",""featureCode"":""settings"",""sortOrder"":100}
]";

    private static string GetClinicUISchemasJson() => @"{
  ""patient"": {
    ""entityName"": ""patient"",
    ""displayName"": ""entity.patient.singular"",
    ""displayNamePlural"": ""entity.patient.plural"",
    ""primaryField"": ""fullName"",
    ""fields"": [
      {""name"":""firstName"",""type"":""text"",""label"":""form.patient.firstName"",""validation"":{""required"":true,""maxLength"":100},""width"":""half""},
      {""name"":""lastName"",""type"":""text"",""label"":""form.patient.lastName"",""validation"":{""required"":true,""maxLength"":100},""width"":""half""},
      {""name"":""dateOfBirth"",""type"":""date"",""label"":""form.patient.dateOfBirth"",""validation"":{""required"":true},""width"":""half""},
      {""name"":""gender"",""type"":""select"",""label"":""form.patient.gender"",""options"":[{""value"":""male"",""label"":""Male""},{""value"":""female"",""label"":""Female""}],""validation"":{""required"":true},""width"":""half""},
      {""name"":""phone"",""type"":""phone"",""label"":""form.patient.phone"",""validation"":{""required"":true},""width"":""half""},
      {""name"":""email"",""type"":""email"",""label"":""form.patient.email"",""width"":""half""},
      {""name"":""emiratesId"",""type"":""emiratesId"",""label"":""form.patient.emiratesId"",""width"":""half""},
      {""name"":""passport"",""type"":""text"",""label"":""form.patient.passport"",""width"":""half""},
      {""name"":""nationality"",""type"":""select"",""label"":""form.patient.nationality"",""lookupEndpoint"":""/api/lookups/nationalities"",""width"":""half""},
      {""name"":""bloodType"",""type"":""select"",""label"":""form.patient.bloodType"",""options"":[{""value"":""A+"",""label"":""A+""},{""value"":""A-"",""label"":""A-""},{""value"":""B+"",""label"":""B+""},{""value"":""B-"",""label"":""B-""},{""value"":""AB+"",""label"":""AB+""},{""value"":""AB-"",""label"":""AB-""},{""value"":""O+"",""label"":""O+""},{""value"":""O-"",""label"":""O-""}],""width"":""half""},
      {""name"":""address"",""type"":""textarea"",""label"":""form.patient.address"",""width"":""full""},
      {""name"":""allergies"",""type"":""multiselect"",""label"":""form.patient.allergies"",""lookupEndpoint"":""/api/lookups/allergies"",""width"":""full""},
      {""name"":""medicalHistory"",""type"":""textarea"",""label"":""form.patient.medicalHistory"",""width"":""full""},
      {""name"":""emergencyContactName"",""type"":""text"",""label"":""form.emergencyContactName"",""width"":""half""},
      {""name"":""emergencyContactPhone"",""type"":""phone"",""label"":""form.emergencyContactPhone"",""width"":""half""}
    ]
  }
}";

    private static string GetClinicFormLayoutsJson() => @"{
  ""patient"": {
    ""entityName"": ""patient"",
    ""sections"": [
      {""id"":""basic"",""title"":""section.basicInfo"",""columns"":2,""fields"":[""firstName"",""lastName"",""dateOfBirth"",""gender""]},
      {""id"":""contact"",""title"":""section.contactInfo"",""columns"":2,""fields"":[""phone"",""email"",""address""]},
      {""id"":""identification"",""title"":""section.identification"",""columns"":2,""fields"":[""emiratesId"",""passport"",""nationality""]},
      {""id"":""medical"",""title"":""section.medicalInfo"",""columns"":2,""collapsible"":true,""fields"":[""bloodType"",""allergies"",""medicalHistory""]},
      {""id"":""emergency"",""title"":""section.emergencyInfo"",""columns"":2,""collapsible"":true,""fields"":[""emergencyContactName"",""emergencyContactPhone""]}
    ]
  }
}";

    private static string GetClinicListLayoutsJson() => @"{
  ""patient"": {
    ""entityName"": ""patient"",
    ""columns"": [
      {""field"":""fullName"",""sortable"":true},
      {""field"":""emiratesId""},
      {""field"":""phone""},
      {""field"":""dateOfBirth"",""format"":""date""},
      {""field"":""gender"",""format"":""badge""},
      {""field"":""lastVisitDate"",""format"":""date""}
    ],
    ""actions"": {
      ""row"": [
        {""id"":""view"",""label"":""action.view"",""icon"":""Eye"",""type"":""secondary""},
        {""id"":""edit"",""label"":""action.edit"",""icon"":""Pencil"",""type"":""primary""},
        {""id"":""newAppointment"",""label"":""action.newAppointment"",""icon"":""Calendar"",""type"":""secondary"",""featureCode"":""appointments""},
        {""id"":""delete"",""label"":""action.delete"",""icon"":""Trash"",""type"":""danger"",""confirmMessage"":""confirm.delete""}
      ],
      ""bulk"": [{""id"":""export"",""label"":""action.export"",""icon"":""Download"",""type"":""secondary""}],
      ""header"": [
        {""id"":""create"",""label"":""action.addPatient"",""icon"":""Plus"",""type"":""primary""},
        {""id"":""import"",""label"":""action.import"",""icon"":""Upload"",""type"":""secondary""}
      ]
    },
    ""filters"": [
      {""field"":""gender"",""type"":""select"",""options"":[{""value"":""male"",""label"":""Male""},{""value"":""female"",""label"":""Female""}]}
    ],
    ""defaultPageSize"": 25,
    ""pageSizeOptions"": [10, 25, 50, 100],
    ""showSearch"": true,
    ""searchFields"": [""firstName"", ""lastName"", ""emiratesId"", ""phone"", ""email""]
  }
}";

    #endregion

    #region Trading Templates

    private static string GetTradingTerminologyJson() => @"{
  ""entity.patient.singular"": ""Customer"",
  ""entity.patient.plural"": ""Customers"",
  ""entity.visit.singular"": ""Order"",
  ""entity.visit.plural"": ""Orders"",
  ""entity.appointment.singular"": ""Meeting"",
  ""entity.appointment.plural"": ""Meetings"",
  ""role.doctor"": ""Sales Representative"",
  ""role.nurse"": ""Sales Associate"",
  ""role.receptionist"": ""Customer Service"",
  ""nav.patients"": ""Customers"",
  ""nav.appointments"": ""Meetings"",
  ""nav.visits"": ""Orders"",
  ""nav.leads"": ""Leads"",
  ""nav.products"": ""Products"",
  ""nav.quotations"": ""Quotations"",
  ""page.patients.title"": ""Customer Management"",
  ""page.patients.subtitle"": ""Manage your customers and leads"",
  ""page.patients.addNew"": ""Add Customer"",
  ""page.patients.empty"": ""No customers found"",
  ""page.appointments.title"": ""Meeting Schedule"",
  ""page.visits.title"": ""Order Management"",
  ""form.patient.firstName"": ""First Name"",
  ""form.patient.lastName"": ""Last Name"",
  ""form.patient.phone"": ""Phone"",
  ""form.patient.email"": ""Email"",
  ""form.patient.company"": ""Company"",
  ""form.patient.emiratesId"": ""Trade License"",
  ""action.addPatient"": ""Add Customer"",
  ""action.editPatient"": ""Edit Customer"",
  ""section.businessInfo"": ""Business Information""
}";

    private static string GetTradingNavigationJson() => @"[
  {""id"":""dashboard"",""label"":""nav.dashboard"",""icon"":""LayoutDashboard"",""route"":""/"",""featureCode"":""dashboard"",""sortOrder"":0},
  {""id"":""customers"",""label"":""nav.patients"",""icon"":""Users"",""route"":""/customers"",""featureCode"":""customers"",""sortOrder"":10},
  {""id"":""leads"",""label"":""nav.leads"",""icon"":""Target"",""route"":""/leads"",""featureCode"":""leads"",""sortOrder"":20},
  {""id"":""products"",""label"":""nav.products"",""icon"":""Package"",""route"":""/products"",""featureCode"":""products"",""sortOrder"":30},
  {""id"":""orders"",""label"":""nav.visits"",""icon"":""ShoppingCart"",""route"":""/orders"",""featureCode"":""orders"",""sortOrder"":40},
  {""id"":""quotations"",""label"":""nav.quotations"",""icon"":""FileText"",""route"":""/quotations"",""featureCode"":""quotations"",""sortOrder"":50},
  {""id"":""inventory"",""label"":""nav.inventory"",""icon"":""Warehouse"",""route"":""/inventory"",""featureCode"":""inventory"",""sortOrder"":60},
  {""id"":""billing"",""label"":""nav.billing"",""icon"":""Receipt"",""route"":""/billing"",""featureCode"":""billing"",""sortOrder"":70},
  {""id"":""reports"",""label"":""nav.reports"",""icon"":""BarChart"",""route"":""/reports"",""featureCode"":""reports"",""sortOrder"":80},
  {""id"":""settings"",""label"":""nav.settings"",""icon"":""Settings"",""route"":""/settings"",""featureCode"":""settings"",""sortOrder"":100}
]";

    private static string GetTradingUISchemasJson() => @"{
  ""customer"": {
    ""entityName"": ""customer"",
    ""displayName"": ""entity.patient.singular"",
    ""displayNamePlural"": ""entity.patient.plural"",
    ""primaryField"": ""fullName"",
    ""fields"": [
      {""name"":""firstName"",""type"":""text"",""label"":""form.patient.firstName"",""validation"":{""required"":true,""maxLength"":100}},
      {""name"":""lastName"",""type"":""text"",""label"":""form.patient.lastName"",""validation"":{""required"":true,""maxLength"":100}},
      {""name"":""email"",""type"":""email"",""label"":""form.patient.email"",""validation"":{""required"":true}},
      {""name"":""phone"",""type"":""phone"",""label"":""form.patient.phone"",""validation"":{""required"":true}},
      {""name"":""company"",""type"":""text"",""label"":""form.patient.company""},
      {""name"":""tradeLicense"",""type"":""text"",""label"":""form.patient.emiratesId""},
      {""name"":""address"",""type"":""textarea"",""label"":""form.address""},
      {""name"":""notes"",""type"":""textarea"",""label"":""form.notes""},
      {""name"":""status"",""type"":""select"",""label"":""form.status"",""options"":[{""value"":""active"",""label"":""Active""},{""value"":""inactive"",""label"":""Inactive""},{""value"":""prospect"",""label"":""Prospect""}]}
    ]
  }
}";

    private static string GetTradingFormLayoutsJson() => @"{
  ""customer"": {
    ""entityName"": ""customer"",
    ""sections"": [
      {""id"":""basic"",""title"":""section.basicInfo"",""columns"":2,""fields"":[""firstName"",""lastName"",""email"",""phone""]},
      {""id"":""business"",""title"":""section.businessInfo"",""columns"":2,""fields"":[""company"",""tradeLicense"",""status""]},
      {""id"":""additional"",""title"":""section.additional"",""columns"":1,""fields"":[""address"",""notes""]}
    ]
  }
}";

    private static string GetTradingListLayoutsJson() => @"{
  ""customer"": {
    ""entityName"": ""customer"",
    ""columns"": [
      {""field"":""fullName"",""sortable"":true},
      {""field"":""email""},
      {""field"":""phone""},
      {""field"":""company""},
      {""field"":""status"",""format"":""badge""}
    ],
    ""actions"": {
      ""row"": [
        {""id"":""view"",""label"":""action.view"",""icon"":""Eye"",""type"":""secondary""},
        {""id"":""edit"",""label"":""action.edit"",""icon"":""Pencil"",""type"":""primary""},
        {""id"":""delete"",""label"":""action.delete"",""icon"":""Trash"",""type"":""danger"",""confirmMessage"":""confirm.delete""}
      ],
      ""bulk"": [{""id"":""export"",""label"":""action.export"",""icon"":""Download"",""type"":""secondary""}],
      ""header"": [
        {""id"":""create"",""label"":""action.addPatient"",""icon"":""Plus"",""type"":""primary""},
        {""id"":""import"",""label"":""action.import"",""icon"":""Upload"",""type"":""secondary""}
      ]
    },
    ""filters"": [
      {""field"":""status"",""type"":""select"",""options"":[{""value"":""active"",""label"":""Active""},{""value"":""inactive"",""label"":""Inactive""},{""value"":""prospect"",""label"":""Prospect""}]}
    ],
    ""defaultPageSize"": 25,
    ""pageSizeOptions"": [10, 25, 50, 100],
    ""showSearch"": true,
    ""searchFields"": [""firstName"", ""lastName"", ""email"", ""phone"", ""company""]
  }
}";

    #endregion

    #region Audiology Templates

    private static string GetAudiologyTerminologyJson() => @"{
  ""nav.audiology"": ""Audiology"",
  ""nav.audiograms"": ""Audiograms"",
  ""nav.hearingDevices"": ""Hearing Devices"",
  ""nav.fittingSessions"": ""Fitting Sessions"",
  ""entity.audiogram.singular"": ""Audiogram"",
  ""entity.audiogram.plural"": ""Audiograms"",
  ""entity.hearingDevice.singular"": ""Hearing Device"",
  ""entity.hearingDevice.plural"": ""Hearing Devices"",
  ""form.audiogram.testDate"": ""Test Date"",
  ""form.audiogram.testType"": ""Test Type"",
  ""form.hearingDevice.manufacturer"": ""Manufacturer"",
  ""form.hearingDevice.model"": ""Model"",
  ""form.hearingDevice.serialNumber"": ""Serial Number"",
  ""form.hearingDevice.side"": ""Side"",
  ""section.audiogramResults"": ""Audiogram Results"",
  ""section.deviceDetails"": ""Device Details""
}";

    private static string GetAudiologyNavigationJson() => @"[
  {""id"":""audiology"",""label"":""nav.audiology"",""icon"":""Ear"",""route"":""/audiology"",""featureCode"":""audiogram"",""sortOrder"":35,""children"":[
    {""id"":""audiograms"",""label"":""nav.audiograms"",""icon"":""Activity"",""route"":""/audiology/audiograms"",""featureCode"":""audiogram"",""sortOrder"":0},
    {""id"":""hearingDevices"",""label"":""nav.hearingDevices"",""icon"":""Headphones"",""route"":""/audiology/devices"",""featureCode"":""hearingDevices"",""sortOrder"":1},
    {""id"":""fittingSessions"",""label"":""nav.fittingSessions"",""icon"":""Wrench"",""route"":""/audiology/fittings"",""featureCode"":""fittingSessions"",""sortOrder"":2}
  ]}
]";

    private static string GetAudiologyUISchemasJson() => @"{
  ""audiogram"": {
    ""entityName"": ""audiogram"",
    ""displayName"": ""entity.audiogram.singular"",
    ""displayNamePlural"": ""entity.audiogram.plural"",
    ""primaryField"": ""testDate"",
    ""fields"": [
      {""name"":""patientId"",""type"":""lookup"",""label"":""entity.patient.singular"",""lookupEndpoint"":""/api/patients"",""lookupDisplayField"":""fullName"",""validation"":{""required"":true}},
      {""name"":""testDate"",""type"":""datetime"",""label"":""form.audiogram.testDate"",""validation"":{""required"":true}},
      {""name"":""testType"",""type"":""select"",""label"":""form.audiogram.testType"",""options"":[{""value"":""puretoneBoth"",""label"":""Pure Tone (Both Ears)""},{""value"":""puretoneRight"",""label"":""Pure Tone (Right)""},{""value"":""puretoneLeft"",""label"":""Pure Tone (Left)""},{""value"":""speech"",""label"":""Speech Audiometry""}],""validation"":{""required"":true}},
      {""name"":""rightEarData"",""type"":""json"",""label"":""form.audiogram.rightEar""},
      {""name"":""leftEarData"",""type"":""json"",""label"":""form.audiogram.leftEar""},
      {""name"":""interpretation"",""type"":""textarea"",""label"":""form.interpretation""},
      {""name"":""recommendations"",""type"":""textarea"",""label"":""form.recommendations""}
    ]
  },
  ""hearingDevice"": {
    ""entityName"": ""hearingDevice"",
    ""displayName"": ""entity.hearingDevice.singular"",
    ""displayNamePlural"": ""entity.hearingDevice.plural"",
    ""primaryField"": ""model"",
    ""fields"": [
      {""name"":""patientId"",""type"":""lookup"",""label"":""entity.patient.singular"",""lookupEndpoint"":""/api/patients"",""validation"":{""required"":true}},
      {""name"":""manufacturer"",""type"":""select"",""label"":""form.hearingDevice.manufacturer"",""options"":[{""value"":""phonak"",""label"":""Phonak""},{""value"":""widex"",""label"":""Widex""},{""value"":""resound"",""label"":""ReSound""},{""value"":""signia"",""label"":""Signia""},{""value"":""oticon"",""label"":""Oticon""}],""validation"":{""required"":true}},
      {""name"":""model"",""type"":""text"",""label"":""form.hearingDevice.model"",""validation"":{""required"":true}},
      {""name"":""serialNumber"",""type"":""text"",""label"":""form.hearingDevice.serialNumber"",""validation"":{""required"":true}},
      {""name"":""side"",""type"":""select"",""label"":""form.hearingDevice.side"",""options"":[{""value"":""left"",""label"":""Left""},{""value"":""right"",""label"":""Right""},{""value"":""both"",""label"":""Both (Binaural)""}],""validation"":{""required"":true}},
      {""name"":""purchaseDate"",""type"":""date"",""label"":""form.purchaseDate""},
      {""name"":""warrantyExpiry"",""type"":""date"",""label"":""form.warrantyExpiry""},
      {""name"":""price"",""type"":""currency"",""label"":""form.price"",""currency"":""AED""}
    ]
  }
}";

    private static string GetAudiologyFormLayoutsJson() => @"{
  ""audiogram"": {
    ""entityName"": ""audiogram"",
    ""sections"": [
      {""id"":""testInfo"",""title"":""section.testInfo"",""columns"":2,""fields"":[""patientId"",""testDate"",""testType""]},
      {""id"":""results"",""title"":""section.audiogramResults"",""columns"":2,""fields"":[""rightEarData"",""leftEarData""]},
      {""id"":""assessment"",""title"":""section.assessment"",""columns"":1,""fields"":[""interpretation"",""recommendations""]}
    ]
  },
  ""hearingDevice"": {
    ""entityName"": ""hearingDevice"",
    ""sections"": [
      {""id"":""patient"",""title"":""entity.patient.singular"",""columns"":1,""fields"":[""patientId""]},
      {""id"":""device"",""title"":""section.deviceDetails"",""columns"":2,""fields"":[""manufacturer"",""model"",""serialNumber"",""side""]},
      {""id"":""purchase"",""title"":""section.purchaseInfo"",""columns"":2,""fields"":[""purchaseDate"",""warrantyExpiry"",""price""]}
    ]
  }
}";

    #endregion

    #region Dental Templates

    private static string GetDentalTerminologyJson() => @"{
  ""nav.dental"": ""Dental"",
  ""nav.toothChart"": ""Tooth Chart"",
  ""nav.treatmentPlans"": ""Treatment Plans"",
  ""nav.procedures"": ""Procedures"",
  ""entity.tooth.singular"": ""Tooth"",
  ""entity.tooth.plural"": ""Teeth"",
  ""entity.treatmentPlan.singular"": ""Treatment Plan"",
  ""entity.treatmentPlan.plural"": ""Treatment Plans"",
  ""entity.procedure.singular"": ""Procedure"",
  ""entity.procedure.plural"": ""Procedures"",
  ""form.tooth.number"": ""Tooth Number"",
  ""form.tooth.condition"": ""Condition"",
  ""form.procedure.type"": ""Procedure Type"",
  ""form.procedure.surface"": ""Surface(s)"",
  ""section.toothConditions"": ""Tooth Conditions"",
  ""section.plannedTreatments"": ""Planned Treatments""
}";

    private static string GetDentalNavigationJson() => @"[
  {""id"":""dental"",""label"":""nav.dental"",""icon"":""Smile"",""route"":""/dental"",""featureCode"":""toothChart"",""sortOrder"":35,""children"":[
    {""id"":""toothChart"",""label"":""nav.toothChart"",""icon"":""Grid3x3"",""route"":""/dental/chart"",""featureCode"":""toothChart"",""sortOrder"":0},
    {""id"":""treatmentPlans"",""label"":""nav.treatmentPlans"",""icon"":""ListChecks"",""route"":""/dental/plans"",""featureCode"":""treatmentPlan"",""sortOrder"":1},
    {""id"":""procedures"",""label"":""nav.procedures"",""icon"":""Stethoscope"",""route"":""/dental/procedures"",""featureCode"":""dentalProcedures"",""sortOrder"":2}
  ]}
]";

    private static string GetDentalUISchemasJson() => @"{
  ""dentalProcedure"": {
    ""entityName"": ""dentalProcedure"",
    ""displayName"": ""entity.procedure.singular"",
    ""displayNamePlural"": ""entity.procedure.plural"",
    ""primaryField"": ""procedureType"",
    ""fields"": [
      {""name"":""patientId"",""type"":""lookup"",""label"":""entity.patient.singular"",""lookupEndpoint"":""/api/patients"",""validation"":{""required"":true}},
      {""name"":""procedureType"",""type"":""select"",""label"":""form.procedure.type"",""options"":[{""value"":""filling"",""label"":""Filling""},{""value"":""extraction"",""label"":""Extraction""},{""value"":""rootCanal"",""label"":""Root Canal""},{""value"":""crown"",""label"":""Crown""},{""value"":""cleaning"",""label"":""Cleaning/Scaling""}],""validation"":{""required"":true}},
      {""name"":""toothNumber"",""type"":""select"",""label"":""form.tooth.number""},
      {""name"":""surfaces"",""type"":""multiselect"",""label"":""form.procedure.surface"",""options"":[{""value"":""M"",""label"":""Mesial""},{""value"":""D"",""label"":""Distal""},{""value"":""O"",""label"":""Occlusal""},{""value"":""B"",""label"":""Buccal""},{""value"":""L"",""label"":""Lingual""}]},
      {""name"":""procedureDate"",""type"":""datetime"",""label"":""form.date"",""validation"":{""required"":true}},
      {""name"":""cost"",""type"":""currency"",""label"":""form.cost"",""currency"":""AED""},
      {""name"":""notes"",""type"":""textarea"",""label"":""form.notes""},
      {""name"":""status"",""type"":""select"",""label"":""form.status"",""options"":[{""value"":""planned"",""label"":""Planned""},{""value"":""inProgress"",""label"":""In Progress""},{""value"":""completed"",""label"":""Completed""}]}
    ]
  }
}";

    #endregion

    #region Vet Templates

    private static string GetVetTerminologyJson() => @"{
  ""entity.patient.singular"": ""Pet"",
  ""entity.patient.plural"": ""Pets"",
  ""entity.owner.singular"": ""Owner"",
  ""entity.owner.plural"": ""Owners"",
  ""entity.vaccination.singular"": ""Vaccination"",
  ""entity.vaccination.plural"": ""Vaccinations"",
  ""nav.patients"": ""Pets"",
  ""nav.petOwners"": ""Pet Owners"",
  ""nav.vaccinations"": ""Vaccinations"",
  ""nav.grooming"": ""Grooming"",
  ""nav.boarding"": ""Boarding"",
  ""page.patients.title"": ""Pet Management"",
  ""page.patients.subtitle"": ""Manage pet records and medical history"",
  ""page.patients.addNew"": ""Register Pet"",
  ""page.patients.empty"": ""No pets registered"",
  ""form.patient.firstName"": ""Pet Name"",
  ""form.patient.species"": ""Species"",
  ""form.patient.breed"": ""Breed"",
  ""form.patient.color"": ""Color/Markings"",
  ""form.patient.microchip"": ""Microchip Number"",
  ""form.patient.weight"": ""Weight (kg)"",
  ""form.vaccination.type"": ""Vaccine Type"",
  ""form.vaccination.date"": ""Vaccination Date"",
  ""form.vaccination.nextDue"": ""Next Due Date"",
  ""action.addPatient"": ""Register Pet"",
  ""action.viewVaccinations"": ""View Vaccinations"",
  ""section.petInfo"": ""Pet Information"",
  ""section.ownerInfo"": ""Owner Information"",
  ""section.vaccinationHistory"": ""Vaccination History""
}";

    private static string GetVetNavigationJson() => @"[
  {""id"":""pets"",""label"":""nav.patients"",""icon"":""PawPrint"",""route"":""/pets"",""featureCode"":""petManagement"",""sortOrder"":10},
  {""id"":""petOwners"",""label"":""nav.petOwners"",""icon"":""Users"",""route"":""/owners"",""featureCode"":""petOwners"",""sortOrder"":15},
  {""id"":""vaccinations"",""label"":""nav.vaccinations"",""icon"":""Syringe"",""route"":""/vaccinations"",""featureCode"":""vaccinations"",""sortOrder"":25,""badge"":{""type"":""count"",""countKey"":""dueVaccinations""}},
  {""id"":""grooming"",""label"":""nav.grooming"",""icon"":""Scissors"",""route"":""/grooming"",""featureCode"":""grooming"",""sortOrder"":45},
  {""id"":""boarding"",""label"":""nav.boarding"",""icon"":""Home"",""route"":""/boarding"",""featureCode"":""boarding"",""sortOrder"":46}
]";

    private static string GetVetUISchemasJson() => @"{
  ""patient"": {
    ""entityName"": ""patient"",
    ""displayName"": ""entity.patient.singular"",
    ""displayNamePlural"": ""entity.patient.plural"",
    ""primaryField"": ""petName"",
    ""fields"": [
      {""name"":""petName"",""type"":""text"",""label"":""form.patient.firstName"",""validation"":{""required"":true,""maxLength"":100}},
      {""name"":""species"",""type"":""select"",""label"":""form.patient.species"",""options"":[{""value"":""dog"",""label"":""Dog""},{""value"":""cat"",""label"":""Cat""},{""value"":""bird"",""label"":""Bird""},{""value"":""rabbit"",""label"":""Rabbit""},{""value"":""other"",""label"":""Other""}],""validation"":{""required"":true}},
      {""name"":""breed"",""type"":""text"",""label"":""form.patient.breed""},
      {""name"":""dateOfBirth"",""type"":""date"",""label"":""form.patient.dateOfBirth""},
      {""name"":""gender"",""type"":""select"",""label"":""form.patient.gender"",""options"":[{""value"":""male"",""label"":""Male""},{""value"":""female"",""label"":""Female""},{""value"":""neutered"",""label"":""Neutered Male""},{""value"":""spayed"",""label"":""Spayed Female""}]},
      {""name"":""color"",""type"":""text"",""label"":""form.patient.color""},
      {""name"":""weight"",""type"":""number"",""label"":""form.patient.weight"",""decimals"":2},
      {""name"":""microchip"",""type"":""text"",""label"":""form.patient.microchip""},
      {""name"":""ownerId"",""type"":""lookup"",""label"":""entity.owner.singular"",""lookupEndpoint"":""/api/petowners"",""validation"":{""required"":true}},
      {""name"":""medicalHistory"",""type"":""textarea"",""label"":""form.patient.medicalHistory""}
    ]
  },
  ""vaccination"": {
    ""entityName"": ""vaccination"",
    ""displayName"": ""entity.vaccination.singular"",
    ""displayNamePlural"": ""entity.vaccination.plural"",
    ""primaryField"": ""vaccineType"",
    ""fields"": [
      {""name"":""petId"",""type"":""lookup"",""label"":""entity.patient.singular"",""lookupEndpoint"":""/api/pets"",""validation"":{""required"":true}},
      {""name"":""vaccineType"",""type"":""select"",""label"":""form.vaccination.type"",""options"":[{""value"":""rabies"",""label"":""Rabies""},{""value"":""distemper"",""label"":""Distemper""},{""value"":""parvovirus"",""label"":""Parvovirus""},{""value"":""bordetella"",""label"":""Bordetella""}],""validation"":{""required"":true}},
      {""name"":""vaccinationDate"",""type"":""date"",""label"":""form.vaccination.date"",""validation"":{""required"":true}},
      {""name"":""nextDueDate"",""type"":""date"",""label"":""form.vaccination.nextDue""},
      {""name"":""batchNumber"",""type"":""text"",""label"":""form.vaccination.batch""},
      {""name"":""notes"",""type"":""textarea"",""label"":""form.notes""}
    ]
  }
}";

    private static string GetVetFormLayoutsJson() => @"{
  ""patient"": {
    ""entityName"": ""patient"",
    ""sections"": [
      {""id"":""pet"",""title"":""section.petInfo"",""columns"":2,""fields"":[""petName"",""species"",""breed"",""dateOfBirth"",""gender"",""color"",""weight"",""microchip""]},
      {""id"":""owner"",""title"":""section.ownerInfo"",""columns"":1,""fields"":[""ownerId""]},
      {""id"":""medical"",""title"":""section.medicalInfo"",""columns"":1,""collapsible"":true,""fields"":[""medicalHistory""]}
    ]
  }
}";

    #endregion
}
