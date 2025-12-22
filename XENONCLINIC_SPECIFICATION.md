<project_specification>
  <project_name>XenonClinic - Multi-Tenant Healthcare ERP System</project_name>

  <overview>
    XenonClinic is a comprehensive, multi-tenant healthcare management system (ERP) designed for clinics,
    hospitals, and healthcare organizations. The system provides complete patient care, clinical documentation,
    financial management, HR/payroll, inventory control, and support for 14+ medical specialties. Built with
    modern clean architecture principles, the platform offers multi-tenancy with data isolation, role-based
    access control, and extensive customization capabilities including custom UI schemas and terminology per tenant.
  </overview>

  <technology_stack>
    <frontend>
      <framework>React 19.2.0 with TypeScript 5.9</framework>
      <build_tool>Vite 7.2</build_tool>
      <styling>Tailwind CSS 3.4</styling>
      <state_management>Zustand 5.0</state_management>
      <routing>React Router DOM 7.10</routing>
      <data_fetching>TanStack React Query 5.90</data_fetching>
      <form_handling>React Hook Form 7.68</form_handling>
      <charting>Recharts 3.5</charting>
      <ui_components>Headless UI 2.2, Heroicons 2.2</ui_components>
      <testing>Vitest 4.0, React Testing Library 16.3</testing>
      <documentation>Storybook 8.0</documentation>
      <port>Admin Dashboard: 3000, Public Website: 3001</port>
    </frontend>
    <backend>
      <runtime>.NET 8.0 (C#)</runtime>
      <framework>ASP.NET Core</framework>
      <database>PostgreSQL 16 (primary), SQL Server (supported)</database>
      <orm>Entity Framework Core 8.0.10</orm>
      <caching>Redis 7.2</caching>
      <validation>FluentValidation</validation>
      <authentication>ASP.NET Identity + JWT Bearer 8.0.10</authentication>
      <port>API: 5000</port>
    </backend>
    <infrastructure>
      <containerization>Docker and Docker Compose</containerization>
      <code_quality>ESLint, Prettier, dotnet format</code_quality>
      <git_hooks>Husky, lint-staged</git_hooks>
      <testing>xUnit, NUnit (backend), Playwright (E2E)</testing>
      <ci_cd>GitHub Actions</ci_cd>
    </infrastructure>
    <communication>
      <api>RESTful endpoints (401+ endpoints)</api>
      <real_time>SignalR for notifications (future)</real_time>
      <integrations>FHIR compliance, PACS/DICOM, OAuth, Payment Gateways</integrations>
    </communication>
  </technology_stack>

  <architecture>
    <pattern>Clean Architecture with Layered Design</pattern>
    <layers>
      <presentation>XenonClinic.Api - REST API Controllers (20+)</presentation>
      <application>XenonClinic.Core - Domain entities, DTOs, interfaces, business logic abstractions</application>
      <infrastructure>XenonClinic.Infrastructure - EF Core DbContext, repository implementations, services (40+)</infrastructure>
      <frontend>XenonClinic.React - Admin Dashboard, Shared.UI - Component Library</frontend>
      <platform>Xenon.Platform - SaaS Platform API for tenant management</platform>
      <public>Xenon.PublicWebsite - Public marketing website</public>
    </layers>
    <statistics>
      <total_code_lines>44,592+ (services alone)</total_code_lines>
      <api_endpoints>401+</api_endpoints>
      <database_entities>118+</database_entities>
      <controllers>20+</controllers>
      <services>40+</services>
      <dtos>27+</dtos>
      <enums>30+</enums>
      <specialty_modules>14+</specialty_modules>
    </statistics>
  </architecture>

  <core_modules>
    <patient_management>
      <description>Complete patient lifecycle management</description>
      <features>
        - Patient registration and profile management
        - Medical history tracking and documentation
        - Document storage and management (reports, images, scans)
        - Allergy and medication tracking
        - Insurance information management
        - Patient search with advanced filtering
        - Patient consent tracking and management
        - Patient portal self-service access
        - Patient statistics and demographics
      </features>
      <entities>
        - Patient - Core patient records
        - PatientMedicalHistory - Historical medical information
        - PatientDocument - Document storage
        - PatientAllergy - Allergy records
        - PatientInsurance - Insurance details
        - PatientConsent - Consent tracking
      </entities>
      <api_endpoints>
        - GET /api/patient - List patients with pagination
        - GET /api/patient/{id} - Get single patient
        - POST /api/patient - Create new patient
        - PUT /api/patient/{id} - Update patient
        - DELETE /api/patient/{id} - Delete patient
        - GET /api/patient/{patientId}/medical-history - Get medical history
        - POST /api/patient/{patientId}/medical-history - Save medical history
        - GET /api/patient/{patientId}/documents - Get documents
        - POST /api/patient/{patientId}/documents - Upload document
        - GET /api/patient/statistics - Patient statistics
      </api_endpoints>
    </patient_management>

    <appointment_management>
      <description>Comprehensive scheduling and appointment workflow</description>
      <features>
        - Appointment scheduling and booking
        - Doctor schedule management
        - Appointment status tracking (Scheduled, Confirmed, CheckedIn, InProgress, Completed, Cancelled, NoShow)
        - Appointment type categorization (Consultation, FollowUp, Emergency, NewPatient, HearingTest, DentalCheckup, etc.)
        - Date-based filtering and search
        - Appointment duration management
        - Recurring appointments
        - Calendar view with day/week/month views
        - Appointment reminders and notifications
        - Doctor availability management
      </features>
      <entities>
        - Appointment - Appointment records
        - DoctorSchedule - Doctor availability scheduling
        - AppointmentType - Appointment type lookups
        - AppointmentStatus - Status definitions
      </entities>
      <api_endpoints>
        - GET /api/appointments - List appointments with date range filtering
        - GET /api/appointments/{id} - Get appointment details
        - POST /api/appointments - Create appointment
        - PUT /api/appointments/{id} - Update appointment
        - DELETE /api/appointments/{id} - Delete appointment
        - PUT /api/appointments/{id}/status - Update status
        - GET /api/appointments/by-doctor/{doctorId} - Get by doctor
        - GET /api/appointments/by-patient/{patientId} - Get by patient
      </api_endpoints>
    </appointment_management>

    <clinical_visits>
      <description>Clinical documentation and patient encounter management</description>
      <features>
        - Clinical visit documentation
        - Vital signs tracking (temperature, BP, heart rate, respiratory rate, SpO2, height, weight, BMI)
        - Diagnosis recording with ICD-10 codes
        - Prescription management with drug interactions
        - Procedures documentation with CPT codes
        - Referrals management
        - Visit notes and clinical observations
        - Lab order integration
        - Follow-up scheduling
      </features>
      <entities>
        - ClinicalVisit - Visit records
        - VitalSign - Vital measurements
        - Diagnosis - Diagnosis information
        - Prescription - Medication prescriptions
        - PrescriptionItem - Individual prescription items
        - Procedure - Medical procedures
        - Visit - General visit tracking
      </entities>
      <api_endpoints>
        - GET /api/clinical-visits - List visits
        - GET /api/clinical-visits/{id} - Get visit details
        - POST /api/clinical-visits - Create visit
        - PUT /api/clinical-visits/{id} - Update visit
        - POST /api/clinical-visits/{id}/vitals - Add vital signs
        - POST /api/clinical-visits/{id}/diagnoses - Add diagnosis
        - POST /api/clinical-visits/{id}/prescriptions - Add prescription
        - POST /api/clinical-visits/{id}/procedures - Add procedure
        - POST /api/clinical-visits/{id}/lab-orders - Create lab order
      </api_endpoints>
    </clinical_visits>

    <laboratory_module>
      <description>Complete laboratory operations management</description>
      <features>
        - Lab test catalog management
        - Lab order creation and tracking
        - Order status workflow (Pending → Collected → InProgress → Completed)
        - Specimen tracking and collection
        - Lab result entry and validation
        - External lab integration
        - Result reporting and printing
        - Lab test categorization
        - Critical value alerts
        - Reference range management
      </features>
      <entities>
        - LabTest - Available lab tests
        - LabOrder - Lab test orders
        - LabOrderItem - Individual items in orders
        - LabResult - Test results
        - ExternalLab - External lab management
        - SpecimenType - Specimen type lookups
        - TestCategory - Test categorization
      </entities>
      <api_endpoints>
        - GET /api/laboratory/tests - List available tests
        - GET /api/laboratory/tests/{id} - Get test details
        - POST /api/laboratory/tests - Create test
        - PUT /api/laboratory/tests/{id} - Update test
        - GET /api/laboratory/orders - List lab orders
        - GET /api/laboratory/orders/{id} - Get order details
        - POST /api/laboratory/orders - Create lab order
        - PUT /api/laboratory/orders/{id}/status - Update order status
        - POST /api/laboratory/orders/{id}/collect-samples - Record sample collection
        - GET /api/laboratory/orders/{orderId}/results - Get results
        - POST /api/laboratory/results - Record results
        - GET /api/laboratory/pending - Get pending orders
        - GET /api/laboratory/urgent - Get urgent orders
      </api_endpoints>
    </laboratory_module>

    <radiology_module>
      <description>Medical imaging and DICOM management</description>
      <features>
        - Imaging order management
        - DICOM viewer integration
        - PACS (Picture Archiving and Communication System) configuration
        - Radiology report generation
        - Study and series management
        - DICOM worklist support
        - Image streaming and caching
        - Multi-modality support (X-Ray, CT, MRI, Ultrasound, etc.)
        - Report templates
        - Critical findings alerts
      </features>
      <entities>
        - DicomStudy - DICOM imaging studies
        - DicomSeries - Series within studies
        - DicomInstance - Individual DICOM images
        - RadiologyReport - Imaging reports
        - PacsServerConfig - PACS server configuration
        - DicomWorklistEntry - Worklist management
      </entities>
      <api_endpoints>
        - GET /api/radiology/studies - List studies
        - GET /api/radiology/studies/{id} - Get study details
        - POST /api/radiology/orders - Create imaging order
        - POST /api/radiology/dicom/upload - Upload DICOM
        - GET /api/radiology/reports - List reports
        - POST /api/radiology/reports - Create report
        - GET /api/radiology/pacs-config - Get PACS configuration
        - PUT /api/radiology/pacs-config - Update PACS configuration
      </api_endpoints>
    </radiology_module>

    <pharmacy_module>
      <description>Pharmacy and medication management</description>
      <features>
        - Prescription management and fulfillment
        - Drug inventory tracking
        - Controlled substance management
        - Drug adverse event reporting
        - Formulary management
        - Patient assistance programs
        - Drug pricing and interactions
        - Drug database integration
        - Prescription refill management
        - Drug-drug and drug-allergy interaction checking
      </features>
      <entities>
        - FormularyDrug - Drug formulary
        - DrugPricing - Drug pricing information
        - ControlledSubstanceInfo - Controlled substance tracking
        - DrugAdverseEvent - Adverse event reports
        - PatientAssistanceProgram - Assistance programs
        - DrugDatabaseUpdate - Drug database updates
      </entities>
    </pharmacy_module>

    <financial_management>
      <description>Complete financial operations and billing</description>
      <features>
        - Patient invoicing and billing
        - Payment processing and tracking
        - Expense management with approval workflows
        - Chart of Accounts management
        - Financial transactions and journaling
        - Insurance claim processing
        - Insurance pre-authorization
        - Payment gateway integration
        - Financial reporting and analytics
        - Voucher management
        - Co-payment tracking
        - Revenue analysis
        - Cost estimation
      </features>
      <entities>
        - Invoice - Patient invoices
        - InvoicePayment - Payment records
        - Payment - Payment transactions
        - Account - Chart of accounts
        - FinancialAccount - Financial accounts
        - FinancialTransaction - Transaction records
        - Expense - Expense records
        - ExpenseCategory - Expense categories
        - Voucher - Voucher management
        - VoucherLine - Voucher line items
        - InsuranceClaim - Insurance claims
        - InsuranceClaimItem - Claim items
        - InsurancePreAuthorization - Pre-authorization records
        - PaymentGatewayConfig - Payment gateway configuration
        - PaymentGatewayTransaction - Gateway transactions
      </entities>
      <api_endpoints>
        - GET /api/financial/accounts - List accounts
        - POST /api/financial/accounts - Create account
        - GET /api/financial/invoices - List invoices
        - POST /api/financial/invoices - Create invoice
        - PUT /api/financial/invoices/{id} - Update invoice
        - POST /api/financial/invoices/{id}/void - Void invoice
        - POST /api/financial/invoices/{id}/duplicate - Duplicate invoice
        - POST /api/financial/payments - Record payment
        - GET /api/financial/expenses - List expenses
        - POST /api/financial/expenses - Create expense
        - PUT /api/financial/expenses/{id}/approve - Approve expense
        - GET /api/financial/transactions - List transactions
        - POST /api/financial/claims - Submit insurance claim
        - POST /api/financial/pre-auth - Request pre-authorization
        - GET /api/financial/reports/revenue - Revenue report
        - GET /api/financial/reports/expenses - Expense report
      </api_endpoints>
    </financial_management>

    <inventory_management>
      <description>Stock and supply chain management</description>
      <features>
        - Stock item management
        - Inventory transactions (purchase, sale, transfer, adjustment)
        - Supplier management
        - Goods receipt processing
        - Purchase order management
        - Inventory adjustments
        - Multi-location tracking
        - Low stock alerts
        - Quotation management
        - Supplier payments
        - Stock level reporting
        - Batch and expiry tracking
      </features>
      <entities>
        - InventoryItem - Stock items
        - InventoryTransaction - Transaction records
        - Supplier - Supplier information
        - PurchaseOrder - Purchase orders
        - PurchaseOrderItem - PO line items
        - GoodsReceipt - Goods receipt records
        - GoodsReceiptItem - Receipt line items
        - SupplierPayment - Supplier payments
        - Quotation - Purchase quotations
        - QuotationItem - Quotation line items
      </entities>
      <api_endpoints>
        - GET /api/inventory/items - List inventory items
        - POST /api/inventory/items - Create item
        - PUT /api/inventory/items/{id} - Update item
        - GET /api/inventory/transactions - List transactions
        - POST /api/inventory/transactions - Record transaction
        - GET /api/inventory/purchase-orders - List POs
        - POST /api/inventory/purchase-orders - Create PO
        - POST /api/inventory/goods-receipts - Create goods receipt
        - GET /api/inventory/suppliers - List suppliers
        - POST /api/inventory/suppliers - Create supplier
        - GET /api/inventory/low-stock - Get low stock alerts
        - GET /api/inventory/stock-report - Stock level report
      </api_endpoints>
    </inventory_management>

    <hr_management>
      <description>Human resources and employee management</description>
      <features>
        - Employee profile management
        - Attendance tracking (clock in/out)
        - Leave management and approval
        - Performance reviews
        - Job positions and departments
        - Employee document management
        - Work schedule management
        - Employee onboarding
        - Organizational hierarchy
        - Skills and certifications tracking
      </features>
      <entities>
        - Employee - Employee records
        - Doctor - Doctor-specific information
        - Attendance - Attendance records
        - AttendanceRecord - Detailed attendance
        - LeaveRequest - Leave requests
        - PerformanceReview - Performance evaluations
        - Department - Organizational departments
        - JobPosition - Job positions
      </entities>
      <api_endpoints>
        - GET /api/hr/employees - List employees
        - POST /api/hr/employees - Create employee
        - PUT /api/hr/employees/{id} - Update employee
        - GET /api/hr/attendance - List attendance
        - POST /api/hr/attendance/clock-in - Clock in
        - POST /api/hr/attendance/clock-out - Clock out
        - GET /api/hr/leave-requests - List leave requests
        - POST /api/hr/leave-requests - Submit request
        - PUT /api/hr/leave-requests/{id}/approve - Approve request
        - GET /api/hr/performance-reviews - List reviews
        - POST /api/hr/performance-reviews - Create review
        - GET /api/hr/departments - List departments
        - GET /api/hr/positions - List positions
      </api_endpoints>
    </hr_management>

    <payroll_module>
      <description>Payroll processing and salary management</description>
      <features>
        - Salary and payroll processing
        - Payslip generation
        - Salary components (basic, allowances, deductions)
        - Tax calculations and configuration
        - WPS (Wage Protection System) submission (Gulf-specific)
        - Loan and deduction management
        - Overtime calculation
        - Payroll period management
        - Salary history tracking
      </features>
      <entities>
        - PayrollPeriod - Payroll periods
        - Payslip - Generated payslips
        - SalaryComponent - Salary components
        - TaxConfiguration - Tax settings
        - WpsSubmission - WPS submission records
      </entities>
    </payroll_module>
  </core_modules>

  <specialty_modules>
    <cardiology>
      <description>Cardiology specialty module</description>
      <features>
        - ECG recording and analysis
        - Echocardiogram results
        - Stress test management
        - Cardiac procedures documentation
        - Heart condition tracking
        - Cardiac risk assessment
      </features>
      <entities>CardioVisit, ECGRecord, EchoResult, StressTest, CardiacProcedure, HeartCondition</entities>
    </cardiology>

    <dental>
      <description>Dental specialty module</description>
      <features>
        - Tooth charting system (Odontogram)
        - Individual tooth records
        - Dental procedures documentation
        - Treatment planning
        - X-ray imaging management
        - Periodontal charting
      </features>
      <entities>DentalVisit, ToothChart, ToothRecord, DentalProcedure, DentalTreatmentPlan, DentalTreatmentPlanItem, DentalXRay</entities>
    </dental>

    <ophthalmology>
      <description>Eye care specialty module</description>
      <features>
        - Visual acuity testing
        - Refraction examination
        - Intraocular pressure (IOP) measurement
        - Eye procedures documentation
        - Glasses/lens prescription management
        - Eye condition tracking
      </features>
      <entities>OphthalmologyVisit, VisionTest, EyeExam, EyePrescription, EyeProcedure, EyeCondition</entities>
    </ophthalmology>

    <dermatology>
      <description>Dermatology specialty module</description>
      <features>
        - Skin condition documentation
        - Lesion photography and tracking
        - Mole mapping
        - Dermatological procedures
        - Treatment planning
        - Skin biopsy management
      </features>
      <entities>DermatologyVisit, SkinCondition, LesionRecord, SkinProcedure, SkinTreatmentPlan</entities>
    </dermatology>

    <physiotherapy>
      <description>Physical therapy specialty module</description>
      <features>
        - Physio assessments
        - Treatment session tracking
        - Exercise program management
        - Range of motion (ROM) measurements
        - Functional outcome tracking
        - Progress documentation
      </features>
      <entities>PhysioAssessment, PhysioSession, ExerciseProgram, ExerciseProgramItem, RangeOfMotionRecord, FunctionalOutcome</entities>
    </physiotherapy>

    <pediatrics>
      <description>Pediatric care specialty module</description>
      <features>
        - Growth chart tracking
        - Vaccination schedule management
        - Developmental milestone tracking
        - Developmental screening
        - Newborn records
        - Pediatric-specific assessments
      </features>
      <entities>PediatricVisit, GrowthRecord, VaccinationRecord, DevelopmentalMilestone, DevelopmentalScreening, NewbornRecord</entities>
    </pediatrics>

    <ent>
      <description>ENT (Otolaryngology) specialty module</description>
      <features>
        - Hearing screening and tests
        - Sinus assessment
        - Throat examination
        - ENT procedures
        - Allergy testing
        - Endoscopy documentation
      </features>
      <entities>ENTVisit, HearingScreening, SinusAssessment, ThroatExam, ENTProcedure, AllergyTest</entities>
    </ent>

    <audiology>
      <description>Audiology specialty module</description>
      <features>
        - Audiometric testing
        - Audiogram recording
        - Hearing device management
        - Hearing loss classification
        - Hearing aid fitting
        - Tinnitus assessment
      </features>
      <entities>AudiologyVisit, Audiogram, HearingDevice, HearingScreening</entities>
    </audiology>

    <orthopedics>
      <description>Orthopedic specialty module</description>
      <features>
        - Injury tracking
        - Fracture management
        - Joint assessments
        - Cast management
        - Orthopedic imaging
        - Surgery tracking
      </features>
      <entities>OrthoVisit, OrthoInjury, OrthoProcedure, JointAssessment, CastRecord, OrthoImaging</entities>
    </orthopedics>

    <gynecology_obstetrics>
      <description>OB/GYN specialty module</description>
      <features>
        - Pregnancy tracking
        - Prenatal visit management
        - Ultrasound records
        - Gynecological procedures
        - Pap smear tracking
        - Fetal monitoring
      </features>
      <entities>GynVisit, PregnancyRecord, PrenatalVisit, ObUltrasound, GynProcedure, PapSmearRecord</entities>
    </gynecology_obstetrics>

    <psychiatry>
      <description>Mental health specialty module</description>
      <features>
        - Psychological assessments
        - Therapy session management
        - Medication planning
        - Mood tracking
        - Treatment goal setting
        - Progress notes
      </features>
      <entities>MentalHealthVisit, PsychAssessment, TherapySession, PsychMedicationPlan, MoodRecord, TreatmentGoal</entities>
    </psychiatry>

    <gastroenterology>
      <description>Gastroenterology specialty module</description>
      <features>
        - Endoscopy documentation
        - Liver function test tracking
        - GI procedures
        - Digestive condition management
        - Colonoscopy records
      </features>
      <entities>GastroVisit, EndoscopyRecord, LiverFunctionTest, GastroProcedure, DigestiveCondition</entities>
    </gastroenterology>

    <neurology>
      <description>Neurology specialty module</description>
      <features>
        - Neurological examinations
        - EEG recording and analysis
        - Nerve conduction studies
        - Neuro procedures
        - Neurological condition tracking
      </features>
      <entities>NeuroVisit, NeuroExam, EEGRecord, NerveStudy, NeuroProcedure, NeuroCondition</entities>
    </neurology>

    <oncology>
      <description>Oncology specialty module</description>
      <features>
        - Cancer diagnosis and staging
        - Chemotherapy session tracking
        - Radiation therapy records
        - Tumor marker monitoring
        - Treatment planning
        - Prognosis documentation
      </features>
      <entities>OncologyVisit, CancerDiagnosis, ChemotherapySession, RadiationRecord, TumorMarker, OncologyTreatmentPlan</entities>
    </oncology>

    <additional_specialties>
      <fertility>FertilityVisit, IVFCycle, EmbryoRecord, HormoneLevel, SpermAnalysis</fertility>
      <pain_management>PainVisit, PainAssessment, PainProcedure, PainScore, TriggerPointRecord</pain_management>
      <sleep_medicine>SleepVisit, SleepStudy, SleepDiary, CPAPRecord</sleep_medicine>
      <dialysis>DialysisSession, DialysisAccessRecord, DialysisLabResult, FluidBalance</dialysis>
      <chiropractic>ChiroVisit, SpinalAssessment, ChiroAdjustment, PostureAnalysis</chiropractic>
      <podiatry>PodiatryVisit, FootAssessment, PodiatryGaitAnalysis</podiatry>
      <veterinary>Pet, PetOwner, VetVisit, Vaccination, BoardingReservation</veterinary>
    </additional_specialties>
  </specialty_modules>

  <advanced_features>
    <multi_tenancy>
      <description>Complete multi-tenant architecture with data isolation</description>
      <features>
        - Tenant-level data isolation
        - Branch-level filtering (143+ entities)
        - Feature licensing per tenant
        - Customizable terminology per tenant
        - Custom UI schemas per tenant
        - Custom form layouts
        - Custom list layouts
        - Custom navigation
        - Super admin bypass capability
      </features>
      <entities>Tenant, Company, Branch, TenantSettings, CompanySettings, TenantFeature, TenantTerminology, TenantUISchema, TenantFormLayout, TenantListLayout, TenantNavigation, UserBranch</entities>
    </multi_tenancy>

    <rbac>
      <description>Role-Based Access Control</description>
      <features>
        - Role definitions and management
        - Granular permission system
        - Role-permission mapping
        - User-role assignment
        - User-level permission overrides
        - Data access rules for row-level security
      </features>
      <entities>Role, Permission, RolePermission, UserRole, UserPermission, DataAccessRule</entities>
    </rbac>

    <patient_portal>
      <description>Patient self-service portal</description>
      <features>
        - Self-service appointment booking
        - Medical record access
        - Secure messaging with providers
        - Prescription refill requests
        - Online payment portal
        - Document upload
        - Health notifications
        - Notification preferences
      </features>
      <entities>PortalAccount, MessageThread, Message, MessageAttachment, RefillRequest, PatientNotification, NotificationPreferences</entities>
      <api_endpoints>
        - POST /api/portal/register - Register portal account
        - GET /api/portal/appointments - View appointments
        - POST /api/portal/appointments - Book appointment
        - GET /api/portal/records - View medical records
        - GET /api/portal/messages - Get messages
        - POST /api/portal/messages - Send message
        - POST /api/portal/refill-requests - Request refill
        - POST /api/portal/payments - Make payment
      </api_endpoints>
    </patient_portal>

    <push_notifications>
      <description>Mobile and web push notification system</description>
      <features>
        - Device registration and management
        - Topic-based subscriptions
        - Multi-language notification templates
        - Scheduled notifications
        - Delivery tracking and history
        - Push configuration management
      </features>
      <entities>PushDevice, PushNotificationTopic, TopicSubscription, NotificationTemplate, NotificationTemplateLocalization, ScheduledPushNotification, PushNotificationHistoryEntry, PushConfiguration</entities>
    </push_notifications>

    <analytics_reporting>
      <description>Custom analytics and reporting engine</description>
      <features>
        - Custom dashboard creation
        - Dashboard widgets
        - Analytics alerts and rules
        - Anomaly detection
        - Dashboard sharing
        - Report subscriptions
        - Custom report definitions
        - Scheduled reports
        - Report execution history
        - KPI tracking
        - Export functionality (PDF, Excel, CSV)
      </features>
      <entities>AnalyticsDashboard, AnalyticsDashboardWidget, AnalyticsAlert, AnalyticsAlertRule, AnalyticsAnomaly, DashboardShare, DashboardShareLink, AnalyticsDashboardSubscription, CustomReportDefinition, ReportSchedule, SavedReport, ReportExecutionHistoryEntry, ReportWidget, ReportPermission</entities>
      <api_endpoints>
        - GET /api/analytics/dashboards - List dashboards
        - POST /api/analytics/dashboards - Create dashboard
        - GET /api/analytics/dashboards/{id} - Get dashboard
        - POST /api/analytics/dashboards/{id}/widgets - Add widget
        - GET /api/analytics/reports - List reports
        - POST /api/analytics/reports - Create report
        - POST /api/analytics/reports/{id}/execute - Execute report
        - GET /api/analytics/kpis - Get KPIs
        - POST /api/analytics/export - Export data
      </api_endpoints>
    </analytics_reporting>

    <audit_compliance>
      <description>Audit trail and compliance management</description>
      <features>
        - Complete audit trail logging
        - Data retention policies
        - Alert configuration
        - Patient consent management
        - Consent history tracking
        - Consent form templates
        - Backup tracking and management
        - HIPAA compliance features
      </features>
      <entities>AuditLog, AuditRetentionPolicy, AuditAlertConfig, PatientConsent, ConsentHistory, ConsentFormTemplate, BackupRecord</entities>
      <api_endpoints>
        - GET /api/audit/logs - Get audit logs
        - GET /api/audit/logs/entity/{entityType}/{entityId} - Get entity audit
        - GET /api/audit/retention-policies - Get retention policies
        - POST /api/audit/alerts - Configure alerts
        - GET /api/consent/{patientId} - Get patient consents
        - POST /api/consent - Record consent
      </api_endpoints>
    </audit_compliance>

    <security>
      <description>Advanced security features</description>
      <features>
        - API key management
        - Password history enforcement
        - Key rotation tracking
        - Secret storage and encryption
        - Security configuration management
        - Session management
      </features>
      <entities>SecuritySettingsEntity, SecretEntity, ApiKeyEntity, PasswordHistoryEntity, KeyRotationAuditLog</entities>
    </security>

    <calendar_sync>
      <description>External calendar integration</description>
      <features>
        - Google Calendar integration
        - Microsoft Outlook integration
        - Appointment synchronization
        - Conflict detection and resolution
        - OAuth integration
        - Sync history tracking
      </features>
      <entities>CalendarConnection, CalendarSyncSettings, AppointmentCalendarMapping, CalendarSyncHistoryEntry, CalendarSyncConflict, OAuthConfig, OAuthState, OAuthLinkedAccount</entities>
    </calendar_sync>

    <case_management>
      <description>Clinical case and workflow management</description>
      <features>
        - Case creation and tracking
        - Case type definitions
        - Status workflow management
        - Case notes
        - Activity tracking
        - Task management
        - Approval workflows
      </features>
      <entities>Case, CaseType, CaseStatus, CaseNote, CaseActivity</entities>
    </case_management>

    <insurance_management>
      <description>Insurance provider and claim management</description>
      <features>
        - Insurance provider management
        - Insurance plan configurations
        - Patient-insurance mapping
        - Claim submission and tracking
        - Pre-authorization requests
        - Co-payment calculation
        - Eligibility verification
      </features>
      <entities>InsuranceProvider, InsurancePlan, PatientInsurance, InsuranceClaim, InsuranceClaimItem, InsurancePreAuthorization</entities>
    </insurance_management>

    <medical_coding>
      <description>Medical coding for billing</description>
      <features>
        - ICD-10 diagnosis codes
        - CPT procedure codes
        - HCPCS codes
        - Code modifiers
        - Code-based pricing
        - Compliance validation
      </features>
      <entities>ICD10Code, CPTCode, HCPCSCode, MedicalCodeModifier</entities>
    </medical_coding>

    <marketing_crm>
      <description>Marketing and customer relationship management</description>
      <features>
        - Lead management
        - Marketing campaign tracking
        - Activity tracking
        - Lead source analysis
        - Campaign status management
      </features>
      <entities>Lead, Campaign, MarketingActivity, LeadSource, CampaignStatus</entities>
    </marketing_crm>
  </advanced_features>

  <database_schema>
    <overview>
      <total_entities>118+</total_entities>
      <lookup_entities>25+</lookup_entities>
      <multi_tenant_entities>15+</multi_tenant_entities>
      <specialty_module_entities>60+</specialty_module_entities>
      <configuration_entities>10+</configuration_entities>
    </overview>

    <core_tables>
      <multi_tenancy>Tenants, Companies, Branches, TenantSettings, CompanySettings, Features, TenantFeatures</multi_tenancy>
      <patient_clinical>Patients, PatientMedicalHistory, PatientDocuments, PatientAllergies, Appointments, ClinicalVisits, Diagnoses, Procedures, Prescriptions, VitalSigns</patient_clinical>
      <financial>Invoices, Payments, Accounts, Expenses, FinancialTransactions, Vouchers, InsuranceClaims</financial>
      <inventory>InventoryItems, InventoryTransactions, Suppliers, PurchaseOrders, GoodsReceipts</inventory>
      <hr_payroll>Employees, Doctors, Attendance, LeaveRequests, PerformanceReviews, Departments, JobPositions, PayrollPeriods, Payslips</hr_payroll>
      <laboratory>LabTests, LabOrders, LabResults, ExternalLabs</laboratory>
      <radiology>DicomStudies, DicomSeries, DicomInstances, RadiologyReports, PacsServerConfigs</radiology>
      <pharmacy>FormularyDrugs, DrugPricings, ControlledSubstances, DrugAdverseEvents</pharmacy>
    </core_tables>

    <base_entity_fields>
      - Id (GUID primary key)
      - TenantId (for multi-tenancy)
      - BranchId (for branch isolation)
      - CreatedAt (timestamp)
      - UpdatedAt (timestamp)
      - CreatedBy (user reference)
      - UpdatedBy (user reference)
      - IsDeleted (soft delete flag)
    </base_entity_fields>

    <global_filters>
      - Tenant-level filters on 15+ core entities
      - Branch-level filters on 143+ entities
      - Soft delete filtering
      - Super admin bypass capability
    </global_filters>
  </database_schema>

  <api_endpoints_summary>
    <authentication>
      - POST /api/auth/login
      - POST /api/auth/register
      - POST /api/auth/logout
      - POST /api/auth/refresh-token
      - POST /api/auth/forgot-password
      - POST /api/auth/reset-password
      - GET /api/auth/me
      - PUT /api/auth/profile
    </authentication>

    <patients>
      - GET /api/patient
      - POST /api/patient
      - GET /api/patient/{id}
      - PUT /api/patient/{id}
      - DELETE /api/patient/{id}
      - GET /api/patient/{patientId}/medical-history
      - POST /api/patient/{patientId}/medical-history
      - GET /api/patient/{patientId}/documents
      - POST /api/patient/{patientId}/documents
      - GET /api/patient/statistics
      - GET /api/patient/search
    </patients>

    <appointments>
      - GET /api/appointments
      - POST /api/appointments
      - GET /api/appointments/{id}
      - PUT /api/appointments/{id}
      - DELETE /api/appointments/{id}
      - PUT /api/appointments/{id}/status
      - GET /api/appointments/by-doctor/{doctorId}
      - GET /api/appointments/by-patient/{patientId}
      - GET /api/appointments/available-slots
    </appointments>

    <clinical_visits>
      - GET /api/clinical-visits
      - POST /api/clinical-visits
      - GET /api/clinical-visits/{id}
      - PUT /api/clinical-visits/{id}
      - POST /api/clinical-visits/{id}/vitals
      - POST /api/clinical-visits/{id}/diagnoses
      - POST /api/clinical-visits/{id}/prescriptions
      - POST /api/clinical-visits/{id}/procedures
      - POST /api/clinical-visits/{id}/lab-orders
    </clinical_visits>

    <laboratory>
      - GET /api/laboratory/tests
      - POST /api/laboratory/tests
      - GET /api/laboratory/tests/{id}
      - PUT /api/laboratory/tests/{id}
      - GET /api/laboratory/orders
      - POST /api/laboratory/orders
      - GET /api/laboratory/orders/{id}
      - PUT /api/laboratory/orders/{id}/status
      - POST /api/laboratory/orders/{id}/collect-samples
      - GET /api/laboratory/orders/{orderId}/results
      - POST /api/laboratory/results
      - GET /api/laboratory/pending
      - GET /api/laboratory/urgent
    </laboratory>

    <radiology>
      - GET /api/radiology/studies
      - GET /api/radiology/studies/{id}
      - POST /api/radiology/orders
      - POST /api/radiology/dicom/upload
      - GET /api/radiology/reports
      - POST /api/radiology/reports
      - GET /api/radiology/pacs-config
      - PUT /api/radiology/pacs-config
    </radiology>

    <financial>
      - GET /api/financial/accounts
      - POST /api/financial/accounts
      - GET /api/financial/invoices
      - POST /api/financial/invoices
      - PUT /api/financial/invoices/{id}
      - POST /api/financial/invoices/{id}/void
      - POST /api/financial/invoices/{id}/duplicate
      - POST /api/financial/payments
      - GET /api/financial/expenses
      - POST /api/financial/expenses
      - PUT /api/financial/expenses/{id}/approve
      - GET /api/financial/transactions
      - POST /api/financial/claims
      - POST /api/financial/pre-auth
      - GET /api/financial/reports/revenue
      - GET /api/financial/reports/expenses
    </financial>

    <inventory>
      - GET /api/inventory/items
      - POST /api/inventory/items
      - PUT /api/inventory/items/{id}
      - GET /api/inventory/transactions
      - POST /api/inventory/transactions
      - GET /api/inventory/purchase-orders
      - POST /api/inventory/purchase-orders
      - POST /api/inventory/goods-receipts
      - GET /api/inventory/suppliers
      - POST /api/inventory/suppliers
      - GET /api/inventory/low-stock
      - GET /api/inventory/stock-report
    </inventory>

    <hr>
      - GET /api/hr/employees
      - POST /api/hr/employees
      - PUT /api/hr/employees/{id}
      - GET /api/hr/attendance
      - POST /api/hr/attendance/clock-in
      - POST /api/hr/attendance/clock-out
      - GET /api/hr/leave-requests
      - POST /api/hr/leave-requests
      - PUT /api/hr/leave-requests/{id}/approve
      - GET /api/hr/performance-reviews
      - POST /api/hr/performance-reviews
      - GET /api/hr/departments
      - GET /api/hr/positions
    </hr>

    <analytics>
      - GET /api/analytics/dashboards
      - POST /api/analytics/dashboards
      - GET /api/analytics/dashboards/{id}
      - POST /api/analytics/dashboards/{id}/widgets
      - GET /api/analytics/reports
      - POST /api/analytics/reports
      - POST /api/analytics/reports/{id}/execute
      - GET /api/analytics/kpis
      - POST /api/analytics/export
    </analytics>

    <audit>
      - GET /api/audit/logs
      - GET /api/audit/logs/entity/{entityType}/{entityId}
      - GET /api/audit/retention-policies
      - POST /api/audit/alerts
    </audit>

    <patient_portal>
      - POST /api/portal/register
      - GET /api/portal/appointments
      - POST /api/portal/appointments
      - GET /api/portal/records
      - GET /api/portal/messages
      - POST /api/portal/messages
      - POST /api/portal/refill-requests
      - POST /api/portal/payments
    </patient_portal>

    <workflows>
      - GET /api/workflows/cases
      - POST /api/workflows/cases
      - GET /api/workflows/cases/{id}
      - PUT /api/workflows/cases/{id}/status
      - POST /api/workflows/cases/{id}/notes
      - GET /api/workflows/tasks
      - PUT /api/workflows/tasks/{id}
    </workflows>

    <security>
      - GET /api/security/roles
      - POST /api/security/roles
      - PUT /api/security/roles/{id}
      - GET /api/security/permissions
      - POST /api/security/roles/{id}/permissions
      - GET /api/security/users/{id}/permissions
      - PUT /api/security/users/{id}/permissions
    </security>
  </api_endpoints_summary>

  <ui_layout>
    <main_structure>
      - Three-column layout: sidebar (navigation), main (content), detail panel (contextual)
      - Collapsible sidebar with resize handle
      - Responsive breakpoints: mobile (single column), tablet (two column), desktop (three column)
      - Persistent header with tenant/branch selector
      - Breadcrumb navigation
      - Quick actions toolbar
    </main_structure>

    <sidebar_navigation>
      - Dashboard link
      - Patients section
      - Appointments section
      - Clinical section (Visits, Lab, Radiology)
      - Pharmacy section
      - Financial section (Invoices, Payments, Expenses)
      - Inventory section
      - HR section (Employees, Attendance, Leave)
      - Specialty modules (expandable)
      - Reports section
      - Settings at bottom
      - User profile at bottom
    </sidebar_navigation>

    <main_content_area>
      - Page title with breadcrumbs
      - Quick filters and search
      - Data tables with sorting, filtering, pagination
      - Form views for create/edit
      - Detail views with tabs
      - Action buttons and bulk operations
    </main_content_area>

    <shared_components>
      - Badge - Status badges and labels
      - Modal - Dialog boxes and forms
      - DataTable - Sortable, filterable tables with pagination
      - Pagination - Page navigation
      - FormField - Form inputs with validation
      - Toast - Notification toasts
      - ConfirmDialog - Confirmation modals
      - LoadingSkeleton - Loading placeholders
      - EmptyState - Empty state indicators
    </shared_components>
  </ui_layout>

  <design_system>
    <color_palette>
      - Primary: Blue (#3B82F6)
      - Secondary: Gray (#6B7280)
      - Success: Green (#10B981)
      - Warning: Yellow (#F59E0B)
      - Error: Red (#EF4444)
      - Background: White (light mode), Dark gray (#1F2937 dark mode)
      - Surface: Light gray (#F9FAFB light), Darker gray (#374151 dark)
      - Text: Near black (#111827 light), Off-white (#F9FAFB dark)
    </color_palette>

    <typography>
      - Font family: Inter, system-ui, sans-serif
      - Headings: font-semibold
      - Body: font-normal, leading-relaxed
      - Code: Monospace (JetBrains Mono, Consolas)
    </typography>

    <components>
      <buttons>
        - Primary: Blue background, white text, rounded
        - Secondary: Border style with hover fill
        - Icon buttons: Square with hover background
        - Disabled state: Reduced opacity
      </buttons>

      <inputs>
        - Rounded borders with focus ring
        - Label above input
        - Error messages below
        - Required field indicator
      </inputs>

      <cards>
        - Subtle border with shadow
        - Rounded corners (8px)
        - Padding: p-4 to p-6
        - Hover state for clickable
      </cards>

      <tables>
        - Striped rows for readability
        - Sortable column headers
        - Pagination at bottom
        - Row actions on hover
      </tables>
    </components>
  </design_system>

  <deployment>
    <docker_compose>
      <services>
        <postgres>
          - Image: postgres:16
          - Port: 5432
          - Container: xenon-db
          - Volume: postgres_data
        </postgres>
        <redis>
          - Image: redis:7.2
          - Port: 6379
          - Container: xenon-redis
          - Volume: redis_data
        </redis>
        <backend>
          - Container: xenon-api
          - Port: 5000
          - Health check: /health
          - .NET 8 application
        </backend>
        <admin_dashboard>
          - Container: xenon-admin
          - Port: 3000
          - React + Vite
        </admin_dashboard>
        <public_website>
          - Container: xenon-public
          - Port: 3001
          - React + Vite
        </public_website>
      </services>
    </docker_compose>

    <environment_variables>
      - DB_USER: Database username
      - DB_PASSWORD: Database password
      - DB_NAME: Database name
      - JWT_SECRET: JWT signing key
      - JWT_ISSUER: JWT issuer
      - JWT_AUDIENCE: JWT audience
      - VITE_API_URL: API URL for frontend
      - ASPNETCORE_ENVIRONMENT: Development/Production
      - Redis__ConnectionString: Redis connection
    </environment_variables>

    <quick_start>
      # Copy environment file
      cp .env.example .env

      # Start all services
      docker-compose up -d

      # Access services
      # Admin: http://localhost:3000
      # Public: http://localhost:3001
      # API: http://localhost:5000
      # Swagger: http://localhost:5000/swagger
    </quick_start>
  </deployment>

  <default_credentials>
    <admin>
      - Email: admin@xenon.ae
      - Password: Admin@123!
    </admin>
    <note>Change credentials in production!</note>
  </default_credentials>

  <implementation_steps>
    <step number="1">
      <title>Project Foundation and Multi-Tenancy</title>
      <tasks>
        - Set up .NET 8 solution structure
        - Configure EF Core with PostgreSQL
        - Implement multi-tenant architecture
        - Set up tenant and branch isolation
        - Configure JWT authentication
        - Create base entity classes
        - Set up global query filters
      </tasks>
    </step>

    <step number="2">
      <title>Core Patient Management</title>
      <tasks>
        - Create Patient entity and DTOs
        - Implement PatientService
        - Build PatientController with CRUD endpoints
        - Add medical history tracking
        - Implement document management
        - Create patient search functionality
      </tasks>
    </step>

    <step number="3">
      <title>Appointment System</title>
      <tasks>
        - Create Appointment entity and DTOs
        - Implement scheduling logic
        - Build doctor schedule management
        - Add appointment status workflow
        - Create calendar view endpoints
        - Implement availability checking
      </tasks>
    </step>

    <step number="4">
      <title>Clinical Documentation</title>
      <tasks>
        - Create ClinicalVisit entity and workflow
        - Implement vital signs recording
        - Build diagnosis management with ICD-10
        - Create prescription system
        - Implement procedure documentation
        - Add referral management
      </tasks>
    </step>

    <step number="5">
      <title>Laboratory Module</title>
      <tasks>
        - Create lab test catalog
        - Implement order management
        - Build specimen tracking
        - Create result entry system
        - Add critical value alerts
        - Integrate with clinical visits
      </tasks>
    </step>

    <step number="6">
      <title>Financial Management</title>
      <tasks>
        - Create Chart of Accounts
        - Implement invoicing system
        - Build payment processing
        - Create expense management
        - Add insurance claim processing
        - Implement financial reporting
      </tasks>
    </step>

    <step number="7">
      <title>Inventory and HR</title>
      <tasks>
        - Build inventory item management
        - Create purchase order system
        - Implement goods receipt
        - Build employee management
        - Create attendance tracking
        - Implement leave management
      </tasks>
    </step>

    <step number="8">
      <title>Specialty Modules</title>
      <tasks>
        - Implement cardiology module
        - Build dental module with tooth chart
        - Create ophthalmology module
        - Add dermatology module
        - Implement remaining specialties
      </tasks>
    </step>

    <step number="9">
      <title>Advanced Features</title>
      <tasks>
        - Build analytics and reporting
        - Implement audit logging
        - Create patient portal
        - Add push notifications
        - Build workflow engine
        - Implement calendar sync
      </tasks>
    </step>

    <step number="10">
      <title>React Frontend</title>
      <tasks>
        - Set up React with Vite and TypeScript
        - Configure Zustand for state management
        - Build shared UI component library
        - Create module pages
        - Implement responsive design
        - Add Storybook documentation
      </tasks>
    </step>
  </implementation_steps>

  <success_criteria>
    <functionality>
      - All 401+ API endpoints functional
      - Multi-tenancy with complete data isolation
      - All 14+ specialty modules operational
      - Patient lifecycle fully managed
      - Financial operations complete
      - Lab and radiology workflows functional
    </functionality>

    <performance>
      - API response times under 200ms
      - Efficient database queries with EF Core
      - Redis caching implemented
      - Pagination for large datasets
      - Optimized frontend bundle size
    </performance>

    <security>
      - RBAC fully implemented
      - JWT authentication secure
      - Data access rules enforced
      - Audit logging complete
      - HIPAA compliance features
    </security>

    <quality>
      - Clean architecture maintained
      - Unit test coverage adequate
      - E2E tests passing
      - Code quality tools configured
      - Documentation complete
    </quality>
  </success_criteria>
</project_specification>
