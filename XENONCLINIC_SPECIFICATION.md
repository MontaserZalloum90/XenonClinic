<project_specification>
  <project_name>XenonClinic - Multi-Tenant Healthcare ERP System</project_name>

  <overview>
    XenonClinic is a comprehensive, AI-powered, multi-tenant healthcare management system (ERP) designed for clinics,
    hospitals, and healthcare organizations. The system provides complete patient care, clinical documentation,
    financial management, sales and billing, procurement, HR/payroll, inventory control, and support for 22+
    medical specialties. Built with modern clean architecture principles, the platform offers multi-tenancy
    with data isolation, role-based access control, telemedicine support, FHIR/HL7 interoperability, and
    extensive customization capabilities including custom UI schemas and terminology per tenant.

    The platform includes next-generation "wow" features that differentiate it from traditional healthcare ERPs:
    - AI/ML capabilities including predictive analytics, clinical decision support, NLP, and medical image analysis
    - Native mobile applications for patients, providers, and staff with offline capability
    - Real-time collaboration powered by SignalR for live updates and team communication
    - IoT and wearable device integration with remote patient monitoring (RPM)
    - Comprehensive patient engagement tools including digital check-in, queue management, and health goals
    - Advanced analytics and business intelligence with self-service reporting
    - Low-code workflow automation for custom process automation
    - Modern security with biometric authentication, zero-trust architecture, and threat detection
    - Complete integration ecosystem with e-prescribing, claims clearinghouse, and payment processing
    - WCAG 2.1 accessibility compliance and support for 15+ languages including RTL
    - Hospital operations suite including bed management, nursing, OR, and emergency department modules
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
      <api_endpoints>550+ (401 core + 150 wow features)</api_endpoints>
      <database_entities>406+ (306 core + 100 wow features)</database_entities>
      <controllers>35+</controllers>
      <services>80+</services>
      <dtos>850+</dtos>
      <enums>180+</enums>
      <specialty_modules>22+</specialty_modules>
      <formally_defined_modules>10</formally_defined_modules>
      <wow_feature_modules>12</wow_feature_modules>
      <signalr_hubs>8</signalr_hubs>
      <mobile_apps>3 (Patient, Provider, Staff)</mobile_apps>
      <ai_ml_models>10+</ai_ml_models>
      <supported_languages>15</supported_languages>
      <integration_protocols>10+</integration_protocols>
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

    <sales_module>
      <description>Complete sales and billing management</description>
      <features>
        - Sales order creation and management
        - Quotation/estimate generation
        - Quotation to sale conversion
        - Sales confirmation and completion workflow
        - Patient-linked sales tracking
        - Overdue sales monitoring
        - Payment recording and tracking
        - Partial payment support
        - Sales statistics and analytics
        - Discount and pricing management
        - Sales item line management
        - Sales cancellation workflow
        - Sales history and reporting
      </features>
      <entities>
        - Sale - Sales transaction records
        - SaleItem - Individual line items in sales
        - Quotation - Pre-sales estimates/quotes
        - QuotationItem - Quotation line items
        - Payment - Payment records linked to sales
      </entities>
      <enums>
        - SaleStatus (Draft, Confirmed, Completed, Cancelled)
        - QuotationStatus (Draft, Sent, Accepted, Rejected, Expired)
        - PaymentStatus (Pending, Completed, Failed, Refunded)
      </enums>
      <api_endpoints>
        - GET /api/sales - List all sales with filtering
        - GET /api/sales/{id} - Get sale details
        - GET /api/sales/patient/{patientId} - Get patient's sales history
        - GET /api/sales/overdue - Get overdue/unpaid sales
        - POST /api/sales - Create new sale
        - PUT /api/sales/{id}/confirm - Confirm sale
        - PUT /api/sales/{id}/complete - Mark sale as complete
        - PUT /api/sales/{id}/cancel - Cancel sale
        - DELETE /api/sales/{id} - Delete sale
        - GET /api/sales/{id}/payments - Get sale payments
        - POST /api/sales/{id}/payments - Record payment for sale
        - GET /api/sales/quotations - List quotations
        - GET /api/sales/quotations/{id} - Get quotation details
        - POST /api/sales/quotations - Create quotation
        - PUT /api/sales/quotations/{id} - Update quotation
        - POST /api/sales/quotations/{id}/accept - Accept quotation
        - POST /api/sales/quotations/{id}/reject - Reject quotation
        - POST /api/sales/quotations/{id}/convert - Convert quotation to sale
        - GET /api/sales/statistics - Get sales statistics and KPIs
      </api_endpoints>
    </sales_module>

    <procurement_module>
      <description>Complete procurement and purchasing management</description>
      <features>
        - Purchase order creation and management
        - Supplier/vendor management
        - Goods receipt processing
        - Purchase order approval workflow
        - Supplier payment tracking
        - Purchase order status tracking (Draft, Approved, Ordered, Received, Closed)
        - Partial receipt handling
        - External lab management for outsourced services
        - Quotation request from suppliers
        - Price comparison
        - Supplier performance tracking
        - Reorder point management
        - Purchase history and reporting
      </features>
      <entities>
        - PurchaseOrder - Purchase order records
        - PurchaseOrderItem - PO line items
        - GoodsReceipt - Goods receipt records
        - GoodsReceiptItem - Receipt line items
        - Supplier - Vendor/supplier information
        - SupplierPayment - Supplier payment records
        - ExternalLab - External lab partnerships
      </entities>
      <enums>
        - PurchaseOrderStatus (Draft, Approved, Ordered, PartiallyReceived, Received, Closed, Cancelled)
        - GoodsReceiptStatus (Pending, PartiallyReceived, Completed, Rejected)
        - SupplierPaymentStatus (Pending, Paid, PartiallyPaid, Overdue)
      </enums>
      <api_endpoints>
        - GET /api/procurement/purchase-orders - List purchase orders
        - GET /api/procurement/purchase-orders/{id} - Get PO details
        - POST /api/procurement/purchase-orders - Create purchase order
        - PUT /api/procurement/purchase-orders/{id} - Update purchase order
        - PUT /api/procurement/purchase-orders/{id}/approve - Approve PO
        - PUT /api/procurement/purchase-orders/{id}/cancel - Cancel PO
        - DELETE /api/procurement/purchase-orders/{id} - Delete PO
        - GET /api/procurement/goods-receipts - List goods receipts
        - POST /api/procurement/goods-receipts - Create goods receipt
        - PUT /api/procurement/goods-receipts/{id} - Update goods receipt
        - GET /api/procurement/suppliers - List suppliers
        - GET /api/procurement/suppliers/{id} - Get supplier details
        - POST /api/procurement/suppliers - Create supplier
        - PUT /api/procurement/suppliers/{id} - Update supplier
        - DELETE /api/procurement/suppliers/{id} - Delete supplier
        - GET /api/procurement/suppliers/{id}/payments - Get supplier payments
        - POST /api/procurement/suppliers/{id}/payments - Record supplier payment
        - GET /api/procurement/external-labs - List external labs
        - POST /api/procurement/external-labs - Add external lab
      </api_endpoints>
    </procurement_module>
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
      <enums>LeadSource, LeadStatus, CampaignStatus, CampaignType</enums>
    </marketing_crm>

    <communication_notifications>
      <description>Multi-channel communication and notification system</description>
      <features>
        - In-app messaging between patients and providers
        - Message threads with attachments
        - Email notifications
        - SMS notifications
        - WhatsApp integration
        - Push notifications (mobile and web)
        - Notification templates with multi-language support
        - Scheduled notifications
        - Topic-based subscriptions
        - Device registration and management
        - Notification preferences per user
        - Delivery tracking and history
        - Appointment reminders
        - Lab result notifications
        - Prescription refill reminders
      </features>
      <entities>
        - MessageThread - Conversation threads
        - Message - Individual messages
        - MessageAttachment - Message attachments
        - PatientNotification - Notification records
        - NotificationPreferences - User notification settings
        - NotificationTemplate - Notification templates
        - NotificationTemplateLocalization - Multi-language templates
        - ScheduledPushNotification - Scheduled notifications
        - PushNotificationHistoryEntry - Notification history
        - PushDevice - Registered devices
        - PushNotificationTopic - Notification topics
        - TopicSubscription - Topic subscriptions
        - PushConfiguration - Push service configuration
      </entities>
      <services>IPushNotificationService, IEmailService, ISmsService, IWhatsAppService</services>
    </communication_notifications>

    <telemedicine>
      <description>Virtual care and telemedicine support</description>
      <features>
        - Telemedicine appointment scheduling
        - Virtual visit support
        - Doctor telemedicine availability flag
        - Telemedicine meeting links in appointments
        - Video consultation integration
        - Remote patient monitoring support
        - Virtual waiting room concept
        - Telemedicine-specific appointment types
      </features>
      <entities>
        - Doctor.OffersTelemedicine - Doctor telemedicine availability
        - Appointment.IsTelemedicine - Telemedicine appointment flag
        - Appointment.TelemedicineLink - Video call link
      </entities>
    </telemedicine>

    <fhir_hl7_integration>
      <description>Healthcare interoperability standards compliance</description>
      <features>
        - HL7 FHIR R4 compliance
        - FHIR resource mapping
        - Patient resource export
        - Observation resources
        - DiagnosticReport resources
        - Medication resources
        - Encounter resources
        - Practitioner resources
        - Healthcare data exchange
        - Interoperability with external systems
      </features>
      <services>IFhirService</services>
      <standards>HL7 FHIR R4, HL7 v2.x support</standards>
    </fhir_hl7_integration>

    <clinical_decision_support>
      <description>AI-assisted clinical decision support system</description>
      <features>
        - Drug-drug interaction checking
        - Drug-allergy interaction alerts
        - Clinical guidelines recommendations
        - Diagnostic suggestions
        - Treatment protocol guidance
        - Critical value alerts
        - Duplicate therapy detection
        - Dosage recommendations
        - Clinical reminders
        - Evidence-based medicine support
      </features>
      <services>IClinicalDecisionSupportService, IDrugDatabaseService</services>
      <api_endpoints>
        - GET /api/cds/drug-interactions - Check drug interactions
        - GET /api/cds/allergy-check - Check patient allergies
        - GET /api/cds/recommendations - Get clinical recommendations
        - GET /api/cds/guidelines - Get clinical guidelines
      </api_endpoints>
    </clinical_decision_support>

    <consent_management>
      <description>Patient consent and privacy management</description>
      <features>
        - Digital consent form creation
        - Consent form templates
        - Multi-language consent forms
        - Electronic signature capture
        - Consent history tracking
        - Consent expiration management
        - Consent revocation
        - HIPAA compliance support
        - GDPR compliance support
        - Consent audit trail
        - Procedure-specific consents
        - Research consent tracking
      </features>
      <entities>
        - PatientConsent - Patient consent records
        - ConsentHistory - Consent change history
        - ConsentFormTemplate - Reusable consent templates
      </entities>
      <services>IConsentService</services>
      <api_endpoints>
        - GET /api/consent/templates - List consent templates
        - POST /api/consent/templates - Create consent template
        - GET /api/consent/patient/{patientId} - Get patient consents
        - POST /api/consent - Record patient consent
        - PUT /api/consent/{id}/revoke - Revoke consent
        - GET /api/consent/{id}/history - Get consent history
      </api_endpoints>
    </consent_management>

    <oauth_authentication>
      <description>OAuth and multi-provider authentication</description>
      <features>
        - OAuth 2.0 support
        - Multiple identity provider support
        - Google authentication
        - Microsoft/Azure AD authentication
        - Social login integration
        - Dynamic authentication configuration
        - Multi-factor authentication (MFA)
        - Company-specific auth settings
        - OAuth account linking
        - Token management
        - Session management
        - SSO (Single Sign-On) support
      </features>
      <entities>
        - OAuthLinkedAccount - Linked OAuth accounts
        - OAuthConfig - OAuth provider configuration
        - OAuthState - OAuth state tokens
        - CompanyAuthSettings - Company auth configuration
        - CompanyIdentityProvider - Identity provider setup
        - UserMfaConfiguration - User MFA settings
      </entities>
      <services>IOAuthService, IDynamicAuthenticationService, ICompanyAuthConfigService, IMfaService</services>
    </oauth_authentication>

    <backup_disaster_recovery>
      <description>Backup and disaster recovery management</description>
      <features>
        - Automated database backups
        - Scheduled backup jobs
        - Backup validation
        - Point-in-time recovery
        - Backup history tracking
        - Restore capabilities
        - Off-site backup support
        - Backup retention policies
        - Backup encryption
        - Recovery testing
      </features>
      <entities>BackupRecord - Backup history and metadata</entities>
      <services>IBackupService</services>
      <api_endpoints>
        - GET /api/backup/history - Get backup history
        - POST /api/backup/create - Trigger backup
        - POST /api/backup/restore - Restore from backup
        - GET /api/backup/status - Get backup status
      </api_endpoints>
    </backup_disaster_recovery>

    <background_jobs>
      <description>Background job processing and scheduling</description>
      <features>
        - Scheduled job execution
        - Report generation jobs
        - Email notification jobs
        - Data cleanup jobs
        - Sync jobs (calendar, external systems)
        - Job status monitoring
        - Job retry logic
        - Job history tracking
        - Cron-based scheduling
      </features>
      <services>IBackgroundJobService</services>
      <api_endpoints>
        - GET /api/jobs - List scheduled jobs
        - GET /api/jobs/{id} - Get job details
        - POST /api/jobs/{id}/run - Trigger job manually
        - PUT /api/jobs/{id}/pause - Pause job
        - GET /api/jobs/history - Get job execution history
      </api_endpoints>
    </background_jobs>

    <file_storage>
      <description>File and document storage management</description>
      <features>
        - Cloud storage integration
        - Local file storage
        - Document versioning
        - File type validation
        - File size limits
        - Secure file access
        - File encryption at rest
        - Temporary file handling
        - Image optimization
        - PDF generation
      </features>
      <services>IFileStorageService</services>
    </file_storage>

    <localization>
      <description>Multi-language and localization support</description>
      <features>
        - Multi-language UI
        - RTL (Right-to-Left) language support
        - Arabic language support
        - Date/time localization
        - Number formatting
        - Currency formatting
        - Translatable notification templates
        - Per-tenant terminology customization
      </features>
      <services>ILocalizationService</services>
    </localization>

    <theme_customization>
      <description>UI theme and branding customization</description>
      <features>
        - Custom color schemes
        - Logo customization
        - Per-tenant branding
        - Light/dark mode
        - Custom CSS support
        - Component theming
      </features>
      <services>IThemeService</services>
    </theme_customization>

    <license_management>
      <description>License and feature licensing</description>
      <features>
        - Feature-based licensing
        - Module activation/deactivation
        - License validation
        - Usage tracking per license
        - License expiration handling
        - Feature flags
      </features>
      <entities>LicenseConfig, Feature, TenantFeature</entities>
      <services>ILicenseValidator, ILicenseGuardService</services>
    </license_management>
  </advanced_features>

  <wow_features>
    <description>
      Next-generation features that differentiate XenonClinic as a modern, AI-powered healthcare platform.
      These features transform the system from a traditional ERP into an intelligent healthcare ecosystem.
    </description>

    <ai_ml_capabilities>
      <description>Artificial Intelligence and Machine Learning powered features for intelligent healthcare delivery</description>

      <predictive_analytics>
        <description>AI-powered predictions to improve operational efficiency and patient outcomes</description>
        <features>
          - No-show prediction with risk scoring for appointments
          - Readmission risk assessment (30-day, 90-day)
          - Patient deterioration early warning system (NEWS2, MEWS scores)
          - Length of stay prediction for inpatients
          - Resource demand forecasting (staff, beds, equipment)
          - Disease outbreak prediction
          - Revenue forecasting with trend analysis
          - Patient churn prediction
          - Treatment outcome prediction
          - Medication adherence prediction
        </features>
        <entities>
          - PredictionModel - ML model configurations
          - PredictionResult - Prediction outputs and scores
          - RiskScore - Patient risk assessments
          - ForecastData - Demand and resource forecasts
          - ModelTrainingJob - ML training job tracking
          - FeatureStore - ML feature storage
        </entities>
        <api_endpoints>
          - GET /api/ai/predictions/no-show/{appointmentId} - Predict no-show probability
          - GET /api/ai/predictions/readmission/{patientId} - Get readmission risk
          - GET /api/ai/predictions/deterioration/{patientId} - Get deterioration risk
          - GET /api/ai/forecasts/demand - Get resource demand forecast
          - GET /api/ai/forecasts/revenue - Get revenue forecast
          - POST /api/ai/models/train - Trigger model training
          - GET /api/ai/models - List available models
        </api_endpoints>
      </predictive_analytics>

      <clinical_ai>
        <description>AI-assisted clinical decision making and diagnosis support</description>
        <features>
          - Differential diagnosis suggestions based on symptoms
          - AI-powered symptom checker for patients
          - Treatment recommendation engine
          - Drug dosage optimization suggestions
          - Lab result interpretation assistance
          - Radiology image AI analysis (X-ray, CT, MRI abnormality detection)
          - Pathology slide analysis
          - ECG/EKG interpretation assistance
          - Skin lesion analysis for dermatology
          - Retinal scan analysis for ophthalmology
          - Clinical pathway recommendations
          - Evidence-based medicine suggestions
          - Comorbidity risk analysis
          - Genetic risk factor analysis
        </features>
        <entities>
          - DiagnosisSuggestion - AI-generated diagnosis suggestions
          - TreatmentRecommendation - AI treatment recommendations
          - ImageAnalysisResult - Medical image AI analysis
          - SymptomAnalysis - Symptom checker results
          - ClinicalPathway - Clinical pathway definitions
          - AIAssistantSession - AI consultation sessions
        </entities>
        <api_endpoints>
          - POST /api/ai/clinical/symptoms - Analyze symptoms
          - POST /api/ai/clinical/diagnosis - Get diagnosis suggestions
          - POST /api/ai/clinical/treatment - Get treatment recommendations
          - POST /api/ai/imaging/analyze - Analyze medical images
          - POST /api/ai/lab/interpret - Interpret lab results
          - POST /api/ai/ecg/analyze - Analyze ECG data
          - GET /api/ai/clinical/pathways/{conditionId} - Get clinical pathways
        </api_endpoints>
      </clinical_ai>

      <natural_language_processing>
        <description>NLP capabilities for clinical documentation and communication</description>
        <features>
          - Voice-to-text clinical note dictation
          - Real-time speech transcription during consultations
          - Automatic medical coding from clinical narratives (ICD-10, CPT)
          - Clinical note summarization
          - Key information extraction from documents
          - Sentiment analysis for patient feedback
          - Multi-language medical translation
          - Medical terminology normalization
          - Named entity recognition for medications, conditions, procedures
          - Patient communication analysis
          - Automated clinical letter generation
          - Smart auto-complete for clinical notes
        </features>
        <entities>
          - TranscriptionJob - Voice transcription jobs
          - TranscriptionResult - Transcription outputs
          - NLPExtraction - Extracted entities from text
          - AutoCodeSuggestion - Auto-generated medical codes
          - ClinicalSummary - AI-generated summaries
          - TranslationRequest - Medical translation requests
        </entities>
        <api_endpoints>
          - POST /api/ai/nlp/transcribe - Transcribe audio to text
          - POST /api/ai/nlp/extract - Extract entities from text
          - POST /api/ai/nlp/summarize - Summarize clinical notes
          - POST /api/ai/nlp/autocode - Generate medical codes from text
          - POST /api/ai/nlp/translate - Translate medical content
          - GET /api/ai/nlp/autocomplete - Get smart autocomplete suggestions
        </api_endpoints>
      </natural_language_processing>

      <smart_scheduling>
        <description>AI-optimized scheduling and resource allocation</description>
        <features>
          - Optimal appointment slot recommendations
          - Dynamic schedule optimization based on patient needs
          - No-show overbooking intelligence
          - Staff schedule optimization
          - OR/procedure room scheduling optimization
          - Equipment utilization optimization
          - Wait time prediction and minimization
          - Patient preference learning
          - Travel time consideration for home visits
          - Buffer time optimization between appointments
        </features>
        <entities>
          - ScheduleOptimization - Optimization configurations
          - OptimalSlot - AI-recommended time slots
          - ScheduleScore - Schedule quality scores
          - PatientPreference - Learned patient preferences
        </entities>
        <api_endpoints>
          - GET /api/ai/scheduling/optimal-slots - Get AI-recommended slots
          - POST /api/ai/scheduling/optimize - Optimize schedule
          - GET /api/ai/scheduling/wait-time - Predict wait time
          - POST /api/ai/scheduling/staff-optimize - Optimize staff schedule
        </api_endpoints>
      </smart_scheduling>

      <virtual_health_assistant>
        <description>AI-powered conversational assistant for patients and staff</description>
        <features>
          - 24/7 patient chatbot for common queries
          - Symptom triage and urgency assessment
          - Appointment booking via conversation
          - Medication reminders and adherence support
          - Post-visit follow-up conversations
          - Pre-visit preparation guidance
          - Insurance and billing query assistance
          - Health education and information
          - Mental health check-ins
          - Chronic disease management coaching
          - Staff assistant for quick information lookup
          - Multi-channel support (web, mobile, WhatsApp, voice)
        </features>
        <entities>
          - ChatSession - Conversation sessions
          - ChatMessage - Individual messages
          - ChatIntent - Recognized intents
          - ChatContext - Conversation context
          - AssistantPersona - Customizable assistant personas
          - ConversationFlow - Conversation flow definitions
        </entities>
        <api_endpoints>
          - POST /api/ai/assistant/chat - Send message to assistant
          - GET /api/ai/assistant/sessions - Get chat sessions
          - GET /api/ai/assistant/sessions/{id}/history - Get chat history
          - POST /api/ai/assistant/triage - Perform symptom triage
          - PUT /api/ai/assistant/persona - Configure assistant persona
        </api_endpoints>
      </virtual_health_assistant>
    </ai_ml_capabilities>

    <mobile_applications>
      <description>Native mobile applications for patients, staff, and providers</description>

      <patient_mobile_app>
        <description>Feature-rich mobile application for patients</description>
        <platforms>iOS (Swift/SwiftUI), Android (Kotlin/Jetpack Compose)</platforms>
        <features>
          - Biometric login (Face ID, Touch ID, Fingerprint)
          - Appointment booking and management
          - Virtual waiting room for telemedicine
          - Video consultation with providers
          - Secure messaging with care team
          - Prescription viewing and refill requests
          - Lab results with trend visualization
          - Medical records access
          - Health vitals logging (manual and device sync)
          - Medication reminders with notifications
          - Symptom checker and triage
          - Digital check-in via QR code
          - Bill payment and invoice viewing
          - Insurance card storage
          - Health goals and progress tracking
          - Family member account management
          - Emergency contact quick dial
          - Clinic/hospital finder with navigation
          - Push notifications for appointments, results, messages
          - Dark mode support
          - Offline access to key information
          - Wearable device companion app
        </features>
        <offline_capabilities>
          - View upcoming appointments
          - Access downloaded medical records
          - View medication list
          - Log symptoms and vitals (sync when online)
          - View emergency contact information
          - Access health education content
        </offline_capabilities>
      </patient_mobile_app>

      <provider_mobile_app>
        <description>Mobile application for doctors and healthcare providers</description>
        <platforms>iOS, Android</platforms>
        <features>
          - Today's schedule at a glance
          - Patient list and quick lookup
          - Voice dictation for clinical notes
          - Clinical decision support alerts
          - Lab and imaging result review
          - Prescription writing with e-signature
          - Secure messaging with patients
          - Colleague consultation requests
          - Task and to-do management
          - On-call schedule management
          - Push notifications for urgent items
          - Quick patient check-in/out
          - Telemedicine video calls
          - Medical reference lookup
          - CPT/ICD code lookup
          - Drug interaction checker
          - Medical calculator tools
          - Photo capture for clinical documentation
          - Offline patient summary access
        </features>
      </provider_mobile_app>

      <staff_mobile_app>
        <description>Mobile application for clinical and administrative staff</description>
        <platforms>iOS, Android</platforms>
        <features>
          - Clock in/out with location verification
          - Task assignment notifications
          - Patient transport tracking
          - Equipment/asset scanning
          - Inventory quick lookup
          - Urgent message alerts
          - Schedule and shift viewing
          - Leave request submission
          - Incident reporting
          - Quick patient lookup
          - Appointment status updates
          - Room/bed status updates
          - Housekeeping task management
          - Maintenance request submission
        </features>
      </staff_mobile_app>

      <mobile_infrastructure>
        <features>
          - Push notification service (APNs, FCM)
          - Deep linking for direct navigation
          - App configuration management
          - Feature flags per app version
          - Crash reporting and analytics
          - A/B testing framework
          - App update prompts
          - Remote configuration
          - Secure local storage
          - Certificate pinning
          - Jailbreak/root detection
          - Screen capture prevention for sensitive data
        </features>
        <entities>
          - MobileDevice - Registered devices
          - MobileSession - Active sessions
          - AppVersion - Version management
          - FeatureFlag - Feature toggles
          - MobileAnalytics - Usage analytics
        </entities>
        <api_endpoints>
          - POST /api/mobile/register - Register device
          - POST /api/mobile/push/token - Update push token
          - GET /api/mobile/config - Get app configuration
          - GET /api/mobile/features - Get feature flags
          - POST /api/mobile/analytics - Submit analytics
          - POST /api/mobile/crash - Report crash
        </api_endpoints>
      </mobile_infrastructure>
    </mobile_applications>

    <realtime_collaboration>
      <description>Real-time communication and collaboration features powered by SignalR</description>

      <live_updates>
        <features>
          - Real-time appointment status changes
          - Live queue position updates
          - Instant notification delivery
          - Real-time dashboard updates
          - Live patient vital sign streaming
          - Concurrent user presence indicators
          - Real-time inventory alerts
          - Live OR/procedure status board
          - Emergency broadcast notifications
          - Real-time bed availability updates
        </features>
      </live_updates>

      <collaborative_documentation>
        <features>
          - Multi-user clinical note editing
          - Real-time cursor presence
          - Change tracking and attribution
          - Conflict resolution
          - Version history with diff view
          - Comment and annotation threads
          - @mention notifications
          - Document locking for exclusive edits
          - Auto-save with sync status
        </features>
        <entities>
          - DocumentSession - Active editing sessions
          - DocumentChange - Change events
          - DocumentComment - Comments and annotations
          - DocumentVersion - Version snapshots
          - UserPresence - Active user tracking
        </entities>
      </collaborative_documentation>

      <team_communication>
        <features>
          - Instant messaging between staff
          - Group chat for care teams
          - Secure HIPAA-compliant messaging
          - Message read receipts
          - File and image sharing
          - Voice messages
          - Message search
          - Pinned important messages
          - Message reactions
          - Typing indicators
          - Online/offline status
          - Do not disturb settings
          - Message retention policies
          - Emergency broadcast channel
        </features>
        <entities>
          - TeamChat - Team chat rooms
          - DirectMessage - Direct messages
          - ChatMember - Chat participants
          - MessageReaction - Message reactions
          - BroadcastMessage - Emergency broadcasts
        </entities>
        <api_endpoints>
          - GET /api/chat/rooms - List chat rooms
          - POST /api/chat/rooms - Create chat room
          - GET /api/chat/rooms/{id}/messages - Get messages
          - POST /api/chat/rooms/{id}/messages - Send message
          - GET /api/chat/direct/{userId} - Get direct messages
          - POST /api/chat/direct/{userId} - Send direct message
          - POST /api/chat/broadcast - Send broadcast
        </api_endpoints>
      </team_communication>

      <video_conferencing>
        <description>Integrated video calling for telemedicine and team meetings</description>
        <integration_options>Twilio, Vonage, Zoom SDK, WebRTC native</integration_options>
        <features>
          - One-click video calls from appointment
          - Virtual waiting room with position
          - Screen sharing for education
          - Recording with consent
          - Waiting room customization
          - Multiple participant support
          - Chat during video call
          - File sharing during call
          - Virtual backgrounds
          - Bandwidth adaptation
          - Call quality monitoring
          - Automatic reconnection
          - Mobile-to-desktop handoff
          - Interpreter service integration
        </features>
        <entities>
          - VideoSession - Video call sessions
          - VideoRecording - Call recordings
          - VideoParticipant - Call participants
          - WaitingRoomEntry - Waiting room queue
        </entities>
        <api_endpoints>
          - POST /api/video/sessions - Create video session
          - GET /api/video/sessions/{id}/join - Join video session
          - POST /api/video/sessions/{id}/end - End session
          - GET /api/video/sessions/{id}/recording - Get recording
          - GET /api/video/waiting-room - Get waiting room status
        </api_endpoints>
      </video_conferencing>

      <signalr_hubs>
        <description>SignalR hub definitions for real-time features</description>
        <hubs>
          - NotificationHub - Real-time notifications
          - AppointmentHub - Appointment updates
          - QueueHub - Queue position updates
          - ChatHub - Team messaging
          - DocumentHub - Collaborative editing
          - VitalsHub - Live vital sign streaming
          - DashboardHub - Dashboard data updates
          - AlertHub - Critical alerts
        </hubs>
      </signalr_hubs>
    </realtime_collaboration>

    <iot_wearable_integration>
      <description>Integration with IoT medical devices and consumer wearables</description>

      <medical_device_integration>
        <description>Integration with clinical-grade medical devices</description>
        <supported_devices>
          - Vital sign monitors (Philips, GE, Mindray)
          - Blood pressure monitors
          - Pulse oximeters
          - Glucometers
          - Weight scales
          - Thermometers
          - ECG/EKG devices
          - Spirometers
          - Peak flow meters
          - Continuous glucose monitors (CGM)
          - Holter monitors
          - Smart infusion pumps
          - Ventilators
          - Fetal monitors
        </supported_devices>
        <features>
          - Automatic vital sign capture
          - Device pairing and configuration
          - Real-time data streaming
          - Alert thresholds configuration
          - Device calibration tracking
          - Data validation and quality checks
          - Historical trend analysis
          - Integration with clinical visits
          - Device firmware management
        </features>
        <protocols>
          - Bluetooth Low Energy (BLE)
          - Wi-Fi direct
          - USB serial
          - HL7 ADT messages
          - IEEE 11073 (Personal Health Devices)
          - FHIR Device resources
        </protocols>
      </medical_device_integration>

      <consumer_wearable_integration>
        <description>Integration with consumer health wearables and apps</description>
        <supported_platforms>
          - Apple Health (HealthKit)
          - Google Fit (Health Connect)
          - Samsung Health
          - Fitbit
          - Garmin
          - Withings
          - Oura Ring
          - Whoop
          - Dexcom CGM
          - Abbott FreeStyle Libre
        </supported_platforms>
        <data_types>
          - Steps and activity
          - Heart rate and HRV
          - Sleep patterns
          - Blood oxygen (SpO2)
          - Blood pressure
          - Weight and body composition
          - Blood glucose
          - Menstrual cycle
          - Respiratory rate
          - ECG readings
          - Temperature
          - Stress levels
        </data_types>
        <features>
          - OAuth-based connection
          - Automatic background sync
          - Data aggregation and trending
          - Anomaly detection
          - Provider dashboard for review
          - Patient data sharing consent
          - Data quality indicators
          - Integration with care plans
        </features>
        <entities>
          - WearableConnection - Connected devices/apps
          - WearableData - Synced health data
          - WearableSync - Sync job history
          - DataConsent - Patient consent for sharing
        </entities>
        <api_endpoints>
          - GET /api/wearables/connections - List connections
          - POST /api/wearables/connect/{platform} - Initiate connection
          - DELETE /api/wearables/connections/{id} - Disconnect
          - GET /api/wearables/data/{patientId} - Get patient wearable data
          - POST /api/wearables/sync - Trigger manual sync
        </api_endpoints>
      </consumer_wearable_integration>

      <remote_patient_monitoring>
        <description>Continuous remote monitoring for chronic disease and post-acute care</description>
        <features>
          - Monitoring program enrollment
          - Device kit assignment and shipping
          - Automated measurement reminders
          - Real-time threshold alerts
          - Care team escalation workflows
          - Patient compliance tracking
          - Trending and analytics dashboard
          - Automated documentation for billing (CPT 99453-99458)
          - Video check-in integration
          - Care plan adjustments based on data
          - Medicare CCM/RPM program support
          - Family member visibility
        </features>
        <conditions>
          - Hypertension monitoring
          - Diabetes management
          - Heart failure management
          - COPD monitoring
          - Post-surgical recovery
          - Pregnancy monitoring
          - Weight management
          - Cardiac rehabilitation
        </conditions>
        <entities>
          - RPMProgram - Monitoring program definitions
          - RPMEnrollment - Patient enrollments
          - RPMDeviceKit - Device kits
          - RPMReading - Monitoring readings
          - RPMAlert - Threshold alerts
          - RPMTimeLog - Time tracking for billing
        </entities>
        <api_endpoints>
          - GET /api/rpm/programs - List RPM programs
          - POST /api/rpm/enroll - Enroll patient
          - GET /api/rpm/patients/{id}/readings - Get patient readings
          - GET /api/rpm/alerts - Get active alerts
          - POST /api/rpm/alerts/{id}/acknowledge - Acknowledge alert
          - GET /api/rpm/dashboard - Get monitoring dashboard
          - GET /api/rpm/billing - Get RPM billing data
        </api_endpoints>
      </remote_patient_monitoring>
    </iot_wearable_integration>

    <patient_engagement>
      <description>Features to improve patient engagement, satisfaction, and health outcomes</description>

      <digital_front_door>
        <features>
          - Online self-scheduling
          - Digital intake forms
          - Insurance verification
          - Cost estimation before visit
          - Document upload before visit
          - Pre-visit questionnaires
          - Appointment preparation instructions
          - Location and parking guidance
          - Wait time visibility
          - SMS appointment confirmations
          - Calendar integration (Google, Apple, Outlook)
        </features>
      </digital_front_door>

      <digital_check_in>
        <features>
          - QR code check-in at arrival
          - Kiosk check-in stations
          - Mobile check-in (start 24 hours before)
          - Consent form e-signature
          - Co-pay collection at check-in
          - Updated information verification
          - Photo ID verification
          - Insurance card scanning
          - Queue position after check-in
          - SMS notification when provider ready
        </features>
        <entities>
          - CheckInSession - Check-in attempts
          - CheckInKiosk - Kiosk configurations
          - ConsentSignature - E-signatures
        </entities>
        <api_endpoints>
          - POST /api/checkin/start - Start check-in
          - POST /api/checkin/verify - Verify identity
          - POST /api/checkin/consent - Submit consent
          - POST /api/checkin/payment - Process payment
          - POST /api/checkin/complete - Complete check-in
          - GET /api/checkin/queue-position - Get queue position
        </api_endpoints>
      </digital_check_in>

      <patient_education>
        <features>
          - Condition-specific education library
          - Post-visit care instructions
          - Medication education materials
          - Procedure preparation videos
          - Interactive health assessments
          - Personalized content recommendations
          - Multi-language support
          - Accessibility features (audio, large text)
          - Bookmarking and favorites
          - Provider-assigned content
          - Progress tracking
        </features>
        <entities>
          - EducationContent - Educational materials
          - ContentCategory - Content categorization
          - PatientEducationAssignment - Assigned content
          - ContentProgress - Progress tracking
        </entities>
      </patient_education>

      <health_goals_wellness>
        <features>
          - Personal health goal setting
          - Progress tracking and visualization
          - Achievement badges and rewards
          - Social challenges (opt-in)
          - Care plan goal integration
          - Provider goal prescriptions
          - Reminder and motivation notifications
          - Health coaching integration
          - Weight management programs
          - Smoking cessation tracking
          - Exercise and activity goals
          - Nutrition tracking
          - Mental wellness check-ins
        </features>
        <entities>
          - HealthGoal - Patient health goals
          - GoalProgress - Progress entries
          - Achievement - Earned achievements
          - Challenge - Wellness challenges
          - ChallengeParticipant - Challenge participants
        </entities>
        <api_endpoints>
          - GET /api/wellness/goals - Get patient goals
          - POST /api/wellness/goals - Create goal
          - POST /api/wellness/goals/{id}/progress - Log progress
          - GET /api/wellness/achievements - Get achievements
          - GET /api/wellness/challenges - List challenges
          - POST /api/wellness/challenges/{id}/join - Join challenge
        </api_endpoints>
      </health_goals_wellness>

      <patient_feedback>
        <features>
          - Post-visit satisfaction surveys
          - Net Promoter Score (NPS) tracking
          - Provider rating and reviews
          - Facility rating
          - Real-time feedback alerts
          - Trend analysis and reporting
          - Response management
          - Public review site monitoring
          - Complaint tracking and resolution
          - Service recovery workflows
        </features>
        <entities>
          - SatisfactionSurvey - Survey definitions
          - SurveyResponse - Patient responses
          - ProviderRating - Provider ratings
          - FeedbackAlert - Negative feedback alerts
          - Complaint - Complaint records
        </entities>
        <api_endpoints>
          - GET /api/feedback/surveys - Get available surveys
          - POST /api/feedback/responses - Submit survey response
          - GET /api/feedback/nps - Get NPS scores
          - GET /api/feedback/ratings/{providerId} - Get provider ratings
          - POST /api/feedback/complaints - Submit complaint
        </api_endpoints>
      </patient_feedback>

      <family_caregiver_access>
        <features>
          - Proxy account creation
          - Relationship verification
          - Age-appropriate access rules
          - Care circle management
          - Shared calendar access
          - Caregiver notifications
          - Power of Attorney documentation
          - Emergency contact updates
          - Pediatric guardian management
          - Elder care coordination
        </features>
        <entities>
          - CareCircle - Patient care network
          - ProxyAccess - Proxy account links
          - AccessAuthorization - Access permissions
        </entities>
      </family_caregiver_access>
    </patient_engagement>

    <advanced_analytics_bi>
      <description>Business intelligence and advanced analytics for data-driven decisions</description>

      <executive_dashboards>
        <features>
          - Real-time KPI monitoring
          - Revenue and volume trends
          - Provider productivity metrics
          - Patient satisfaction scores
          - Quality measure performance
          - Financial performance indicators
          - Operational efficiency metrics
          - Drill-down capabilities
          - Customizable dashboard layouts
          - Scheduled email reports
          - Mobile dashboard access
        </features>
      </executive_dashboards>

      <population_health>
        <features>
          - Patient population segmentation
          - Risk stratification
          - Care gap identification
          - Chronic disease registries
          - Quality measure tracking (HEDIS, MIPS)
          - Preventive care reminders
          - High-risk patient identification
          - Care management outreach lists
          - Social determinants of health tracking
          - Health equity analysis
        </features>
        <entities>
          - PopulationCohort - Patient cohorts
          - RiskStratification - Risk categories
          - CareGap - Identified care gaps
          - QualityMeasure - Quality metrics
          - OutreachCampaign - Outreach campaigns
        </entities>
        <api_endpoints>
          - GET /api/population/cohorts - List cohorts
          - POST /api/population/cohorts - Create cohort
          - GET /api/population/risk-stratification - Get risk stratification
          - GET /api/population/care-gaps - Get care gaps
          - GET /api/population/quality-measures - Get quality measures
        </api_endpoints>
      </population_health>

      <financial_analytics>
        <features>
          - Revenue cycle analytics
          - Denial management insights
          - Payer mix analysis
          - Procedure profitability
          - Provider revenue tracking
          - Collection rate analysis
          - Days in A/R tracking
          - Bad debt prediction
          - Contract performance analysis
          - Cost per encounter analysis
          - Budget vs actual comparison
          - Financial forecasting
        </features>
      </financial_analytics>

      <operational_analytics>
        <features>
          - Appointment utilization analysis
          - Wait time analytics
          - Patient flow analysis
          - Resource utilization
          - Staff productivity
          - Equipment utilization
          - Room/bed turnover
          - Bottleneck identification
          - Capacity planning
          - Demand forecasting
          - No-show analysis
          - Cancellation analysis
        </features>
      </operational_analytics>

      <clinical_analytics>
        <features>
          - Outcome tracking by condition
          - Readmission analysis
          - Complication rates
          - Mortality analysis
          - Length of stay analysis
          - Clinical variation analysis
          - Best practice identification
          - Comparative effectiveness
          - Drug utilization review
          - Antibiotic stewardship metrics
        </features>
      </clinical_analytics>

      <benchmarking>
        <features>
          - Industry benchmark comparison
          - Peer group comparison
          - Regional comparison
          - National standards
          - Custom benchmark creation
          - Trend analysis
          - Percentile rankings
          - Improvement tracking
        </features>
        <entities>
          - Benchmark - Benchmark definitions
          - BenchmarkData - Benchmark values
          - BenchmarkComparison - Comparison results
        </entities>
      </benchmarking>

      <self_service_analytics>
        <features>
          - Drag-and-drop report builder
          - Ad-hoc query builder
          - Data exploration tools
          - Custom visualization creation
          - Report scheduling
          - Report sharing
          - Export to Excel, PDF, CSV
          - Embedded analytics
          - Natural language queries
          - Automated insights
        </features>
      </self_service_analytics>
    </advanced_analytics_bi>

    <workflow_automation>
      <description>Intelligent workflow automation to reduce manual tasks and improve efficiency</description>

      <low_code_workflow_builder>
        <description>Visual workflow designer for custom process automation</description>
        <features>
          - Drag-and-drop workflow canvas
          - Pre-built workflow templates
          - Conditional branching logic
          - Loop and iteration support
          - Timer and schedule triggers
          - Event-based triggers
          - Human task assignment
          - Approval workflows
          - Notification actions
          - API call actions
          - Data transformation
          - Error handling
          - Workflow versioning
          - Testing and simulation
          - Audit trail
        </features>
        <workflow_types>
          - Approval workflows (expenses, leave, POs)
          - Patient journey workflows
          - Referral workflows
          - Prior authorization workflows
          - Lab result follow-up
          - Appointment reminder sequences
          - Onboarding workflows
          - Discharge workflows
          - Quality review workflows
        </workflow_types>
        <entities>
          - WorkflowDefinition - Workflow configurations
          - WorkflowInstance - Running workflows
          - WorkflowStep - Workflow steps
          - WorkflowTask - Human tasks
          - WorkflowTrigger - Trigger configurations
          - WorkflowHistory - Execution history
        </entities>
        <api_endpoints>
          - GET /api/workflows/definitions - List workflows
          - POST /api/workflows/definitions - Create workflow
          - PUT /api/workflows/definitions/{id} - Update workflow
          - POST /api/workflows/definitions/{id}/publish - Publish workflow
          - GET /api/workflows/instances - List instances
          - POST /api/workflows/trigger - Trigger workflow
          - GET /api/workflows/tasks - Get pending tasks
          - POST /api/workflows/tasks/{id}/complete - Complete task
        </api_endpoints>
      </low_code_workflow_builder>

      <robotic_process_automation>
        <description>Automated handling of repetitive administrative tasks</description>
        <automations>
          - Eligibility verification
          - Prior authorization submission
          - Claim scrubbing and submission
          - Payment posting
          - Appointment reminder calls
          - Patient statement generation
          - Report distribution
          - Data entry from faxes/documents
          - Insurance card data extraction
          - Referral faxing
          - Lab order transmission
        </automations>
        <features>
          - Bot scheduling
          - Exception handling
          - Human-in-the-loop review
          - Performance monitoring
          - Error reporting
          - Audit logging
        </features>
      </robotic_process_automation>

      <smart_rules_engine>
        <description>Configurable business rules for automated decision making</description>
        <features>
          - Visual rule builder
          - Decision tables
          - Rule prioritization
          - Rule versioning
          - Rule testing
          - Real-time evaluation
          - Rule analytics
          - Exception management
        </features>
        <rule_categories>
          - Scheduling rules (appointment types, durations)
          - Billing rules (pricing, discounts)
          - Clinical rules (alerts, reminders)
          - Access rules (permissions)
          - Notification rules
          - Escalation rules
        </rule_categories>
        <entities>
          - BusinessRule - Rule definitions
          - RuleSet - Rule groupings
          - RuleExecution - Execution history
          - RuleException - Rule exceptions
        </entities>
      </smart_rules_engine>

      <task_management>
        <features>
          - Task creation and assignment
          - Due date and priority
          - Task templates
          - Recurring tasks
          - Task dependencies
          - Team task views
          - Personal task lists
          - Task comments
          - File attachments
          - Task timers
          - Kanban board view
          - Calendar view
          - Overdue task alerts
          - Task delegation
        </features>
        <entities>
          - Task - Task records
          - TaskTemplate - Reusable templates
          - TaskAssignment - Task assignments
          - TaskComment - Task comments
        </entities>
        <api_endpoints>
          - GET /api/tasks - List tasks
          - POST /api/tasks - Create task
          - PUT /api/tasks/{id} - Update task
          - PUT /api/tasks/{id}/complete - Complete task
          - PUT /api/tasks/{id}/assign - Assign task
          - GET /api/tasks/my-tasks - Get my tasks
          - GET /api/tasks/team - Get team tasks
        </api_endpoints>
      </task_management>
    </workflow_automation>

    <modern_security>
      <description>Advanced security features for protecting sensitive healthcare data</description>

      <biometric_authentication>
        <features>
          - Fingerprint authentication
          - Face ID / facial recognition
          - Voice recognition
          - Behavioral biometrics
          - Multi-factor authentication (MFA)
          - Adaptive authentication based on risk
          - Hardware token support (YubiKey)
          - TOTP authenticator apps
          - SMS/email OTP fallback
          - Passwordless authentication option
          - Remember trusted devices
          - Step-up authentication for sensitive actions
        </features>
        <entities>
          - BiometricCredential - Registered biometrics
          - MFAConfiguration - MFA settings
          - TrustedDevice - Trusted devices
          - AuthenticationAttempt - Login attempts
        </entities>
      </biometric_authentication>

      <zero_trust_architecture>
        <features>
          - Never trust, always verify
          - Continuous authentication
          - Micro-segmentation
          - Least privilege access
          - Device trust verification
          - Network location awareness
          - Real-time access decisions
          - Session monitoring
          - Anomaly-based access revocation
        </features>
      </zero_trust_architecture>

      <threat_detection>
        <features>
          - Real-time threat monitoring
          - Behavioral analytics
          - Anomaly detection
          - Failed login tracking
          - Brute force protection
          - Geographic anomaly detection
          - Time-based anomaly detection
          - Data exfiltration detection
          - Insider threat detection
          - Security incident alerting
          - SIEM integration
          - Automated threat response
        </features>
        <entities>
          - SecurityIncident - Detected incidents
          - ThreatAlert - Threat alerts
          - BehaviorBaseline - Normal behavior patterns
          - AnomalyDetection - Detected anomalies
        </entities>
        <api_endpoints>
          - GET /api/security/incidents - List incidents
          - GET /api/security/threats - Get active threats
          - POST /api/security/incidents/{id}/resolve - Resolve incident
          - GET /api/security/analytics - Get security analytics
        </api_endpoints>
      </threat_detection>

      <data_protection>
        <features>
          - End-to-end encryption
          - Data masking for sensitive fields
          - Field-level encryption
          - Encryption key management
          - Data loss prevention (DLP)
          - Secure data disposal
          - Data anonymization for research
          - Pseudonymization
          - Secure file sharing
          - Watermarking for documents
          - Screen capture prevention
          - Print protection
        </features>
      </data_protection>

      <blockchain_audit>
        <description>Immutable audit trail using blockchain technology</description>
        <features>
          - Tamper-proof audit records
          - Distributed ledger for critical events
          - Cryptographic verification
          - Audit trail integrity verification
          - Compliance evidence
          - Legal hold support
          - Third-party verification capability
        </features>
        <use_cases>
          - Prescription records
          - Controlled substance dispensing
          - Consent records
          - Medical record access logs
          - Financial transactions
          - Identity verification events
        </use_cases>
      </blockchain_audit>

      <privacy_compliance>
        <features>
          - HIPAA compliance automation
          - GDPR compliance tools
          - Privacy impact assessments
          - Data subject access requests (DSAR)
          - Right to be forgotten implementation
          - Consent management
          - Privacy policy management
          - Data processing agreements
          - Vendor risk assessment
          - Compliance reporting
          - Automated compliance scanning
        </features>
        <entities>
          - PrivacyRequest - DSAR requests
          - DataProcessingRecord - Processing activities
          - VendorAssessment - Vendor evaluations
          - ComplianceCheck - Compliance status
        </entities>
        <api_endpoints>
          - POST /api/privacy/dsar - Submit data request
          - GET /api/privacy/dsar/{id}/status - Check request status
          - GET /api/compliance/hipaa - Get HIPAA status
          - GET /api/compliance/gdpr - Get GDPR status
          - GET /api/compliance/report - Generate compliance report
        </api_endpoints>
      </privacy_compliance>
    </modern_security>

    <integration_ecosystem>
      <description>Comprehensive integration capabilities for a connected healthcare ecosystem</description>

      <api_platform>
        <features>
          - RESTful API with OpenAPI 3.0 specification
          - GraphQL API for flexible queries
          - Developer portal with documentation
          - Interactive API explorer
          - API versioning
          - Rate limiting and throttling
          - API key management
          - OAuth 2.0 for third-party apps
          - Webhook subscriptions
          - Sandbox environment
          - SDK generation (JavaScript, Python, C#, Java)
          - Postman collection export
          - API analytics and monitoring
          - API marketplace for partners
        </features>
        <entities>
          - ApiApplication - Registered applications
          - ApiSubscription - API subscriptions
          - WebhookEndpoint - Webhook configurations
          - WebhookEvent - Webhook deliveries
          - ApiUsage - Usage tracking
        </entities>
        <api_endpoints>
          - GET /api/developer/apps - List applications
          - POST /api/developer/apps - Register application
          - POST /api/developer/apps/{id}/keys - Generate API key
          - GET /api/developer/webhooks - List webhooks
          - POST /api/developer/webhooks - Create webhook
          - GET /api/developer/usage - Get usage statistics
        </api_endpoints>
      </api_platform>

      <healthcare_integrations>
        <features>
          - HL7 v2.x message support (ADT, ORM, ORU, SIU)
          - FHIR R4 API compliance
          - SMART on FHIR app launch
          - CDA document import/export
          - CCDA support
          - Direct messaging (Direct Protocol)
          - Carequality/CommonWell connectivity
          - State immunization registries
          - Prescription drug monitoring programs (PDMP)
          - Public health reporting
          - Cancer registries
          - Lab interface (LIS)
          - Radiology interface (RIS)
          - Pharmacy interface
          - Medical device integration
        </features>
        <protocols>
          - HL7 v2.x (MLLP, HTTP)
          - FHIR R4 (REST)
          - DICOM
          - IHE profiles (XDS, PIX, PDQ)
          - NCPDP SCRIPT (e-prescribing)
          - X12 EDI (claims)
        </protocols>
      </healthcare_integrations>

      <e_prescribing>
        <description>Full e-prescribing integration</description>
        <features>
          - Surescripts integration
          - Medication history lookup
          - Prescription routing to pharmacies
          - Controlled substance e-prescribing (EPCS)
          - Prior authorization integration
          - Real-time benefit check (RTBC)
          - Prescription pricing
          - Pharmacy finder
          - Prescription status tracking
          - Refill requests from pharmacies
        </features>
        <entities>
          - ERxPrescription - Electronic prescriptions
          - ERxPharmacy - Pharmacy directory
          - ERxStatus - Prescription status
          - ERxPriorAuth - Prior auth requests
        </entities>
      </e_prescribing>

      <claims_clearinghouse>
        <description>Insurance claims integration</description>
        <features>
          - X12 837 claim submission
          - X12 835 remittance processing
          - Real-time eligibility (X12 270/271)
          - Claim status inquiry (X12 276/277)
          - Attachment submission
          - Denial management
          - Multiple clearinghouse support
          - Direct payer connections
          - Claim scrubbing
          - ERA auto-posting
        </features>
        <integrations>
          - Change Healthcare
          - Availity
          - Trizetto
          - Waystar
          - Office Ally
        </integrations>
      </claims_clearinghouse>

      <payment_integrations>
        <features>
          - Credit card processing (Stripe, Square, Authorize.net)
          - ACH/eCheck payments
          - Digital wallets (Apple Pay, Google Pay)
          - Payment plans and financing
          - Recurring payments
          - Text-to-pay
          - QR code payments
          - PCI DSS compliance
          - Tokenization
          - Fraud detection
          - Refund processing
        </features>
        <entities>
          - PaymentMethod - Stored payment methods
          - PaymentPlan - Payment plan configurations
          - RecurringPayment - Recurring payment schedules
        </entities>
      </payment_integrations>

      <communication_integrations>
        <features>
          - Twilio (SMS, Voice, Video)
          - SendGrid/Mailgun (Email)
          - WhatsApp Business API
          - Facebook Messenger
          - RingCentral
          - Zoom integration
          - Microsoft Teams integration
          - Fax (eFax, SRFax)
          - Direct mail services
          - Patient communication platforms
        </features>
      </communication_integrations>

      <third_party_connectors>
        <features>
          - Microsoft 365 integration
          - Google Workspace integration
          - Salesforce CRM
          - HubSpot
          - Zapier connector
          - Power Automate connector
          - QuickBooks integration
          - Xero accounting
          - ADP payroll
          - Background check services
          - Drug screening services
          - Credit check services
        </features>
      </third_party_connectors>
    </integration_ecosystem>

    <accessibility_globalization>
      <description>Accessibility compliance and global market support</description>

      <accessibility_compliance>
        <standards>WCAG 2.1 Level AA, Section 508, ADA</standards>
        <features>
          - Screen reader compatibility (ARIA labels)
          - Keyboard navigation support
          - High contrast mode
          - Font size adjustment
          - Color blind friendly palettes
          - Focus indicators
          - Skip navigation links
          - Alt text for images
          - Captions for videos
          - Transcripts for audio
          - Form field labels and error messages
          - Accessible data tables
          - Touch target sizing
          - Motion reduction options
          - Reading level optimization
          - Voice navigation
        </features>
        <testing>
          - Automated accessibility scanning
          - Manual accessibility audits
          - Screen reader testing (NVDA, JAWS, VoiceOver)
          - Keyboard-only navigation testing
          - Color contrast checking
          - User testing with disabilities
        </testing>
      </accessibility_compliance>

      <internationalization>
        <features>
          - Unicode support throughout
          - Right-to-left (RTL) layout support
          - Language detection
          - User language preferences
          - Tenant default language
          - Date/time format localization
          - Number format localization
          - Currency format and conversion
          - Address format localization
          - Phone number format localization
          - Cultural calendar support
          - Time zone handling
          - Translation management system
          - Machine translation integration
        </features>
        <supported_languages>
          - English (US, UK, AU)
          - Arabic (RTL)
          - Spanish
          - French
          - German
          - Portuguese
          - Chinese (Simplified, Traditional)
          - Japanese
          - Korean
          - Hindi
          - Urdu (RTL)
          - Turkish
          - Russian
          - Italian
          - Dutch
        </supported_languages>
        <entities>
          - Translation - Translation strings
          - LanguagePack - Language bundles
          - TranslationJob - Translation workflows
        </entities>
      </internationalization>

      <regional_compliance>
        <features>
          - Country-specific healthcare regulations
          - Regional data residency
          - Local tax calculations
          - Country-specific ID formats
          - Regional consent requirements
          - Local payment methods
          - Currency handling
        </features>
        <regions>
          - USA (HIPAA, state regulations)
          - UAE/GCC (DHA, HAAD, MOH regulations)
          - UK (NHS standards, UK GDPR)
          - EU (GDPR, local health regulations)
          - Australia (Australian Privacy Act)
          - Canada (PIPEDA, provincial health acts)
          - India (DISHA)
          - Saudi Arabia (NPHIES)
        </regions>
      </regional_compliance>
    </accessibility_globalization>

    <queue_management_system>
      <description>Complete patient queue and flow management</description>
      <features>
        - Digital token/number generation
        - Multi-service queue support
        - Priority queuing (VIP, emergency, elderly, disabled)
        - Virtual queue (join from anywhere)
        - SMS notifications for queue position
        - Estimated wait time calculation
        - Queue display screens
        - Service counter management
        - Queue analytics and reporting
        - Staff queue assignment
        - Break management without queue disruption
        - Queue overflow handling
        - Appointment vs walk-in queue separation
        - Department transfer
        - Queue abandonment tracking
      </features>
      <entities>
        - Queue - Queue definitions
        - QueueTicket - Queue tickets
        - ServiceCounter - Service points
        - QueueDisplay - Display configurations
        - QueueStaff - Staff assignments
      </entities>
      <api_endpoints>
        - POST /api/queue/ticket - Generate ticket
        - GET /api/queue/position/{ticketId} - Get position
        - GET /api/queue/wait-time/{queueId} - Get wait time
        - POST /api/queue/call-next - Call next patient
        - POST /api/queue/transfer - Transfer to another queue
        - GET /api/queue/display/{displayId} - Get display data
        - GET /api/queue/analytics - Get queue analytics
      </api_endpoints>
    </queue_management_system>

    <hospital_operations_modules>
      <description>Essential hospital operations modules for inpatient facilities</description>

      <bed_management>
        <status>PRIORITY IMPLEMENTATION</status>
        <features>
          - Real-time bed availability dashboard
          - Bed allocation and assignment
          - Room and ward management
          - Bed type categorization (ICU, general, private, semi-private)
          - Bed cleaning status tracking
          - Admission/discharge/transfer (ADT) workflow
          - Bed reservation for scheduled admissions
          - Bed blocking for maintenance
          - Housekeeping integration
          - Bed turnover analytics
          - Capacity forecasting
          - Overflow management
        </features>
        <entities>
          - Bed - Bed records
          - Room - Room records
          - Ward - Ward records
          - BedAssignment - Patient-bed assignments
          - BedStatus - Status tracking
          - BedCleaningRequest - Cleaning workflow
        </entities>
        <api_endpoints>
          - GET /api/beds - List beds with status
          - GET /api/beds/available - Get available beds
          - POST /api/beds/assign - Assign patient to bed
          - POST /api/beds/transfer - Transfer patient
          - POST /api/beds/discharge - Discharge from bed
          - GET /api/beds/dashboard - Bed management dashboard
        </api_endpoints>
      </bed_management>

      <nursing_module>
        <status>PRIORITY IMPLEMENTATION</status>
        <features>
          - Nursing assessment templates
          - Care plan creation and management
          - Nursing care documentation
          - Medication administration record (MAR)
          - Intake and output (I/O) tracking
          - Pain assessment
          - Fall risk assessment
          - Pressure ulcer risk (Braden scale)
          - Nursing shift handover
          - Nurse-patient assignment
          - Nursing task management
          - Clinical reminders
          - Nursing notes
          - Vital signs trending
          - Patient rounding documentation
        </features>
        <entities>
          - NursingAssessment - Assessment records
          - CarePlan - Care plans
          - CarePlanIntervention - Care interventions
          - MedicationAdministration - MAR records
          - IntakeOutput - I/O records
          - NursingShiftHandover - Handover records
          - NurseAssignment - Patient assignments
          - NursingTask - Tasks
        </entities>
        <api_endpoints>
          - GET /api/nursing/patients - Get nursing patient list
          - POST /api/nursing/assessments - Create assessment
          - GET /api/nursing/care-plans/{patientId} - Get care plans
          - POST /api/nursing/care-plans - Create care plan
          - POST /api/nursing/mar - Record medication administration
          - POST /api/nursing/io - Record intake/output
          - POST /api/nursing/handover - Create shift handover
          - GET /api/nursing/tasks - Get nursing tasks
        </api_endpoints>
      </nursing_module>

      <operating_room_management>
        <features>
          - OR scheduling and booking
          - Surgery request workflow
          - Pre-operative assessment checklist
          - Surgery team assignment (surgeon, anesthesiologist, nurses)
          - Equipment and instrument tracking
          - Anesthesia records
          - Intraoperative documentation
          - Post-operative care orders
          - Recovery room management (PACU)
          - OR utilization analytics
          - Surgery delay tracking
          - Emergency OR allocation
          - Block time management
        </features>
        <entities>
          - OperatingRoom - OR records
          - SurgerySchedule - Surgery schedules
          - SurgeryCase - Surgery cases
          - SurgeryTeam - Team assignments
          - AnesthesiaRecord - Anesthesia documentation
          - ORInstrumentSet - Instrument tracking
          - PreOpChecklist - Preop checklists
        </entities>
      </operating_room_management>

      <emergency_department>
        <features>
          - Triage system (ESI 5-level)
          - Emergency registration (quick reg)
          - Trauma alerts
          - Resuscitation documentation
          - ED tracking board
          - Bed assignment
          - Provider assignment
          - Order sets for common emergencies
          - Disposition management
          - ED-specific vital signs
          - Time tracking (door-to-doctor, etc.)
          - Ambulance arrival notification
          - Mass casualty protocols
        </features>
        <entities>
          - TriageAssessment - Triage records
          - EDVisit - ED visit records
          - TraumaActivation - Trauma alerts
          - EDTrackingBoard - Board configuration
        </entities>
      </emergency_department>

      <discharge_management>
        <features>
          - Discharge planning workflow
          - Discharge checklist
          - Discharge summary generation
          - Discharge instructions (AVS)
          - Medication reconciliation
          - Follow-up appointment scheduling
          - Home care referrals
          - Durable medical equipment orders
          - Transportation coordination
          - Discharge education
          - Pharmacy discharge medications
          - Insurance authorization for post-acute care
        </features>
        <entities>
          - DischargePlan - Discharge plans
          - DischargeChecklist - Checklist items
          - DischargeSummary - Summary documents
          - DischargeInstruction - Patient instructions
          - DMEOrder - Equipment orders
        </entities>
        <api_endpoints>
          - GET /api/discharge/pending - Get pending discharges
          - POST /api/discharge/plan - Create discharge plan
          - POST /api/discharge/summary - Generate summary
          - POST /api/discharge/complete - Complete discharge
          - GET /api/discharge/instructions/{patientId} - Get instructions
        </api_endpoints>
      </discharge_management>
    </hospital_operations_modules>
  </wow_features>

  <database_schema>
    <overview>
      <total_entities>306+</total_entities>
      <lookup_entities>25+</lookup_entities>
      <multi_tenant_entities>15+</multi_tenant_entities>
      <specialty_module_entities>60+</specialty_module_entities>
      <configuration_entities>10+</configuration_entities>
      <total_enums>150+</total_enums>
    </overview>

    <core_tables>
      <multi_tenancy>Tenants, Companies, Branches, TenantSettings, CompanySettings, Features, TenantFeatures, TenantTerminology, TenantUISchema, TenantFormLayout, TenantListLayout, TenantNavigation, UserBranch</multi_tenancy>
      <patient_clinical>Patients, PatientMedicalHistory, PatientDocuments, PatientAllergies, PatientInsurance, PatientConsent, Appointments, ClinicalVisits, Diagnoses, Procedures, Prescriptions, VitalSigns, Visits</patient_clinical>
      <sales>Sales, SaleItems, Quotations, QuotationItems, Payments</sales>
      <procurement>PurchaseOrders, PurchaseOrderItems, GoodsReceipts, GoodsReceiptItems, Suppliers, SupplierPayments, ExternalLabs</procurement>
      <financial>Invoices, InvoicePayments, Accounts, FinancialAccounts, FinancialTransactions, Expenses, ExpenseCategories, Vouchers, VoucherLines, InsuranceClaims, InsuranceClaimItems, InsurancePreAuthorizations, PaymentGatewayConfigs, PaymentGatewayTransactions</financial>
      <inventory>InventoryItems, InventoryTransactions</inventory>
      <hr_payroll>Employees, Doctors, Attendance, AttendanceRecords, LeaveRequests, PerformanceReviews, Departments, JobPositions, PayrollPeriods, Payslips, SalaryComponents, TaxConfigurations, WpsSubmissions</hr_payroll>
      <laboratory>LabTests, LabOrders, LabOrderItems, LabResults, ExternalLabs, SpecimenTypes, TestCategories</laboratory>
      <radiology>DicomStudies, DicomSeries, DicomInstances, DicomWorklistEntries, RadiologyReports, PacsServerConfigs</radiology>
      <pharmacy>FormularyDrugs, DrugPricings, ControlledSubstanceInfo, DrugAdverseEvents, DrugDatabaseUpdates, PatientAssistancePrograms</pharmacy>
      <communication>MessageThreads, Messages, MessageAttachments, PatientNotifications, NotificationPreferences, NotificationTemplates, PushDevices, PushNotificationTopics, TopicSubscriptions, ScheduledPushNotifications, PushNotificationHistoryEntries</communication>
      <analytics>AnalyticsDashboards, AnalyticsDashboardWidgets, AnalyticsAlerts, AnalyticsAlertRules, AnalyticsAnomalies, DashboardShares, CustomReportDefinitions, ReportSchedules, SavedReports, ReportWidgets</analytics>
      <security>Roles, Permissions, RolePermissions, UserRoles, UserPermissions, DataAccessRules, SecuritySettings, ApiKeys, PasswordHistory, AuditLogs</security>
      <case_management>Cases, CaseTypes, CaseStatuses, CaseNotes, CaseActivities</case_management>
      <marketing>Leads, Campaigns, MarketingActivities</marketing>
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

    <sales>
      - GET /api/sales - List sales
      - GET /api/sales/{id} - Get sale details
      - POST /api/sales - Create sale
      - PUT /api/sales/{id}/confirm - Confirm sale
      - PUT /api/sales/{id}/complete - Complete sale
      - PUT /api/sales/{id}/cancel - Cancel sale
      - DELETE /api/sales/{id} - Delete sale
      - GET /api/sales/patient/{patientId} - Get patient sales
      - GET /api/sales/overdue - Get overdue sales
      - GET /api/sales/{id}/payments - Get sale payments
      - POST /api/sales/{id}/payments - Record payment
      - GET /api/sales/quotations - List quotations
      - POST /api/sales/quotations - Create quotation
      - POST /api/sales/quotations/{id}/accept - Accept quotation
      - POST /api/sales/quotations/{id}/reject - Reject quotation
      - POST /api/sales/quotations/{id}/convert - Convert to sale
      - GET /api/sales/statistics - Sales statistics
    </sales>

    <procurement>
      - GET /api/procurement/purchase-orders - List POs
      - POST /api/procurement/purchase-orders - Create PO
      - GET /api/procurement/purchase-orders/{id} - Get PO details
      - PUT /api/procurement/purchase-orders/{id} - Update PO
      - PUT /api/procurement/purchase-orders/{id}/approve - Approve PO
      - PUT /api/procurement/purchase-orders/{id}/cancel - Cancel PO
      - GET /api/procurement/goods-receipts - List goods receipts
      - POST /api/procurement/goods-receipts - Create goods receipt
      - GET /api/procurement/suppliers - List suppliers
      - POST /api/procurement/suppliers - Create supplier
      - PUT /api/procurement/suppliers/{id} - Update supplier
      - GET /api/procurement/suppliers/{id}/payments - Supplier payments
      - POST /api/procurement/suppliers/{id}/payments - Record payment
      - GET /api/procurement/external-labs - List external labs
    </procurement>

    <consent>
      - GET /api/consent/templates - List consent templates
      - POST /api/consent/templates - Create consent template
      - GET /api/consent/patient/{patientId} - Get patient consents
      - POST /api/consent - Record consent
      - PUT /api/consent/{id}/revoke - Revoke consent
    </consent>

    <clinical_decision_support>
      - GET /api/cds/drug-interactions - Check drug interactions
      - GET /api/cds/allergy-check - Check patient allergies
      - GET /api/cds/recommendations - Get clinical recommendations
    </clinical_decision_support>

    <backup>
      - GET /api/backup/history - Get backup history
      - POST /api/backup/create - Trigger backup
      - POST /api/backup/restore - Restore from backup
      - GET /api/backup/status - Get backup status
    </backup>

    <jobs>
      - GET /api/jobs - List scheduled jobs
      - GET /api/jobs/{id} - Get job details
      - POST /api/jobs/{id}/run - Trigger job
      - PUT /api/jobs/{id}/pause - Pause job
    </jobs>
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
      - Sales section (Sales, Quotations, Payments)
      - Procurement section (Purchase Orders, Suppliers, Goods Receipt)
      - Financial section (Invoices, Payments, Expenses, Insurance Claims)
      - Inventory section (Items, Stock, Transactions)
      - HR section (Employees, Attendance, Leave, Performance)
      - Payroll section (Payroll, Payslips, WPS)
      - Specialty modules (expandable - 22+ specialties)
      - Marketing section (Leads, Campaigns)
      - Case Management section
      - Analytics & Reports section
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

  <gaps_missing_modules>
    <description>
      The following healthcare modules are NOT currently implemented as dedicated modules in the system.
      These represent potential areas for future development to create a complete hospital management solution.
    </description>

    <hospital_operations>
      <nursing_module>
        <status>NOT IMPLEMENTED</status>
        <description>Dedicated nursing care management</description>
        <missing_features>
          - Nursing care plans
          - Nursing assessments
          - Medication administration records (MAR)
          - Nursing shift handover
          - Patient care schedules
          - Nursing notes and documentation
          - Nurse-to-patient assignment
        </missing_features>
      </nursing_module>

      <operating_room_management>
        <status>NOT IMPLEMENTED</status>
        <description>Surgery and operating room scheduling</description>
        <missing_features>
          - OR scheduling and booking
          - Surgery team allocation
          - Pre-operative assessments
          - Post-operative care tracking
          - Anesthesia records
          - Surgical equipment tracking
          - OR utilization reports
        </missing_features>
        <note>General procedures are tracked but no dedicated OR management</note>
      </operating_room_management>

      <emergency_department>
        <status>NOT IMPLEMENTED</status>
        <description>Emergency department operations</description>
        <missing_features>
          - Triage system
          - Emergency case prioritization
          - Trauma tracking
          - Emergency protocols
          - Resuscitation records
          - ED-specific workflows
        </missing_features>
        <note>Only emergency access/contact fields exist in general entities</note>
      </emergency_department>

      <icu_management>
        <status>NOT IMPLEMENTED</status>
        <description>Intensive care unit management</description>
        <missing_features>
          - ICU bed management
          - Ventilator tracking
          - Continuous monitoring integration
          - ICU scoring systems (APACHE, SOFA)
          - ICU-specific vital signs
          - Critical care protocols
        </missing_features>
      </icu_management>

      <bed_management>
        <status>NOT IMPLEMENTED</status>
        <description>Hospital bed and room management</description>
        <missing_features>
          - Bed allocation and assignment
          - Room occupancy tracking
          - Bed cleaning status
          - Transfer management
          - Admission/discharge bed workflow
          - Bed utilization reports
        </missing_features>
      </bed_management>

      <discharge_management>
        <status>NOT IMPLEMENTED</status>
        <description>Patient discharge workflow</description>
        <missing_features>
          - Discharge summaries
          - Discharge instructions
          - Follow-up scheduling at discharge
          - Discharge checklist
          - Take-home medications
          - Discharge coordination
        </missing_features>
      </discharge_management>

      <queue_management>
        <status>NOT IMPLEMENTED</status>
        <description>Patient queue and waiting room management</description>
        <missing_features>
          - Digital queuing system
          - Waiting time tracking
          - Queue displays
          - Token/number generation
          - Department-wise queues
          - Priority queuing
        </missing_features>
      </queue_management>
    </hospital_operations>

    <support_services>
      <blood_bank>
        <status>NOT IMPLEMENTED</status>
        <description>Blood bank and transfusion services</description>
        <missing_features>
          - Blood inventory management
          - Donor registration and management
          - Blood grouping and cross-matching
          - Transfusion tracking
          - Blood component separation
          - Blood expiry tracking
          - Donor appointment scheduling
        </missing_features>
      </blood_bank>

      <mortuary_services>
        <status>NOT IMPLEMENTED</status>
        <description>Mortuary and death records management</description>
        <missing_features>
          - Death certificate generation
          - Body storage management
          - Release authorization
          - Autopsy records
          - Funeral home coordination
        </missing_features>
      </mortuary_services>

      <sterilization_cssd>
        <status>NOT IMPLEMENTED</status>
        <description>Central Sterile Services Department</description>
        <missing_features>
          - Instrument sterilization tracking
          - Sterilization cycles
          - Instrument set management
          - Sterility validation
          - Equipment maintenance
        </missing_features>
      </sterilization_cssd>

      <kitchen_dietary>
        <status>NOT IMPLEMENTED</status>
        <description>Kitchen and dietary management</description>
        <missing_features>
          - Patient diet orders
          - Meal planning and preparation
          - Dietary restrictions tracking
          - Kitchen inventory
          - Meal delivery scheduling
          - Nutritionist consultations
        </missing_features>
      </kitchen_dietary>

      <housekeeping>
        <status>NOT IMPLEMENTED</status>
        <description>Hospital housekeeping management</description>
        <missing_features>
          - Cleaning schedules
          - Room cleaning status
          - Housekeeping task assignment
          - Cleaning supplies inventory
          - Infection control cleaning
        </missing_features>
      </housekeeping>

      <linen_laundry>
        <status>NOT IMPLEMENTED</status>
        <description>Linen and laundry management</description>
        <missing_features>
          - Linen inventory
          - Laundry schedules
          - Linen distribution
          - Contaminated linen handling
        </missing_features>
      </linen_laundry>

      <maintenance_management>
        <status>NOT IMPLEMENTED</status>
        <description>Facility and equipment maintenance</description>
        <missing_features>
          - Preventive maintenance schedules
          - Work order management
          - Equipment breakdown tracking
          - Maintenance staff assignment
          - Spare parts inventory
          - Equipment calibration
        </missing_features>
      </maintenance_management>

      <asset_management>
        <status>NOT IMPLEMENTED</status>
        <description>Hospital asset and equipment tracking</description>
        <missing_features>
          - Asset registry
          - Asset depreciation
          - Asset location tracking
          - Asset maintenance history
          - Asset lifecycle management
        </missing_features>
        <note>Only equipment references exist in current system</note>
      </asset_management>
    </support_services>

    <quality_compliance>
      <quality_management>
        <status>NOT IMPLEMENTED</status>
        <description>Quality management system</description>
        <missing_features>
          - Incident reporting
          - Corrective actions (CAPA)
          - Quality indicators
          - Accreditation tracking
          - Policy management
          - Quality audits
        </missing_features>
      </quality_management>

      <infection_control>
        <status>NOT IMPLEMENTED</status>
        <description>Infection control and surveillance</description>
        <missing_features>
          - Nosocomial infection tracking
          - Outbreak management
          - Infection control protocols
          - Hand hygiene compliance
          - Antibiotic stewardship
          - Isolation management
        </missing_features>
      </infection_control>

      <referral_management>
        <status>PARTIAL</status>
        <description>Patient referral tracking</description>
        <missing_features>
          - Dedicated referral module
          - Referral tracking and status
          - External referral management
          - Referral reports
        </missing_features>
        <note>Only referral fields exist in insurance plan; no dedicated referral workflow</note>
      </referral_management>
    </quality_compliance>

    <recommendations>
      <priority_high>
        - Bed Management (critical for inpatient facilities)
        - Nursing Module (essential for hospital operations)
        - Discharge Management (patient safety and continuity)
        - Queue Management (patient experience)
      </priority_high>
      <priority_medium>
        - Operating Room Management (surgical facilities)
        - ICU Management (critical care units)
        - Blood Bank (hospitals with transfusion services)
        - Quality Management (accreditation requirements)
      </priority_medium>
      <priority_low>
        - Mortuary Services (specialized facilities)
        - Kitchen/Dietary (large hospitals)
        - Housekeeping (can use external systems)
        - Laundry Management (can use external systems)
      </priority_low>
    </recommendations>
  </gaps_missing_modules>

  <success_criteria>
    <functionality>
      - All 500+ API endpoints functional (expanded from 401+)
      - Multi-tenancy with complete data isolation
      - All 22+ specialty modules operational
      - Patient lifecycle fully managed
      - Sales and procurement workflows complete
      - Financial operations complete
      - Lab and radiology workflows functional
      - Hospital operations modules (bed, nursing, OR, ED, discharge) functional
    </functionality>

    <performance>
      - API response times under 200ms
      - Efficient database queries with EF Core
      - Redis caching implemented
      - Pagination for large datasets
      - Optimized frontend bundle size
      - Real-time updates via SignalR with <100ms latency
      - Mobile app response times under 300ms
      - AI inference times under 2 seconds
    </performance>

    <security>
      - RBAC fully implemented
      - JWT authentication secure
      - Data access rules enforced
      - Audit logging complete
      - HIPAA compliance features
      - GDPR compliance tools operational
      - Biometric and MFA authentication available
      - Zero-trust architecture principles applied
      - Threat detection and monitoring active
      - Data encryption at rest and in transit
    </security>

    <quality>
      - Clean architecture maintained
      - Unit test coverage adequate (>80%)
      - E2E tests passing
      - Code quality tools configured
      - Documentation complete
      - WCAG 2.1 Level AA accessibility compliance
      - Mobile app store approval ready
    </quality>

    <wow_features_criteria>
      <ai_ml>
        - Predictive models trained with >80% accuracy
        - No-show prediction reducing missed appointments by 30%
        - AI symptom checker providing accurate triage 90% of time
        - Voice transcription accuracy >95%
        - Medical image AI analysis available for supported modalities
      </ai_ml>
      <mobile>
        - Native iOS and Android patient apps published
        - Provider mobile app with offline capability
        - Push notification delivery rate >95%
        - App store rating target: 4.5+ stars
        - Biometric login enabled
      </mobile>
      <patient_experience>
        - Digital check-in reducing wait times by 40%
        - Patient satisfaction scores (NPS) tracked
        - Virtual waiting room functional
        - Wearable data syncing from 5+ platforms
        - Queue management with accurate wait time estimates
      </patient_experience>
      <integrations>
        - FHIR R4 certification
        - E-prescribing (Surescripts) certified
        - 3+ payment gateway integrations
        - 5+ clearinghouse connections
        - Video conferencing integrated
      </integrations>
      <analytics>
        - Real-time executive dashboards
        - Population health tools operational
        - Self-service report builder available
        - Benchmarking data integrated
      </analytics>
    </wow_features_criteria>
  </success_criteria>

  <implementation_priority>
    <phase_1_foundation>
      <name>Core Platform + Essential Wow Features</name>
      <duration>Months 1-4</duration>
      <deliverables>
        - Core ERP modules (Patient, Appointment, Clinical, Lab, Financial)
        - Multi-tenancy and RBAC
        - Basic mobile apps (patient + provider)
        - Real-time notifications via SignalR
        - Digital check-in and queue management
        - Basic AI (no-show prediction, smart scheduling)
      </deliverables>
    </phase_1_foundation>

    <phase_2_engagement>
      <name>Patient Engagement + Advanced Mobile</name>
      <duration>Months 5-7</duration>
      <deliverables>
        - Full-featured patient mobile app
        - Video telemedicine integration
        - Wearable device integration
        - Patient education library
        - Health goals and wellness features
        - Patient feedback and surveys
      </deliverables>
    </phase_2_engagement>

    <phase_3_intelligence>
      <name>AI/ML + Advanced Analytics</name>
      <duration>Months 8-10</duration>
      <deliverables>
        - Clinical AI (diagnosis suggestions, image analysis)
        - NLP (voice transcription, auto-coding)
        - Virtual health assistant chatbot
        - Population health analytics
        - Advanced BI dashboards
        - Predictive analytics suite
      </deliverables>
    </phase_3_intelligence>

    <phase_4_hospital>
      <name>Hospital Operations + Workflow</name>
      <duration>Months 11-13</duration>
      <deliverables>
        - Bed management
        - Nursing module
        - Operating room management
        - Emergency department
        - Discharge management
        - Low-code workflow builder
      </deliverables>
    </phase_4_hospital>

    <phase_5_ecosystem>
      <name>Integration Ecosystem + Compliance</name>
      <duration>Months 14-16</duration>
      <deliverables>
        - E-prescribing certification
        - Claims clearinghouse integration
        - Developer API platform
        - FHIR R4 certification
        - Advanced security features
        - Regional compliance (HIPAA, GDPR, regional)
      </deliverables>
    </phase_5_ecosystem>
  </implementation_priority>

  <wow_feature_summary>
    <total_new_entities>100+</total_new_entities>
    <total_new_api_endpoints>150+</total_new_api_endpoints>
    <key_differentiators>
      - AI-powered clinical decision support and predictions
      - Native mobile apps for patients, providers, and staff
      - Real-time collaboration with SignalR
      - Wearable and IoT device integration
      - Remote patient monitoring (RPM) program
      - Virtual health assistant (chatbot)
      - Low-code workflow automation
      - Complete queue management system
      - Hospital operations suite (bed, nursing, OR, ED)
      - Modern security with biometrics and zero-trust
      - Comprehensive integration ecosystem
      - WCAG 2.1 accessibility compliance
      - Multi-language and RTL support
    </key_differentiators>
    <competitive_advantages>
      - Only healthcare ERP with integrated AI across all modules
      - Mobile-first design with offline capability
      - Complete patient journey from home to hospital and back
      - Self-service analytics without IT dependency
      - Flexible workflow automation for any process
      - Single platform for clinic to hospital operations
    </competitive_advantages>
  </wow_feature_summary>
</project_specification>
