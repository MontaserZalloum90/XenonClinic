import { useParams, Link } from "react-router-dom";
import {
  getModuleById,
  getJourneysByModule,
  personasData,
  journeysData,
} from "@/lib/docs/docsData";
import {
  Users,
  Calendar,
  Stethoscope,
  FlaskConical,
  ScanLine,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Megaphone,
  GitBranch,
  UserCircle,
  BarChart3,
  Heart,
  ArrowRight,
  CheckCircle,
  Shield,
  AlertCircle,
  Clock,
  Target,
  FileText,
  PlayCircle,
  Code,
  BookOpen,
  Layers,
  Activity,
} from "lucide-react";

const iconMap: Record<string, React.ComponentType<{ className?: string }>> = {
  Users,
  Calendar,
  Stethoscope,
  FlaskConical,
  ScanLine,
  Pill,
  DollarSign,
  Package,
  UserCog,
  Megaphone,
  GitBranch,
  UserCircle,
  BarChart3,
  Heart,
};

// Additional journeys for modules that might not have associated journeys in the data
const additionalModuleJourneys: Record<
  string,
  {
    id: string;
    name: string;
    description: string;
    steps: number;
    duration: string;
    priority: string;
  }[]
> = {
  "patient-management": [
    {
      id: "patient-registration",
      name: "Patient Registration",
      description:
        "Register new patients with complete demographic information",
      steps: 7,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "patient-search",
      name: "Patient Search & Lookup",
      description: "Find patients using various search criteria",
      steps: 3,
      duration: "1-2 min",
      priority: "high",
    },
    {
      id: "medical-history-update",
      name: "Update Medical History",
      description:
        "Add or modify patient allergies, conditions, and surgical history",
      steps: 5,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "document-upload",
      name: "Document Upload",
      description: "Upload and categorize patient documents",
      steps: 4,
      duration: "2-3 min",
      priority: "medium",
    },
    {
      id: "insurance-verification",
      name: "Insurance Verification",
      description: "Verify and update patient insurance information",
      steps: 6,
      duration: "5-7 min",
      priority: "high",
    },
  ],
  appointments: [
    {
      id: "appointment-booking",
      name: "Appointment Booking",
      description: "Schedule new appointments with provider selection",
      steps: 9,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "appointment-rescheduling",
      name: "Appointment Rescheduling",
      description: "Change existing appointment date or time",
      steps: 5,
      duration: "2-3 min",
      priority: "high",
    },
    {
      id: "patient-checkin",
      name: "Patient Check-in",
      description: "Check in arriving patients and update queue",
      steps: 4,
      duration: "1-2 min",
      priority: "high",
    },
    {
      id: "appointment-cancellation",
      name: "Appointment Cancellation",
      description: "Cancel appointments with reason tracking",
      steps: 3,
      duration: "1-2 min",
      priority: "medium",
    },
    {
      id: "waitlist-management",
      name: "Waitlist Management",
      description: "Manage patients waiting for earlier slots",
      steps: 4,
      duration: "2-3 min",
      priority: "medium",
    },
    {
      id: "recurring-appointments",
      name: "Recurring Appointments",
      description: "Set up recurring appointment schedules",
      steps: 6,
      duration: "3-5 min",
      priority: "low",
    },
  ],
  "clinical-visits": [
    {
      id: "clinical-visit",
      name: "Complete Clinical Visit",
      description: "Full clinical encounter from intake to documentation",
      steps: 12,
      duration: "15-30 min",
      priority: "high",
    },
    {
      id: "patient-intake-vitals",
      name: "Patient Intake & Vitals",
      description: "Record vital signs and chief complaint",
      steps: 5,
      duration: "5-7 min",
      priority: "high",
    },
    {
      id: "clinical-examination",
      name: "Clinical Examination",
      description: "Document physical examination findings",
      steps: 4,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "diagnosis-entry",
      name: "Diagnosis Entry",
      description: "Enter ICD-10 coded diagnoses",
      steps: 3,
      duration: "2-3 min",
      priority: "high",
    },
    {
      id: "prescription-creation",
      name: "Prescription Creation",
      description: "Create prescriptions with drug interaction checking",
      steps: 6,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "referral-creation",
      name: "Referral Creation",
      description: "Create specialist referrals",
      steps: 5,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "visit-signing",
      name: "Visit Signing & Completion",
      description: "Sign and lock visit for billing",
      steps: 3,
      duration: "1-2 min",
      priority: "high",
    },
  ],
  laboratory: [
    {
      id: "lab-order-to-results",
      name: "Lab Order to Results",
      description: "Complete lab workflow from order to results",
      steps: 10,
      duration: "10-30 min",
      priority: "high",
    },
    {
      id: "lab-order-creation",
      name: "Lab Order Creation",
      description: "Create lab orders with test selection",
      steps: 5,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "specimen-collection",
      name: "Specimen Collection",
      description: "Collect and label specimens",
      steps: 4,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "result-entry",
      name: "Result Entry",
      description: "Enter lab results with validation",
      steps: 5,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "result-approval",
      name: "Result Approval",
      description: "Review and approve lab results",
      steps: 3,
      duration: "2-3 min",
      priority: "high",
    },
    {
      id: "critical-value-notification",
      name: "Critical Value Notification",
      description: "Handle critical lab values",
      steps: 4,
      duration: "2-5 min",
      priority: "high",
    },
  ],
  pharmacy: [
    {
      id: "prescription-dispensing",
      name: "Prescription Dispensing",
      description: "Complete prescription dispensing workflow",
      steps: 8,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "prescription-verification",
      name: "Prescription Verification",
      description: "Pharmacist review and verification",
      steps: 4,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "medication-dispensing",
      name: "Medication Dispensing",
      description: "Dispense and label medications",
      steps: 5,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "patient-counseling",
      name: "Patient Counseling",
      description: "Provide medication counseling",
      steps: 3,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "controlled-substance-handling",
      name: "Controlled Substance Handling",
      description: "Handle DEA-regulated medications",
      steps: 6,
      duration: "5-7 min",
      priority: "high",
    },
    {
      id: "inventory-reorder",
      name: "Inventory Reorder",
      description: "Reorder low-stock medications",
      steps: 4,
      duration: "5-10 min",
      priority: "medium",
    },
  ],
  financial: [
    {
      id: "invoice-payment",
      name: "Invoice & Payment",
      description: "Create invoice and process payment",
      steps: 7,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "invoice-creation",
      name: "Invoice Creation",
      description: "Generate patient invoices",
      steps: 5,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "payment-collection",
      name: "Payment Collection",
      description: "Process patient payments",
      steps: 4,
      duration: "2-3 min",
      priority: "high",
    },
    {
      id: "insurance-claim-submission",
      name: "Insurance Claim Submission",
      description: "Submit claims to insurance",
      steps: 6,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "refund-processing",
      name: "Refund Processing",
      description: "Process patient refunds",
      steps: 5,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "revenue-reporting",
      name: "Revenue Reporting",
      description: "Generate financial reports",
      steps: 4,
      duration: "5-10 min",
      priority: "medium",
    },
  ],
  radiology: [
    {
      id: "imaging-study",
      name: "Imaging Study",
      description: "Complete imaging workflow from order to report",
      steps: 8,
      duration: "15-30 min",
      priority: "high",
    },
    {
      id: "imaging-order",
      name: "Imaging Order",
      description: "Create radiology orders",
      steps: 4,
      duration: "3-5 min",
      priority: "high",
    },
    {
      id: "study-acquisition",
      name: "Study Acquisition",
      description: "Perform imaging study",
      steps: 5,
      duration: "10-20 min",
      priority: "high",
    },
    {
      id: "radiology-reporting",
      name: "Radiology Reporting",
      description: "Create radiology reports",
      steps: 4,
      duration: "5-15 min",
      priority: "high",
    },
    {
      id: "pacs-integration",
      name: "PACS Integration",
      description: "Access images from PACS",
      steps: 3,
      duration: "1-2 min",
      priority: "medium",
    },
  ],
  inventory: [
    {
      id: "stock-receipt",
      name: "Stock Receipt",
      description: "Receive and record incoming inventory",
      steps: 6,
      duration: "10-15 min",
      priority: "high",
    },
    {
      id: "stock-adjustment",
      name: "Stock Adjustment",
      description: "Adjust inventory quantities",
      steps: 4,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "purchase-order",
      name: "Purchase Order Creation",
      description: "Create purchase orders",
      steps: 5,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "inventory-transfer",
      name: "Inventory Transfer",
      description: "Transfer stock between locations",
      steps: 5,
      duration: "5-7 min",
      priority: "medium",
    },
    {
      id: "expiry-management",
      name: "Expiry Management",
      description: "Track and manage expiring items",
      steps: 4,
      duration: "5-10 min",
      priority: "high",
    },
  ],
  "hr-management": [
    {
      id: "employee-onboarding",
      name: "Employee Onboarding",
      description: "Complete new employee setup",
      steps: 10,
      duration: "30-60 min",
      priority: "high",
    },
    {
      id: "attendance-tracking",
      name: "Attendance Tracking",
      description: "Track employee attendance",
      steps: 3,
      duration: "1-2 min",
      priority: "high",
    },
    {
      id: "leave-request",
      name: "Leave Request",
      description: "Submit and approve leave requests",
      steps: 5,
      duration: "3-5 min",
      priority: "medium",
    },
    {
      id: "payroll-processing",
      name: "Payroll Processing",
      description: "Process employee payroll",
      steps: 6,
      duration: "15-30 min",
      priority: "high",
    },
    {
      id: "performance-review",
      name: "Performance Review",
      description: "Conduct employee evaluations",
      steps: 5,
      duration: "20-30 min",
      priority: "medium",
    },
  ],
  analytics: [
    {
      id: "dashboard-view",
      name: "Dashboard Viewing",
      description: "View key performance metrics",
      steps: 2,
      duration: "1-2 min",
      priority: "high",
    },
    {
      id: "report-generation",
      name: "Report Generation",
      description: "Generate custom reports",
      steps: 5,
      duration: "5-10 min",
      priority: "high",
    },
    {
      id: "data-export",
      name: "Data Export",
      description: "Export reports in various formats",
      steps: 3,
      duration: "2-3 min",
      priority: "medium",
    },
    {
      id: "trend-analysis",
      name: "Trend Analysis",
      description: "Analyze performance trends",
      steps: 4,
      duration: "5-10 min",
      priority: "medium",
    },
  ],
};

