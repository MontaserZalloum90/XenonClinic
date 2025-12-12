# XenonClinic Module Journeys Documentation

Complete documentation of all user journeys across XenonClinic modules, including journey steps and step owners.

---

## Table of Contents

1. [Patient Management](#1-patient-management)
2. [Appointments](#2-appointments)
3. [Clinical Visits](#3-clinical-visits)
4. [Laboratory](#4-laboratory)
5. [Radiology](#5-radiology)
6. [Pharmacy](#6-pharmacy)
7. [Financial](#7-financial)
8. [Inventory](#8-inventory)
9. [HR Management](#9-hr-management)
10. [Payroll](#10-payroll)
11. [Marketing](#11-marketing)
12. [Workflow Engine](#12-workflow-engine)
13. [Patient Portal](#13-patient-portal)
14. [Multi-Tenancy](#14-multi-tenancy)
15. [Analytics & Reporting](#15-analytics--reporting)
16. [Security & Audit](#16-security--audit)
17. [Specialty Modules](#17-specialty-modules)

---

## 1. Patient Management

### 1.1 Patient Registration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Navigate to patient registration | Receptionist |
| 2 | Enter personal information (name, DOB, gender) | Receptionist |
| 3 | Enter contact details (phone, email, address) | Receptionist |
| 4 | Add emergency contact information | Receptionist |
| 5 | Capture/upload patient photo | Receptionist |
| 6 | Assign unique patient ID | System |
| 7 | Save and confirm registration | Receptionist |

### 1.2 Patient Search & Lookup
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access patient search interface | Staff |
| 2 | Enter search criteria (name, ID, phone) | Staff |
| 3 | Review search results | Staff |
| 4 | Select patient from results | Staff |
| 5 | View patient profile | Staff |

### 1.3 Medical History Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open patient record | Nurse/Doctor |
| 2 | Navigate to medical history section | Nurse/Doctor |
| 3 | Add allergies and reactions | Nurse/Doctor |
| 4 | Record chronic conditions | Doctor |
| 5 | Enter past surgeries/procedures | Doctor |
| 6 | Document family medical history | Doctor |
| 7 | Save medical history | Nurse/Doctor |

### 1.4 Document Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access patient documents | Staff |
| 2 | Upload new document (ID, insurance, referral) | Staff |
| 3 | Categorize document type | Staff |
| 4 | Add document metadata | Staff |
| 5 | Verify document quality | Staff |
| 6 | Save document to patient record | System |

### 1.5 Insurance Information
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Navigate to insurance section | Receptionist |
| 2 | Add insurance provider details | Receptionist |
| 3 | Enter policy number and group ID | Receptionist |
| 4 | Set coverage dates | Receptionist |
| 5 | Verify insurance eligibility | Billing Staff |
| 6 | Save insurance information | Receptionist |

---

## 2. Appointments

### 2.1 Appointment Booking
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Search for patient | Receptionist |
| 2 | Select appointment type | Receptionist |
| 3 | Choose department/specialty | Receptionist |
| 4 | Select doctor/provider | Receptionist |
| 5 | View available time slots | System |
| 6 | Select preferred date and time | Receptionist |
| 7 | Add appointment notes/reason | Receptionist |
| 8 | Confirm booking | Receptionist |
| 9 | Send confirmation notification | System |

### 2.2 Appointment Rescheduling
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Locate existing appointment | Receptionist |
| 2 | Open appointment details | Receptionist |
| 3 | Click reschedule option | Receptionist |
| 4 | Select new date/time | Receptionist |
| 5 | Update appointment notes | Receptionist |
| 6 | Confirm rescheduling | Receptionist |
| 7 | Send update notification | System |

### 2.3 Appointment Cancellation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Find scheduled appointment | Receptionist |
| 2 | Open appointment details | Receptionist |
| 3 | Select cancel option | Receptionist |
| 4 | Enter cancellation reason | Receptionist |
| 5 | Confirm cancellation | Receptionist |
| 6 | Send cancellation notification | System |
| 7 | Free up time slot | System |

### 2.4 Patient Check-in
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Identify patient arrival | Receptionist |
| 2 | Verify appointment details | Receptionist |
| 3 | Confirm patient identity | Receptionist |
| 4 | Update appointment status to "Checked-in" | Receptionist |
| 5 | Collect co-payment if applicable | Receptionist |
| 6 | Direct patient to waiting area | Receptionist |
| 7 | Notify provider of arrival | System |

### 2.5 Waitlist Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Patient requests earlier appointment | Receptionist |
| 2 | Add patient to waitlist | Receptionist |
| 3 | Set preferred dates and constraints | Receptionist |
| 4 | Monitor for cancellations | System |
| 5 | Notify patient of availability | System |
| 6 | Confirm new appointment | Receptionist |
| 7 | Remove from waitlist | System |

### 2.6 Recurring Appointments
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create initial appointment | Receptionist |
| 2 | Enable recurring option | Receptionist |
| 3 | Set recurrence pattern (weekly/monthly) | Receptionist |
| 4 | Define number of occurrences or end date | Receptionist |
| 5 | Generate recurring appointments | System |
| 6 | Confirm all appointments | Receptionist |

---

## 3. Clinical Visits

### 3.1 Patient Intake & Vitals
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Call patient from waiting area | Nurse |
| 2 | Open patient visit record | Nurse |
| 3 | Measure and record blood pressure | Nurse |
| 4 | Record temperature | Nurse |
| 5 | Measure pulse/heart rate | Nurse |
| 6 | Record respiratory rate | Nurse |
| 7 | Measure height and weight | Nurse |
| 8 | Calculate BMI | System |
| 9 | Record oxygen saturation | Nurse |
| 10 | Document chief complaint | Nurse |
| 11 | Save vitals to visit record | Nurse |

### 3.2 Clinical Examination
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review patient history | Doctor |
| 2 | Review current vitals | Doctor |
| 3 | Conduct physical examination | Doctor |
| 4 | Document examination findings | Doctor |
| 5 | Order diagnostic tests if needed | Doctor |
| 6 | Review test results | Doctor |
| 7 | Formulate assessment | Doctor |

### 3.3 Diagnosis Entry
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Search ICD-10 codes | Doctor |
| 2 | Select primary diagnosis | Doctor |
| 3 | Add secondary diagnoses if applicable | Doctor |
| 4 | Set diagnosis status (confirmed/suspected) | Doctor |
| 5 | Link to clinical findings | Doctor |
| 6 | Save diagnosis | Doctor |

### 3.4 Prescription Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open prescription module | Doctor |
| 2 | Search medication database | Doctor |
| 3 | Select medication | Doctor |
| 4 | Set dosage and frequency | Doctor |
| 5 | Define duration of treatment | Doctor |
| 6 | Check drug interactions | System |
| 7 | Review allergy alerts | System |
| 8 | Add prescription instructions | Doctor |
| 9 | Sign prescription | Doctor |
| 10 | Print/send to pharmacy | Doctor |

### 3.5 Treatment Plan Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create new treatment plan | Doctor |
| 2 | Define treatment goals | Doctor |
| 3 | Add prescribed medications | Doctor |
| 4 | Schedule procedures/therapies | Doctor |
| 5 | Set follow-up appointments | Doctor |
| 6 | Add patient education materials | Doctor |
| 7 | Document expected outcomes | Doctor |
| 8 | Review with patient | Doctor |
| 9 | Obtain patient consent | Doctor |
| 10 | Finalize treatment plan | Doctor |

### 3.6 Referral Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Initiate referral request | Doctor |
| 2 | Select specialty/department | Doctor |
| 3 | Select referring physician | Doctor |
| 4 | Attach relevant clinical documents | Doctor |
| 5 | Add referral reason/notes | Doctor |
| 6 | Set urgency level | Doctor |
| 7 | Submit referral | Doctor |
| 8 | Track referral status | Doctor/Staff |
| 9 | Receive referral response | System |

### 3.7 Visit Completion
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review all visit documentation | Doctor |
| 2 | Verify prescriptions | Doctor |
| 3 | Confirm orders placed | Doctor |
| 4 | Add visit summary | Doctor |
| 5 | Sign and lock visit | Doctor |
| 6 | Generate after-visit summary | System |
| 7 | Schedule follow-up if needed | Receptionist |
| 8 | Check-out patient | Receptionist |

---

## 4. Laboratory

### 4.1 Lab Order Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access lab order module | Doctor/Nurse |
| 2 | Select patient | Doctor/Nurse |
| 3 | Choose test panel or individual tests | Doctor |
| 4 | Set clinical priority | Doctor |
| 5 | Add clinical notes/indications | Doctor |
| 6 | Specify fasting requirements | Doctor |
| 7 | Submit lab order | Doctor |
| 8 | Print requisition/labels | System |

### 4.2 Specimen Collection
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Verify patient identity | Lab Technician |
| 2 | Verify order details | Lab Technician |
| 3 | Prepare collection materials | Lab Technician |
| 4 | Collect specimen(s) | Lab Technician |
| 5 | Label specimens with barcode | Lab Technician |
| 6 | Document collection time | Lab Technician |
| 7 | Record any collection issues | Lab Technician |
| 8 | Transport to lab | Lab Technician |

### 4.3 Specimen Processing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Receive specimen in lab | Lab Staff |
| 2 | Verify specimen quality | Lab Staff |
| 3 | Log specimen receipt | Lab Staff |
| 4 | Centrifuge/process if required | Lab Staff |
| 5 | Aliquot samples if needed | Lab Staff |
| 6 | Load onto analyzer | Lab Staff |
| 7 | Run quality control | Lab Staff |

### 4.4 Result Entry & Validation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review analyzer output | Lab Technician |
| 2 | Enter manual results if needed | Lab Technician |
| 3 | Flag abnormal values | System |
| 4 | Apply reference ranges | System |
| 5 | Technical validation | Lab Technician |
| 6 | Medical validation/review | Pathologist |
| 7 | Approve and release results | Pathologist |

### 4.5 Result Reporting
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Generate result report | System |
| 2 | Notify ordering physician | System |
| 3 | Update patient portal | System |
| 4 | Print result if requested | Lab Staff |
| 5 | Archive result | System |

### 4.6 Quality Control
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Perform daily QC tests | Lab Technician |
| 2 | Record QC results | Lab Technician |
| 3 | Review QC charts | Lab Supervisor |
| 4 | Investigate out-of-range QC | Lab Supervisor |
| 5 | Document corrective actions | Lab Supervisor |
| 6 | Approve daily QC | Lab Supervisor |

---

## 5. Radiology

### 5.1 Imaging Order Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open radiology order module | Doctor |
| 2 | Select patient | Doctor |
| 3 | Choose imaging modality (X-ray, CT, MRI, US) | Doctor |
| 4 | Select body region | Doctor |
| 5 | Add clinical indication | Doctor |
| 6 | Check for contraindications | Doctor/System |
| 7 | Specify contrast requirements | Doctor |
| 8 | Set priority level | Doctor |
| 9 | Submit order | Doctor |

### 5.2 Patient Preparation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review order requirements | Radiology Technician |
| 2 | Verify patient identity | Radiology Technician |
| 3 | Screen for contraindications | Radiology Technician |
| 4 | Explain procedure to patient | Radiology Technician |
| 5 | Obtain informed consent | Radiology Technician |
| 6 | Prepare contrast if required | Radiology Technician |
| 7 | Position patient | Radiology Technician |

### 5.3 Image Acquisition
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Configure imaging equipment | Radiology Technician |
| 2 | Acquire images | Radiology Technician |
| 3 | Review image quality | Radiology Technician |
| 4 | Retake if necessary | Radiology Technician |
| 5 | Process images | System |
| 6 | Upload to PACS | System |
| 7 | Document acquisition details | Radiology Technician |

### 5.4 Image Interpretation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open study in DICOM viewer | Radiologist |
| 2 | Review clinical history | Radiologist |
| 3 | Analyze images systematically | Radiologist |
| 4 | Compare with prior studies | Radiologist |
| 5 | Identify findings | Radiologist |
| 6 | Measure lesions if present | Radiologist |
| 7 | Dictate/type report | Radiologist |

### 5.5 Report Generation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Structure report with findings | Radiologist |
| 2 | Add impression/conclusion | Radiologist |
| 3 | Include recommendations | Radiologist |
| 4 | Sign report | Radiologist |
| 5 | Release report | Radiologist |
| 6 | Notify referring physician | System |
| 7 | Update patient record | System |

### 5.6 Critical Finding Alert
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Identify critical finding | Radiologist |
| 2 | Immediately contact referring physician | Radiologist |
| 3 | Document communication | Radiologist |
| 4 | Update report with critical flag | Radiologist |
| 5 | Log critical alert | System |

---

## 6. Pharmacy

### 6.1 Prescription Receipt
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Receive prescription (electronic/paper) | Pharmacist |
| 2 | Verify prescription validity | Pharmacist |
| 3 | Confirm patient identity | Pharmacist |
| 4 | Check insurance coverage | Pharmacy Staff |
| 5 | Enter into pharmacy system | Pharmacy Staff |
| 6 | Queue for filling | System |

### 6.2 Prescription Verification
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review prescription details | Pharmacist |
| 2 | Check drug interactions | System |
| 3 | Verify dosage appropriateness | Pharmacist |
| 4 | Check patient allergies | Pharmacist |
| 5 | Confirm medication availability | Pharmacist |
| 6 | Contact prescriber if issues | Pharmacist |
| 7 | Approve for dispensing | Pharmacist |

### 6.3 Medication Dispensing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Select medication from inventory | Pharmacy Technician |
| 2 | Verify drug/strength/form | Pharmacy Technician |
| 3 | Count/measure quantity | Pharmacy Technician |
| 4 | Label container | Pharmacy Technician |
| 5 | Pharmacist final check | Pharmacist |
| 6 | Package medication | Pharmacy Technician |
| 7 | Record dispensing | System |

### 6.4 Patient Counseling
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Call patient for pickup | Pharmacy Staff |
| 2 | Verify patient identity | Pharmacist |
| 3 | Explain medication usage | Pharmacist |
| 4 | Discuss side effects | Pharmacist |
| 5 | Answer patient questions | Pharmacist |
| 6 | Provide written instructions | Pharmacist |
| 7 | Obtain signature | Pharmacy Staff |
| 8 | Complete transaction | Pharmacy Staff |

### 6.5 Controlled Substance Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Verify prescriber DEA registration | Pharmacist |
| 2 | Validate prescription requirements | Pharmacist |
| 3 | Check PDMP database | Pharmacist |
| 4 | Document controlled substance dispensing | Pharmacist |
| 5 | Secure storage management | Pharmacist |
| 6 | Record in controlled substance log | Pharmacist |
| 7 | Inventory reconciliation | Pharmacy Manager |

### 6.6 Inventory Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Monitor stock levels | System |
| 2 | Generate reorder alerts | System |
| 3 | Create purchase orders | Pharmacy Manager |
| 4 | Receive shipments | Pharmacy Staff |
| 5 | Verify received quantities | Pharmacy Staff |
| 6 | Update inventory | Pharmacy Staff |
| 7 | Handle expired medications | Pharmacy Staff |
| 8 | Conduct inventory audits | Pharmacy Manager |

---

## 7. Financial

### 7.1 Invoice Generation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Select patient/visit | Billing Staff |
| 2 | Review charges | Billing Staff |
| 3 | Apply procedure/service codes | Billing Staff |
| 4 | Calculate taxes if applicable | System |
| 5 | Apply discounts if authorized | Billing Staff |
| 6 | Generate invoice | System |
| 7 | Preview and verify | Billing Staff |
| 8 | Finalize invoice | Billing Staff |
| 9 | Print/email invoice | Billing Staff |

### 7.2 Payment Processing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open payment module | Cashier |
| 2 | Search for invoice | Cashier |
| 3 | Select payment method (cash/card/insurance) | Cashier |
| 4 | Enter payment amount | Cashier |
| 5 | Process payment | Cashier |
| 6 | Print receipt | Cashier |
| 7 | Update invoice status | System |
| 8 | Close cash drawer | Cashier |

### 7.3 Insurance Claim Submission
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Verify insurance eligibility | Billing Staff |
| 2 | Prepare claim with CPT/ICD codes | Billing Staff |
| 3 | Attach supporting documents | Billing Staff |
| 4 | Submit claim electronically | System |
| 5 | Track claim status | Billing Staff |
| 6 | Receive ERA/EOB | System |
| 7 | Post insurance payment | Billing Staff |
| 8 | Handle denials/appeals | Billing Staff |

### 7.4 Account Receivable Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review aging report | Billing Manager |
| 2 | Identify overdue accounts | Billing Staff |
| 3 | Send payment reminders | System |
| 4 | Contact patients for collections | Billing Staff |
| 5 | Set up payment plans | Billing Staff |
| 6 | Escalate to collections if needed | Billing Manager |
| 7 | Write off bad debt (if approved) | Billing Manager |

### 7.5 Expense Recording
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create expense entry | Accountant |
| 2 | Select expense category | Accountant |
| 3 | Enter vendor details | Accountant |
| 4 | Enter amount and date | Accountant |
| 5 | Attach receipts/invoices | Accountant |
| 6 | Submit for approval | Accountant |
| 7 | Approve expense | Finance Manager |
| 8 | Process payment | Accountant |

### 7.6 Financial Reporting
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Select report type | Finance Manager |
| 2 | Set date range | Finance Manager |
| 3 | Apply filters (branch, department) | Finance Manager |
| 4 | Generate report | System |
| 5 | Review report data | Finance Manager |
| 6 | Export to PDF/Excel | Finance Manager |
| 7 | Share with stakeholders | Finance Manager |

---

## 8. Inventory

### 8.1 Stock Receipt
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Receive delivery at warehouse | Warehouse Staff |
| 2 | Verify against purchase order | Warehouse Staff |
| 3 | Inspect item quality | Warehouse Staff |
| 4 | Record received quantities | Warehouse Staff |
| 5 | Check expiration dates | Warehouse Staff |
| 6 | Update inventory system | System |
| 7 | Store items in designated locations | Warehouse Staff |
| 8 | Complete goods receipt note | Warehouse Staff |

### 8.2 Stock Transfer
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create transfer request | Department Head |
| 2 | Specify source and destination | Department Head |
| 3 | Select items and quantities | Department Head |
| 4 | Submit transfer request | Department Head |
| 5 | Approve transfer | Warehouse Manager |
| 6 | Pick items from source | Warehouse Staff |
| 7 | Transport to destination | Warehouse Staff |
| 8 | Receive at destination | Receiving Staff |
| 9 | Update inventory at both locations | System |

### 8.3 Stock Adjustment
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Identify discrepancy | Warehouse Staff |
| 2 | Initiate stock adjustment | Warehouse Staff |
| 3 | Select adjustment reason | Warehouse Staff |
| 4 | Enter quantity adjustment | Warehouse Staff |
| 5 | Add justification notes | Warehouse Staff |
| 6 | Submit for approval | Warehouse Staff |
| 7 | Approve adjustment | Warehouse Manager |
| 8 | Update inventory | System |
| 9 | Generate adjustment report | System |

### 8.4 Purchase Order Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review stock levels | Procurement Staff |
| 2 | Identify items to reorder | Procurement Staff |
| 3 | Select supplier | Procurement Staff |
| 4 | Add items to purchase order | Procurement Staff |
| 5 | Specify quantities and prices | Procurement Staff |
| 6 | Set delivery terms | Procurement Staff |
| 7 | Submit for approval | Procurement Staff |
| 8 | Approve purchase order | Procurement Manager |
| 9 | Send PO to supplier | System |

### 8.5 Inventory Audit
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Schedule inventory count | Warehouse Manager |
| 2 | Generate count sheets | System |
| 3 | Conduct physical count | Warehouse Staff |
| 4 | Record counted quantities | Warehouse Staff |
| 5 | Compare with system quantities | System |
| 6 | Investigate variances | Warehouse Manager |
| 7 | Approve adjustments | Warehouse Manager |
| 8 | Generate audit report | System |

---

## 9. HR Management

### 9.1 Employee Onboarding
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create employee record | HR Staff |
| 2 | Enter personal information | HR Staff |
| 3 | Upload documents (ID, certificates) | HR Staff |
| 4 | Assign department and position | HR Staff |
| 5 | Set reporting manager | HR Staff |
| 6 | Create system credentials | IT Admin |
| 7 | Assign roles and permissions | IT Admin |
| 8 | Schedule orientation | HR Staff |
| 9 | Complete onboarding checklist | HR Staff |

### 9.2 Attendance Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Employee clocks in | Employee |
| 2 | System records timestamp | System |
| 3 | Employee clocks out | Employee |
| 4 | Calculate work hours | System |
| 5 | Flag late arrivals/early departures | System |
| 6 | Review attendance exceptions | Manager |
| 7 | Approve corrections if needed | Manager |
| 8 | Generate attendance report | System |

### 9.3 Leave Request
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Employee submits leave request | Employee |
| 2 | Select leave type | Employee |
| 3 | Specify dates | Employee |
| 4 | Add reason/notes | Employee |
| 5 | Submit for approval | Employee |
| 6 | Manager reviews request | Manager |
| 7 | Check leave balance | System |
| 8 | Approve or reject request | Manager |
| 9 | Notify employee of decision | System |
| 10 | Update leave balance | System |

### 9.4 Performance Evaluation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | HR initiates evaluation cycle | HR Manager |
| 2 | Employee completes self-assessment | Employee |
| 3 | Manager reviews employee performance | Manager |
| 4 | Schedule review meeting | Manager |
| 5 | Conduct performance discussion | Manager |
| 6 | Document evaluation results | Manager |
| 7 | Set goals for next period | Manager/Employee |
| 8 | Employee acknowledges evaluation | Employee |
| 9 | HR archives evaluation | HR Staff |

### 9.5 Training Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Identify training needs | Manager/HR |
| 2 | Create training program | HR Staff |
| 3 | Schedule training sessions | HR Staff |
| 4 | Enroll employees | HR Staff |
| 5 | Conduct training | Trainer |
| 6 | Track attendance | HR Staff |
| 7 | Record completion | System |
| 8 | Issue certificates | HR Staff |
| 9 | Evaluate training effectiveness | HR Manager |

---

## 10. Payroll

### 10.1 Salary Configuration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access payroll settings | Payroll Admin |
| 2 | Define salary components | Payroll Admin |
| 3 | Set up allowances | Payroll Admin |
| 4 | Configure deductions | Payroll Admin |
| 5 | Set tax rules | Payroll Admin |
| 6 | Define overtime rates | Payroll Admin |
| 7 | Save payroll configuration | Payroll Admin |

### 10.2 Payroll Processing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Select pay period | Payroll Staff |
| 2 | Import attendance data | System |
| 3 | Calculate work hours | System |
| 4 | Apply overtime calculations | System |
| 5 | Add bonuses/commissions | Payroll Staff |
| 6 | Apply deductions | System |
| 7 | Calculate tax withholdings | System |
| 8 | Generate payroll summary | System |
| 9 | Review for accuracy | Payroll Manager |
| 10 | Approve payroll | Finance Manager |
| 11 | Process payments | Payroll Staff |

### 10.3 Payslip Generation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Finalize payroll | Payroll Staff |
| 2 | Generate individual payslips | System |
| 3 | Include earnings breakdown | System |
| 4 | Include deductions breakdown | System |
| 5 | Calculate net pay | System |
| 6 | Distribute payslips | System |
| 7 | Archive payslips | System |

### 10.4 Overtime Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Employee requests overtime | Employee |
| 2 | Specify overtime hours and reason | Employee |
| 3 | Submit for approval | Employee |
| 4 | Manager reviews request | Manager |
| 5 | Approve or reject | Manager |
| 6 | Record approved overtime | System |
| 7 | Include in payroll calculation | System |

### 10.5 Loan Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Employee applies for loan | Employee |
| 2 | Specify loan amount and reason | Employee |
| 3 | HR reviews application | HR Staff |
| 4 | Finance approves loan | Finance Manager |
| 5 | Set repayment schedule | Finance Staff |
| 6 | Disburse loan | Finance Staff |
| 7 | Deduct from monthly salary | System |
| 8 | Track repayment progress | System |

---

## 11. Marketing

### 11.1 Campaign Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access marketing module | Marketing Manager |
| 2 | Create new campaign | Marketing Manager |
| 3 | Define campaign name and type | Marketing Manager |
| 4 | Set target audience | Marketing Manager |
| 5 | Define campaign objectives | Marketing Manager |
| 6 | Set budget allocation | Marketing Manager |
| 7 | Set start and end dates | Marketing Manager |
| 8 | Design campaign materials | Marketing Staff |
| 9 | Configure distribution channels | Marketing Staff |
| 10 | Submit for approval | Marketing Staff |
| 11 | Approve and launch campaign | Marketing Manager |

### 11.2 Lead Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Capture lead (web form, call, referral) | System/Staff |
| 2 | Create lead record | Marketing Staff |
| 3 | Assign lead source | Marketing Staff |
| 4 | Qualify lead | Sales Staff |
| 5 | Assign to sales representative | Marketing Manager |
| 6 | Track lead interactions | Sales Staff |
| 7 | Update lead status | Sales Staff |
| 8 | Convert to patient/customer | Sales Staff |

### 11.3 Promotion Setup
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Create new promotion | Marketing Manager |
| 2 | Define promotion type (discount, package) | Marketing Manager |
| 3 | Set discount percentage/amount | Marketing Manager |
| 4 | Define applicable services | Marketing Manager |
| 5 | Set validity period | Marketing Manager |
| 6 | Configure usage limits | Marketing Manager |
| 7 | Generate promotion codes | System |
| 8 | Activate promotion | Marketing Manager |

### 11.4 Referral Program
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Configure referral program | Marketing Manager |
| 2 | Set referral rewards | Marketing Manager |
| 3 | Generate referral codes | System |
| 4 | Track referrals | System |
| 5 | Validate successful referrals | Marketing Staff |
| 6 | Process referral rewards | Marketing Staff |
| 7 | Notify referrers | System |

### 11.5 Email Marketing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Design email template | Marketing Staff |
| 2 | Create email content | Marketing Staff |
| 3 | Select recipient segment | Marketing Staff |
| 4 | Schedule send time | Marketing Staff |
| 5 | Submit for approval | Marketing Staff |
| 6 | Approve email campaign | Marketing Manager |
| 7 | Send emails | System |
| 8 | Track opens and clicks | System |
| 9 | Analyze campaign performance | Marketing Manager |

### 11.6 Marketing Analytics
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access analytics dashboard | Marketing Manager |
| 2 | Select metrics to analyze | Marketing Manager |
| 3 | Set date range | Marketing Manager |
| 4 | View campaign performance | Marketing Manager |
| 5 | Analyze conversion rates | Marketing Manager |
| 6 | Calculate ROI | Marketing Manager |
| 7 | Generate performance reports | System |
| 8 | Share insights with stakeholders | Marketing Manager |

---

## 12. Workflow Engine

### 12.1 Process Definition
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access workflow designer | System Admin |
| 2 | Create new process definition | System Admin |
| 3 | Define start event/trigger | System Admin |
| 4 | Add workflow steps/tasks | System Admin |
| 5 | Configure task assignments | System Admin |
| 6 | Add decision gateways | System Admin |
| 7 | Define conditions/rules | System Admin |
| 8 | Set timeouts and escalations | System Admin |
| 9 | Test workflow | System Admin |
| 10 | Deploy to production | System Admin |

### 12.2 Task Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | View assigned tasks | Assigned User |
| 2 | Open task details | Assigned User |
| 3 | Review task instructions | Assigned User |
| 4 | Complete task actions | Assigned User |
| 5 | Add task notes/comments | Assigned User |
| 6 | Submit task completion | Assigned User |
| 7 | System routes to next step | System |

### 12.3 Approval Workflows
| Step | Description | Owner |
|------|-------------|-------|
| 1 | User initiates approval request | Requester |
| 2 | System routes to approver | System |
| 3 | Approver receives notification | System |
| 4 | Approver reviews request | Approver |
| 5 | Approver approves/rejects | Approver |
| 6 | Add approval comments | Approver |
| 7 | System notifies requester | System |
| 8 | Execute post-approval actions | System |

### 12.4 Workflow Monitoring
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access workflow dashboard | Supervisor |
| 2 | View active process instances | Supervisor |
| 3 | Check process status | Supervisor |
| 4 | Identify bottlenecks | Supervisor |
| 5 | View task assignments | Supervisor |
| 6 | Reassign tasks if needed | Supervisor |
| 7 | Handle exceptions | Supervisor |
| 8 | Generate workflow reports | System |

---

## 13. Patient Portal

### 13.1 Patient Registration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access patient portal registration | Patient |
| 2 | Enter personal information | Patient |
| 3 | Create login credentials | Patient |
| 4 | Verify email address | Patient |
| 5 | Accept terms and conditions | Patient |
| 6 | Complete profile | Patient |
| 7 | Link to clinic records | System |

### 13.2 Online Appointment Booking
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in to patient portal | Patient |
| 2 | Navigate to appointments | Patient |
| 3 | Select specialty/doctor | Patient |
| 4 | View available slots | Patient |
| 5 | Select preferred time | Patient |
| 6 | Enter visit reason | Patient |
| 7 | Confirm booking | Patient |
| 8 | Receive confirmation | System |

### 13.3 View Medical Records
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in to patient portal | Patient |
| 2 | Navigate to health records | Patient |
| 3 | View visit summaries | Patient |
| 4 | View lab results | Patient |
| 5 | View prescriptions | Patient |
| 6 | Download/print records | Patient |

### 13.4 Request Prescription Refill
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in to patient portal | Patient |
| 2 | Navigate to prescriptions | Patient |
| 3 | Select medication for refill | Patient |
| 4 | Choose pharmacy | Patient |
| 5 | Submit refill request | Patient |
| 6 | Doctor reviews request | Doctor |
| 7 | Approve/modify prescription | Doctor |
| 8 | Send to pharmacy | System |
| 9 | Notify patient | System |

### 13.5 Secure Messaging
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in to patient portal | Patient |
| 2 | Navigate to messages | Patient |
| 3 | Compose new message | Patient |
| 4 | Select recipient | Patient |
| 5 | Write message | Patient |
| 6 | Attach documents if needed | Patient |
| 7 | Send message | Patient |
| 8 | Provider receives notification | System |
| 9 | Provider responds | Provider |
| 10 | Patient receives notification | System |

### 13.6 Online Bill Payment
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in to patient portal | Patient |
| 2 | Navigate to billing | Patient |
| 3 | View outstanding balances | Patient |
| 4 | Select invoices to pay | Patient |
| 5 | Enter payment details | Patient |
| 6 | Confirm payment | Patient |
| 7 | Process payment | System |
| 8 | Send receipt | System |

---

## 14. Multi-Tenancy

### 14.1 Tenant Provisioning
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Receive new tenant request | Platform Admin |
| 2 | Create tenant record | Platform Admin |
| 3 | Configure tenant settings | Platform Admin |
| 4 | Set up database schema | System |
| 5 | Create admin user | Platform Admin |
| 6 | Configure branding | Platform Admin |
| 7 | Enable modules | Platform Admin |
| 8 | Activate tenant | Platform Admin |
| 9 | Send welcome email | System |

### 14.2 Company Setup
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Log in as tenant admin | Tenant Admin |
| 2 | Navigate to company settings | Tenant Admin |
| 3 | Enter company information | Tenant Admin |
| 4 | Upload company logo | Tenant Admin |
| 5 | Configure fiscal year | Tenant Admin |
| 6 | Set regional preferences | Tenant Admin |
| 7 | Save company settings | Tenant Admin |

### 14.3 Branch Configuration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Navigate to branch management | Tenant Admin |
| 2 | Create new branch | Tenant Admin |
| 3 | Enter branch details | Tenant Admin |
| 4 | Set operating hours | Tenant Admin |
| 5 | Enable services for branch | Tenant Admin |
| 6 | Assign branch manager | Tenant Admin |
| 7 | Configure branch-specific settings | Tenant Admin |
| 8 | Activate branch | Tenant Admin |

### 14.4 User Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Navigate to user management | Tenant Admin |
| 2 | Create new user | Tenant Admin |
| 3 | Assign role(s) | Tenant Admin |
| 4 | Assign to branch(es) | Tenant Admin |
| 5 | Set permissions | Tenant Admin |
| 6 | Send invitation email | System |
| 7 | User activates account | User |

### 14.5 Subscription Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | View current subscription | Tenant Admin |
| 2 | Review usage metrics | Tenant Admin |
| 3 | Select new plan if needed | Tenant Admin |
| 4 | Confirm plan change | Tenant Admin |
| 5 | Update payment method | Tenant Admin |
| 6 | Process subscription change | System |
| 7 | Apply new plan features | System |

---

## 15. Analytics & Reporting

### 15.1 Dashboard Configuration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access analytics dashboard | Manager |
| 2 | Click customize dashboard | Manager |
| 3 | Add/remove widgets | Manager |
| 4 | Rearrange widget positions | Manager |
| 5 | Configure widget settings | Manager |
| 6 | Set default date range | Manager |
| 7 | Save dashboard layout | Manager |

### 15.2 Custom Report Creation
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Open report builder | Analyst |
| 2 | Select data source | Analyst |
| 3 | Add report columns | Analyst |
| 4 | Configure filters | Analyst |
| 5 | Add grouping/sorting | Analyst |
| 6 | Add calculations | Analyst |
| 7 | Preview report | Analyst |
| 8 | Save report | Analyst |
| 9 | Share with users | Analyst |

### 15.3 Scheduled Report Setup
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Select report to schedule | Manager |
| 2 | Click schedule report | Manager |
| 3 | Set frequency (daily/weekly/monthly) | Manager |
| 4 | Set delivery time | Manager |
| 5 | Add email recipients | Manager |
| 6 | Select export format | Manager |
| 7 | Activate schedule | Manager |
| 8 | System sends reports on schedule | System |

### 15.4 Data Export
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Navigate to data export | User |
| 2 | Select data type | User |
| 3 | Apply filters | User |
| 4 | Select date range | User |
| 5 | Choose export format | User |
| 6 | Initiate export | User |
| 7 | Download exported file | User |

### 15.5 KPI Monitoring
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access KPI dashboard | Executive |
| 2 | View key metrics | Executive |
| 3 | Compare to targets | Executive |
| 4 | Analyze trends | Executive |
| 5 | Drill down for details | Executive |
| 6 | Take action on insights | Executive |

---

## 16. Security & Audit

### 16.1 User Access Review
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Generate user access report | Security Admin |
| 2 | Review user permissions | Security Admin |
| 3 | Identify excessive permissions | Security Admin |
| 4 | Request justification | Security Admin |
| 5 | Update permissions as needed | Security Admin |
| 6 | Document review | Security Admin |
| 7 | Schedule next review | Security Admin |

### 16.2 Audit Log Review
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access audit log module | Auditor |
| 2 | Set search criteria | Auditor |
| 3 | Filter by date range | Auditor |
| 4 | Filter by user/action type | Auditor |
| 5 | Review log entries | Auditor |
| 6 | Investigate anomalies | Auditor |
| 7 | Document findings | Auditor |
| 8 | Report to management | Auditor |

### 16.3 Password Policy Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Access security settings | Security Admin |
| 2 | Configure password requirements | Security Admin |
| 3 | Set password expiration | Security Admin |
| 4 | Configure lockout policy | Security Admin |
| 5 | Enable multi-factor authentication | Security Admin |
| 6 | Save policy changes | Security Admin |
| 7 | Notify users of changes | System |

### 16.4 Security Incident Response
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Detect security incident | System/User |
| 2 | Log incident details | Security Team |
| 3 | Assess severity | Security Manager |
| 4 | Contain the incident | Security Team |
| 5 | Investigate root cause | Security Team |
| 6 | Remediate vulnerabilities | IT Team |
| 7 | Document lessons learned | Security Manager |
| 8 | Update security measures | Security Team |

### 16.5 Compliance Reporting
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Identify compliance requirements | Compliance Officer |
| 2 | Map controls to requirements | Compliance Officer |
| 3 | Gather evidence | Compliance Officer |
| 4 | Review control effectiveness | Compliance Officer |
| 5 | Document gaps | Compliance Officer |
| 6 | Create remediation plan | Compliance Officer |
| 7 | Generate compliance report | Compliance Officer |
| 8 | Submit to auditors | Compliance Officer |

---

## 17. Specialty Modules

### 17.1 Audiology

#### Audiometry Testing
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Prepare audiometry room | Audiologist |
| 2 | Explain test to patient | Audiologist |
| 3 | Position patient with headphones | Audiologist |
| 4 | Conduct pure tone audiometry | Audiologist |
| 5 | Record thresholds for each frequency | Audiologist |
| 6 | Perform speech audiometry | Audiologist |
| 7 | Complete tympanometry | Audiologist |
| 8 | Plot audiogram | System |
| 9 | Interpret results | Audiologist |
| 10 | Recommend hearing aids if needed | Audiologist |

### 17.2 Dental

#### Dental Examination
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review patient dental history | Dentist |
| 2 | Perform visual examination | Dentist |
| 3 | Use explorer for detailed check | Dentist |
| 4 | Update dental chart | Dentist |
| 5 | Take X-rays if needed | Dental Assistant |
| 6 | Document findings per tooth | Dentist |
| 7 | Create treatment plan | Dentist |
| 8 | Discuss with patient | Dentist |

### 17.3 Cardiology

#### ECG Recording
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Prepare patient | Cardiac Technician |
| 2 | Apply electrodes | Cardiac Technician |
| 3 | Record 12-lead ECG | Cardiac Technician |
| 4 | Check signal quality | Cardiac Technician |
| 5 | Save ECG recording | System |
| 6 | Cardiologist reviews ECG | Cardiologist |
| 7 | Document interpretation | Cardiologist |
| 8 | Add to patient record | System |

### 17.4 Ophthalmology

#### Eye Examination
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Test visual acuity | Ophthalmologist/Technician |
| 2 | Perform refraction | Ophthalmologist |
| 3 | Measure intraocular pressure | Ophthalmologist |
| 4 | Conduct slit lamp examination | Ophthalmologist |
| 5 | Dilate pupils if needed | Ophthalmologist |
| 6 | Examine fundus | Ophthalmologist |
| 7 | Document findings | Ophthalmologist |
| 8 | Prescribe glasses/treatment | Ophthalmologist |

### 17.5 Orthopedics

#### Fracture Management
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review X-rays | Orthopedic Surgeon |
| 2 | Assess fracture type | Orthopedic Surgeon |
| 3 | Determine treatment approach | Orthopedic Surgeon |
| 4 | Explain to patient | Orthopedic Surgeon |
| 5 | Apply cast/splint or schedule surgery | Orthopedic Surgeon |
| 6 | Document treatment | Orthopedic Surgeon |
| 7 | Schedule follow-up | Staff |
| 8 | Monitor healing progress | Orthopedic Surgeon |

### 17.6 Pediatrics

#### Well-Child Visit
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Measure growth parameters | Nurse |
| 2 | Plot on growth charts | System |
| 3 | Assess developmental milestones | Pediatrician |
| 4 | Conduct physical examination | Pediatrician |
| 5 | Review vaccination schedule | Pediatrician |
| 6 | Administer vaccines | Nurse |
| 7 | Provide anticipatory guidance | Pediatrician |
| 8 | Schedule next visit | Staff |

### 17.7 OB/GYN

#### Prenatal Visit
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Record weight and blood pressure | Nurse |
| 2 | Measure fundal height | OB/GYN |
| 3 | Listen to fetal heart tones | OB/GYN |
| 4 | Review lab results | OB/GYN |
| 5 | Perform ultrasound if scheduled | OB/GYN |
| 6 | Assess fetal movement | OB/GYN |
| 7 | Discuss concerns | OB/GYN |
| 8 | Update pregnancy record | OB/GYN |
| 9 | Schedule next appointment | Staff |

### 17.8 Physiotherapy

#### Physical Therapy Session
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Review treatment plan | Physiotherapist |
| 2 | Assess current status | Physiotherapist |
| 3 | Perform therapeutic exercises | Physiotherapist |
| 4 | Apply manual therapy | Physiotherapist |
| 5 | Use modalities (heat, ultrasound) | Physiotherapist |
| 6 | Educate on home exercises | Physiotherapist |
| 7 | Document session notes | Physiotherapist |
| 8 | Update progress | Physiotherapist |

### 17.9 Oncology

#### Chemotherapy Administration
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Verify patient identity | Oncology Nurse |
| 2 | Review treatment protocol | Oncology Nurse |
| 3 | Check lab values | Oncologist |
| 4 | Prepare chemotherapy agents | Pharmacist |
| 5 | Verify drug, dose, route | Oncology Nurse |
| 6 | Establish IV access | Oncology Nurse |
| 7 | Administer pre-medications | Oncology Nurse |
| 8 | Infuse chemotherapy | Oncology Nurse |
| 9 | Monitor for reactions | Oncology Nurse |
| 10 | Document administration | Oncology Nurse |

### 17.10 Dialysis

#### Hemodialysis Session
| Step | Description | Owner |
|------|-------------|-------|
| 1 | Weigh patient (pre-dialysis) | Dialysis Nurse |
| 2 | Record vital signs | Dialysis Nurse |
| 3 | Assess vascular access | Dialysis Nurse |
| 4 | Cannulate access | Dialysis Nurse |
| 5 | Connect to dialysis machine | Dialysis Nurse |
| 6 | Set dialysis parameters | Dialysis Nurse |
| 7 | Monitor during treatment | Dialysis Nurse |
| 8 | Record intradialytic vitals | Dialysis Nurse |
| 9 | Complete dialysis | Dialysis Nurse |
| 10 | Weigh patient (post-dialysis) | Dialysis Nurse |
| 11 | Document session | Dialysis Nurse |

---

## Summary Statistics

| Module | User Journeys | Total Steps |
|--------|--------------|-------------|
| Patient Management | 5 | 35 |
| Appointments | 6 | 45 |
| Clinical Visits | 7 | 55 |
| Laboratory | 6 | 40 |
| Radiology | 6 | 40 |
| Pharmacy | 6 | 50 |
| Financial | 6 | 45 |
| Inventory | 5 | 40 |
| HR Management | 5 | 45 |
| Payroll | 5 | 40 |
| Marketing | 6 | 55 |
| Workflow Engine | 4 | 30 |
| Patient Portal | 6 | 45 |
| Multi-Tenancy | 5 | 40 |
| Analytics & Reporting | 5 | 35 |
| Security & Audit | 5 | 40 |
| Specialty Modules | 10 | 85 |
| **TOTAL** | **98** | **765** |

---

## Role Reference

| Role | Description |
|------|-------------|
| Receptionist | Front desk staff handling patient registration and appointments |
| Nurse | Clinical staff assisting with patient care |
| Doctor | Physician providing medical care |
| Lab Technician | Staff performing laboratory tests |
| Pathologist | Physician reviewing and interpreting lab results |
| Radiologist | Physician interpreting imaging studies |
| Pharmacist | Licensed professional dispensing medications |
| Billing Staff | Staff handling invoicing and payments |
| HR Staff | Human resources personnel |
| Payroll Staff | Staff processing payroll |
| Marketing Staff | Staff managing marketing campaigns |
| System Admin | Technical administrator |
| Tenant Admin | Organization administrator |
| Security Admin | Security and access management |
| Manager | Department or team manager |
| Patient | End user of patient portal |

---

*Last Updated: December 2024*
*Document Version: 1.0*