const priorityColors: Record<string, string> = {
  high: "bg-red-100 text-red-700",
  medium: "bg-yellow-100 text-yellow-700",
  low: "bg-green-100 text-green-700",
};

export default function ModulePage() {
  const { moduleId } = useParams<{ moduleId: string }>();
  const module = moduleId ? getModuleById(moduleId) : undefined;
  const existingJourneys = moduleId ? getJourneysByModule(moduleId) : [];

  // Get additional journeys or use empty array
  const additionalJourneys =
    moduleId && additionalModuleJourneys[moduleId]
      ? additionalModuleJourneys[moduleId]
      : [];

  if (!module) {
    return (
      <div className="text-center py-12">
        <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h1 className="text-2xl font-bold text-gray-900 mb-2">
          Module Not Found
        </h1>
        <p className="text-gray-600 mb-4">
          The requested module documentation does not exist.
        </p>
        <Link to="/docs/modules" className="text-primary-600 hover:underline">
          View all modules
        </Link>
      </div>
    );
  }

  const Icon = iconMap[module.icon] || Users;
  const modulePersonas = personasData.filter((p) =>
    module.personas.includes(p.id),
  );

  return (
    <div className="space-y-10">
      {/* Header */}
      <div className="flex items-start gap-4">
        <div className="p-4 bg-primary-100 text-primary-600 rounded-xl">
          <Icon className="h-8 w-8" />
        </div>
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{module.name}</h1>
          <p className="text-lg text-gray-600 mt-2">{module.description}</p>
          <div className="flex items-center gap-4 mt-3">
            <span className="text-sm text-gray-500">
              Route:{" "}
              <code className="bg-gray-100 px-2 py-0.5 rounded">
                {module.route}
              </code>
            </span>
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              {module.category}
            </span>
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid sm:grid-cols-4 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <div className="flex items-center gap-2 text-gray-500 text-sm mb-1">
            <Users className="h-4 w-4" />
            Personas
          </div>
          <div className="text-2xl font-bold text-gray-900">
            {module.personas.length}
          </div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <div className="flex items-center gap-2 text-gray-500 text-sm mb-1">
            <Layers className="h-4 w-4" />
            Features
          </div>
          <div className="text-2xl font-bold text-gray-900">
            {module.features.length}
          </div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <div className="flex items-center gap-2 text-gray-500 text-sm mb-1">
            <Activity className="h-4 w-4" />
            Journeys
          </div>
          <div className="text-2xl font-bold text-gray-900">
            {additionalJourneys.length || existingJourneys.length}
          </div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-4">
          <div className="flex items-center gap-2 text-gray-500 text-sm mb-1">
            <Shield className="h-4 w-4" />
            Permissions
          </div>
          <div className="text-2xl font-bold text-gray-900">
            {module.permissions.length}
          </div>
        </div>
      </div>

      {/* Business Value */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Business Value
        </h2>
        <div className="bg-gradient-to-r from-primary-50 to-blue-50 rounded-xl p-6">
          <p className="text-gray-700 leading-relaxed">
            {module.businessValue}
          </p>
        </div>
      </section>

      {/* Who Uses This */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Who Uses This Module
        </h2>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {modulePersonas.length > 0
            ? modulePersonas.map((persona) => (
                <Link
                  key={persona.id}
                  to={`/docs/personas/${persona.id}`}
                  className="flex items-center gap-3 p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
                >
                  <div className="p-2 bg-gray-100 rounded-lg group-hover:bg-primary-100 transition-colors">
                    <Users className="h-5 w-5 text-gray-600 group-hover:text-primary-600" />
                  </div>
                  <div>
                    <div className="font-medium text-gray-900 group-hover:text-primary-600">
                      {persona.name}
                    </div>
                    <div className="text-xs text-gray-500">
                      {persona.category}
                    </div>
                  </div>
                </Link>
              ))
            : module.personas.map((personaId) => (
                <div
                  key={personaId}
                  className="flex items-center gap-3 p-4 bg-white border border-gray-200 rounded-lg"
                >
                  <div className="p-2 bg-gray-100 rounded-lg">
                    <Users className="h-5 w-5 text-gray-600" />
                  </div>
                  <div className="font-medium text-gray-900 capitalize">
                    {personaId.replace(/-/g, " ")}
                  </div>
                </div>
              ))}
        </div>
      </section>

      {/* Features */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Features</h2>
        <div className="space-y-4">
          {module.features.map((feature, index) => (
            <div
              key={feature.id}
              className="flex gap-4 p-4 bg-white border border-gray-200 rounded-lg"
            >
              <div className="flex-shrink-0 w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-semibold text-sm">
                {index + 1}
              </div>
              <div>
                <h3 className="font-medium text-gray-900">{feature.name}</h3>
                <p className="text-gray-600 mt-1">{feature.description}</p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Required Permissions */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Required Roles & Permissions
        </h2>
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Permission
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Description
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Operations
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {module.permissions.map((permission) => {
                const [resource, action] = permission.split(":");
                const operationDescriptions: Record<string, string> = {
                  view: "Read/list records",
                  create: "Create new records",
                  update: "Modify existing records",
                  delete: "Remove records",
                  export: "Export data",
                  approve: "Approve/validate records",
                  reschedule: "Change scheduled items",
                  dispense: "Dispense items",
                  manage: "Full management access",
                  reports: "Access reports",
                };
                return (
                  <tr key={permission}>
                    <td className="px-4 py-3">
                      <code className="bg-gray-100 text-sm px-2 py-0.5 rounded">
                        {permission}
                      </code>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {action
                        ? `${action.charAt(0).toUpperCase() + action.slice(1)} ${resource.replace(/_/g, " ")}`
                        : resource}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500">
                      {operationDescriptions[action] || "Access granted"}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
        <p className="text-sm text-gray-500 mt-2 flex items-center gap-1">
          <Shield className="h-4 w-4" />
          See{" "}
          <Link
            to="/docs/security-rbac"
            className="text-primary-600 hover:underline"
          >
            Security & RBAC
          </Link>{" "}
          for the full permission matrix.
        </p>
      </section>

      {/* Related User Journeys - Comprehensive Section */}
      <section>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-2xl font-semibold text-gray-900">
            Related User Journeys
          </h2>
          <Link
            to="/docs/journeys"
            className="text-sm text-primary-600 hover:underline flex items-center gap-1"
          >
            View all journeys <ArrowRight className="h-4 w-4" />
          </Link>
        </div>

        <p className="text-gray-600 mb-6">
          User journeys are step-by-step workflows that guide users through
          common tasks in this module. Each journey includes preconditions,
          detailed steps, validation rules, and success criteria.
        </p>

        {/* Journey Cards */}
        {existingJourneys.length > 0 || additionalJourneys.length > 0 ? (
          <div className="space-y-4">
            {/* First show existing journeys from data */}
            {existingJourneys.map((journey) => (
              <Link
                key={journey.id}
                to={`/docs/journeys/${journey.id}`}
                className="block bg-white border border-gray-200 rounded-xl p-5 hover:border-primary-300 hover:shadow transition-all group"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <PlayCircle className="h-5 w-5 text-primary-600" />
                      <h3 className="font-semibold text-gray-900 group-hover:text-primary-600">
                        {journey.name}
                      </h3>
                      <span
                        className={`px-2 py-0.5 rounded-full text-xs font-medium ${priorityColors[journey.priority]}`}
                      >
                        {journey.priority} priority
                      </span>
                    </div>
                    <p className="text-gray-600 text-sm mb-3">
                      {journey.description}
                    </p>
                    <div className="flex items-center gap-4 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Target className="h-4 w-4" />
                        {journey.steps.length} steps
                      </span>
                      <span className="flex items-center gap-1">
                        <Clock className="h-4 w-4" />
                        {journey.estimatedDuration}
                      </span>
                      <span className="flex items-center gap-1">
                        <Users className="h-4 w-4" />
                        {journey.personas.join(", ")}
                      </span>
                    </div>
                  </div>
                  <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 flex-shrink-0" />
                </div>
              </Link>
            ))}

            {/* Then show additional journeys */}
            {additionalJourneys.map((journey) => {
              // Check if this journey exists in the main journeys data
              const existsInMain = existingJourneys.some(
                (j) => j.id === journey.id,
              );
              if (existsInMain) return null;

              const journeyExists = journeysData.some(
                (j) => j.id === journey.id,
              );

              const content = (
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <PlayCircle
                        className={`h-5 w-5 ${journeyExists ? "text-primary-600" : "text-gray-400"}`}
                      />
                      <h3
                        className={`font-semibold ${journeyExists ? "text-gray-900 group-hover:text-primary-600" : "text-gray-700"}`}
                      >
                        {journey.name}
                      </h3>
                      <span
                        className={`px-2 py-0.5 rounded-full text-xs font-medium ${priorityColors[journey.priority]}`}
                      >
                        {journey.priority} priority
                      </span>
                    </div>
                    <p className="text-gray-600 text-sm mb-3">
                      {journey.description}
                    </p>
                    <div className="flex items-center gap-4 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Target className="h-4 w-4" />
                        {journey.steps} steps
                      </span>
                      <span className="flex items-center gap-1">
                        <Clock className="h-4 w-4" />
                        {journey.duration}
                      </span>
                    </div>
                  </div>
                  {journeyExists && (
                    <ArrowRight className="h-5 w-5 text-gray-400 group-hover:text-primary-600 flex-shrink-0" />
                  )}
                </div>
              );

              return journeyExists ? (
                <Link
                  key={journey.id}
                  to={`/docs/journeys/${journey.id}`}
                  className="block bg-white border border-gray-200 rounded-xl p-5 hover:border-primary-300 hover:shadow cursor-pointer transition-all group"
                >
                  {content}
                </Link>
              ) : (
                <div
                  key={journey.id}
                  className="block bg-white border border-gray-200 rounded-xl p-5 transition-all group"
                >
                  {content}
                </div>
              );
            })}
          </div>
        ) : (
          <div className="bg-gray-50 border border-gray-200 rounded-xl p-6 text-center">
            <BookOpen className="h-12 w-12 text-gray-300 mx-auto mb-3" />
            <p className="text-gray-500">
              Journey documentation for this module is coming soon.
            </p>
            <Link
              to="/docs/journeys"
              className="text-primary-600 hover:underline text-sm mt-2 inline-block"
            >
              Browse available journeys
            </Link>
          </div>
        )}

        {/* Journey Quick Reference */}
        {(existingJourneys.length > 0 || additionalJourneys.length > 0) && (
          <div className="mt-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
            <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
              <FileText className="h-5 w-5 text-blue-600" />
              Understanding User Journeys
            </h4>
            <div className="grid sm:grid-cols-2 gap-4 text-sm">
              <div>
                <h5 className="font-medium text-gray-800 mb-1">
                  Each journey includes:
                </h5>
                <ul className="space-y-1 text-gray-600">
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> Goal and
                    success criteria
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />{" "}
                    Preconditions and prerequisites
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" />{" "}
                    Step-by-step instructions
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> Required
                    permissions (RBAC)
                  </li>
                </ul>
              </div>
              <div>
                <h5 className="font-medium text-gray-800 mb-1">
                  Additional details:
                </h5>
                <ul className="space-y-1 text-gray-600">
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> System
                    responses and validations
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> Edge
                    cases and error handling
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> Related
                    journeys
                  </li>
                  <li className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-green-500" /> Estimated
                    completion time
                  </li>
                </ul>
              </div>
            </div>
          </div>
        )}
      </section>

      {/* API Reference */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          API Reference
        </h2>
        <div className="bg-gray-900 text-gray-100 rounded-lg p-4 overflow-x-auto">
          <pre className="text-sm">
            <code>
              {`# ${module.name} API Endpoints

Base URL: /api/${module.route.replace("/", "")}

# List Operations
GET    /api/${module.route.replace("/", "")}           # List all (paginated)
GET    /api/${module.route.replace("/", "")}/:id       # Get by ID

# Write Operations
POST   /api/${module.route.replace("/", "")}           # Create new
PUT    /api/${module.route.replace("/", "")}/:id       # Update
DELETE /api/${module.route.replace("/", "")}/:id       # Delete (soft)

# Query Parameters
?page=1                    # Page number
&pageSize=20               # Items per page (max 100)
&sortBy=createdAt          # Sort field
&sortOrder=desc            # Sort direction
&search=keyword            # Full-text search`}
            </code>
          </pre>
        </div>
        <p className="text-sm text-gray-500 mt-2 flex items-center gap-1">
          <Code className="h-4 w-4" />
          See{" "}
          <Link
            to="/docs/api-reference"
            className="text-primary-600 hover:underline"
          >
            API Reference
          </Link>{" "}
          for detailed endpoint documentation with request/response examples.
        </p>
      </section>

      {/* Related Modules */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">
          Related Modules
        </h2>
        <div className="grid sm:grid-cols-2 gap-4">
          {module.category === "core" && (
            <>
              <Link
                to="/docs/modules/clinical-visits"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <Stethoscope className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Clinical Visits
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Patient encounters and documentation
                </p>
              </Link>
              <Link
                to="/docs/modules/financial"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <DollarSign className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Financial Management
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Billing and payments
                </p>
              </Link>
            </>
          )}
          {module.category === "clinical" && (
            <>
              <Link
                to="/docs/modules/patient-management"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <Users className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Patient Management
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Patient records and demographics
                </p>
              </Link>
              <Link
                to="/docs/modules/appointments"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <Calendar className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Appointments
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Scheduling and calendar
                </p>
              </Link>
            </>
          )}
          {module.category === "business" && (
            <>
              <Link
                to="/docs/modules/inventory"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <Package className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Inventory Management
                </h3>
                <p className="text-sm text-gray-500 mt-1">Stock and supplies</p>
              </Link>
              <Link
                to="/docs/modules/analytics"
                className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
              >
                <BarChart3 className="h-6 w-6 text-gray-400 group-hover:text-primary-600 mb-2" />
                <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
                  Analytics & Reporting
                </h3>
                <p className="text-sm text-gray-500 mt-1">
                  Business intelligence
                </p>
              </Link>
            </>
          )}
        </div>
      </section>
    </div>
  );
}
