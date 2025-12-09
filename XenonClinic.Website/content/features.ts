import type { FeatureContent } from '@/types';

export const features: FeatureContent[] = [
  {
    slug: 'multi-tenant',
    title: 'Multi-Tenant Architecture',
    shortDescription: 'Isolate data and configurations for each organization while sharing infrastructure.',
    fullDescription: `XENON's multi-tenant architecture enables you to serve multiple organizations from a single deployment while maintaining complete data isolation and security. Each tenant operates in their own secure environment with customized settings, branding, and user access controls.

Our architecture ensures that one tenant's data is never accessible to another, while allowing you to efficiently manage and update the platform for all tenants simultaneously. This approach reduces operational costs while maintaining enterprise-grade security standards.`,
    icon: 'Building2',
    capabilities: [
      'Complete data isolation between tenants',
      'Tenant-specific branding and theming',
      'Independent user management per tenant',
      'Customizable feature flags per tenant',
      'Centralized administration for platform owners',
      'Scalable to thousands of tenants',
    ],
    whoItsFor: [
      'Healthcare groups managing multiple clinics',
      'Franchise operations with multiple locations',
      'SaaS providers offering white-label solutions',
      'Holding companies with diverse business units',
    ],
    category: 'core',
  },
  {
    slug: 'multi-branch',
    title: 'Multi-Branch Management',
    shortDescription: 'Manage multiple locations with unified reporting and branch-specific controls.',
    fullDescription: `Scale your operations across multiple locations with XENON's comprehensive multi-branch management system. Each branch operates semi-independently with its own staff, inventory, and scheduling, while headquarters maintains visibility and control over the entire organization.

Branch managers have the autonomy to handle day-to-day operations while executives access consolidated dashboards that aggregate performance metrics, financial data, and operational insights across all locations. Transfer stock between branches, share patient records (with proper consent), and maintain consistent service quality organization-wide.`,
    icon: 'Network',
    capabilities: [
      'Centralized dashboard for all branches',
      'Branch-specific user roles and permissions',
      'Inter-branch inventory transfers',
      'Consolidated financial reporting',
      'Branch performance comparison',
      'Location-based scheduling and resources',
    ],
    whoItsFor: [
      'Multi-location clinics and hospitals',
      'Retail and trading companies with warehouses',
      'Service businesses with regional offices',
      'Organizations planning expansion',
    ],
    category: 'core',
  },
  {
    slug: 'roles-permissions',
    title: 'Roles & Permissions',
    shortDescription: 'Fine-grained access control with customizable roles for every team member.',
    fullDescription: `Implement the principle of least privilege with XENON's sophisticated role-based access control (RBAC) system. Define exactly what each team member can see and do based on their responsibilities, ensuring data security while enabling productivity.

Create custom roles that match your organizational structure, from receptionist to department head to system administrator. Combine feature access, data visibility, and action permissions to build roles that perfectly fit your workflow. All permission checks are enforced server-side for maximum security.`,
    icon: 'Shield',
    capabilities: [
      'Pre-built roles for common positions',
      'Custom role creation and modification',
      'Granular permission assignments',
      'Feature-level access control',
      'Data-level visibility restrictions',
      'Audit logging of permission changes',
    ],
    whoItsFor: [
      'Healthcare facilities with compliance requirements',
      'Organizations with sensitive data',
      'Companies with complex organizational hierarchies',
      'Businesses requiring segregation of duties',
    ],
    category: 'core',
  },
  {
    slug: 'dynamic-forms',
    title: 'Dynamic Forms & UI',
    shortDescription: 'Configuration-driven interfaces that adapt to your business type and needs.',
    fullDescription: `XENON's dynamic form system eliminates the need for custom development when your requirements change. Forms, fields, and layouts are defined through configuration, allowing administrators to modify data collection without touching code.

Whether you're a dental clinic needing tooth charts, an audiology center requiring audiogram fields, or a trading company tracking customer orders, the same underlying platform adapts to present exactly the right interface. Add custom fields, reorder sections, set validation rules, and control visibility—all through an intuitive admin panel.`,
    icon: 'FormInput',
    capabilities: [
      'Configuration-driven field definitions',
      'Dynamic form layouts per entity type',
      'Conditional field visibility',
      'Custom validation rules',
      'Field-level permissions',
      'Support for all common field types',
    ],
    whoItsFor: [
      'Clinics with specialty-specific data requirements',
      'Businesses with evolving data needs',
      'Organizations wanting to avoid custom development',
      'Companies with multiple business units using the same platform',
    ],
    category: 'core',
  },
  {
    slug: 'appointments',
    title: 'Appointments & Scheduling',
    shortDescription: 'Streamlined booking system with reminders, recurring appointments, and resource management.',
    fullDescription: `Transform your scheduling operations with XENON's comprehensive appointment management system. From simple bookings to complex multi-resource scheduling, handle every scenario with ease while reducing no-shows through automated reminders.

Configure appointment types with custom durations, prepare for specific resources and rooms, and let patients (or customers) book online through your branded portal. The calendar view provides instant visibility into availability, and conflict detection prevents double-bookings automatically.`,
    icon: 'Calendar',
    capabilities: [
      'Visual calendar with drag-and-drop',
      'Online booking portal',
      'SMS and email reminders',
      'Recurring appointment support',
      'Resource and room scheduling',
      'Wait list management',
      'Multi-provider scheduling',
    ],
    whoItsFor: [
      'Medical clinics and healthcare practices',
      'Service-based businesses',
      'Consulting firms',
      'Any organization requiring appointment scheduling',
    ],
    category: 'clinical',
  },
  {
    slug: 'crm-patient-customer',
    title: 'Patient & Customer Management',
    shortDescription: 'Unified CRM that speaks your language—Patients for clinics, Customers for trading.',
    fullDescription: `XENON's intelligent CRM adapts its terminology and workflows based on your business type. Healthcare organizations work with Patients, Visits, and Doctors, while trading companies see Customers, Orders, and Sales Representatives—all powered by the same robust underlying system.

Capture complete relationship histories, track interactions, manage communications, and build lasting relationships. Advanced search and filtering help you find records instantly, while customizable views ensure each team member sees what matters most to their role.`,
    icon: 'Users',
    capabilities: [
      'Dynamic terminology based on company type',
      'Complete interaction history',
      'Custom fields and categories',
      'Document attachment and management',
      'Communication tracking (calls, emails, visits)',
      'Segmentation and tagging',
      'Merge duplicate records',
    ],
    whoItsFor: [
      'Healthcare clinics (Patient-centric)',
      'Trading and retail companies (Customer-centric)',
      'Service businesses of all types',
      'Organizations migrating from spreadsheets or legacy systems',
    ],
    category: 'core',
  },
  {
    slug: 'inventory',
    title: 'Inventory Management',
    shortDescription: 'Track stock levels, manage suppliers, and optimize your supply chain.',
    fullDescription: `Maintain optimal stock levels across all locations with XENON's inventory management module. From medical supplies to retail products, track every item from procurement to consumption with full visibility into quantities, costs, and movement history.

Set reorder points and receive alerts before running low. Process purchase orders, record goods receipts, and manage supplier relationships all in one place. Multi-branch operations benefit from inter-branch transfer capabilities and consolidated procurement.`,
    icon: 'Package',
    capabilities: [
      'Real-time stock tracking',
      'Multi-location inventory',
      'Automated reorder alerts',
      'Supplier management',
      'Purchase order workflow',
      'Goods receipt processing',
      'Stock transfer between branches',
      'Inventory valuation reports',
    ],
    whoItsFor: [
      'Clinics managing medical supplies',
      'Trading companies with product inventory',
      'Pharmacies and dispensaries',
      'Any business tracking physical goods',
    ],
    category: 'operations',
  },
  {
    slug: 'billing-invoicing',
    title: 'Billing & Invoicing',
    shortDescription: 'Generate invoices, process payments, and maintain financial records effortlessly.',
    fullDescription: `Streamline your revenue cycle with XENON's integrated billing and invoicing system. Create professional invoices from visits, appointments, or orders with automatic line item population and tax calculations. Support for partial payments, payment plans, and multiple payment methods ensures flexibility for your customers.

Generate statements, track outstanding balances, and send payment reminders automatically. Integration with popular payment gateways enables online payments, while comprehensive reporting gives you full visibility into your financial performance.`,
    icon: 'Receipt',
    capabilities: [
      'Automated invoice generation',
      'Customizable invoice templates',
      'Multi-currency support',
      'Payment tracking and reconciliation',
      'Insurance claim management (healthcare)',
      'Partial payments and payment plans',
      'Automated payment reminders',
      'Financial reporting and analytics',
    ],
    whoItsFor: [
      'Healthcare providers handling patient billing',
      'Trading companies invoicing customers',
      'Service businesses tracking payments',
      'Organizations needing professional invoicing',
    ],
    category: 'operations',
  },
  {
    slug: 'lab-module',
    title: 'Laboratory Module',
    shortDescription: 'Manage lab orders, results, and workflows for clinical laboratories.',
    fullDescription: `Digitize your laboratory operations with XENON's dedicated lab module. From order entry to result reporting, manage the complete lab workflow with full traceability. Define test catalogs, configure result templates, and enable referring physicians to order tests electronically.

Track sample collection, processing status, and result verification. Integrate with laboratory equipment where supported, and generate patient-friendly result reports. Maintain complete audit trails for regulatory compliance.`,
    icon: 'FlaskConical',
    capabilities: [
      'Electronic lab order entry',
      'Test catalog management',
      'Sample tracking and barcoding',
      'Result entry and verification',
      'Reference range management',
      'Result history and trending',
      'Integration with lab equipment',
      'Compliance audit trails',
    ],
    whoItsFor: [
      'Clinical laboratories',
      'Hospital lab departments',
      'Diagnostic centers',
      'Clinics with in-house testing',
    ],
    category: 'clinical',
  },
  {
    slug: 'analytics-dashboards',
    title: 'Analytics & Dashboards',
    shortDescription: 'Visual insights into your operations with customizable dashboards and reports.',
    fullDescription: `Transform data into actionable insights with XENON's analytics platform. Role-specific dashboards present the metrics that matter most—from patient volumes and revenue trends to inventory turnover and staff productivity.

Drill down from high-level KPIs to detailed transaction data. Schedule automated report delivery to stakeholders, or explore data ad-hoc with flexible filtering and grouping. Export capabilities ensure you can analyze data in your preferred tools when needed.`,
    icon: 'BarChart3',
    capabilities: [
      'Role-based dashboard views',
      'Real-time operational metrics',
      'Customizable chart widgets',
      'Scheduled report delivery',
      'Data export (CSV, Excel, PDF)',
      'Trend analysis and comparisons',
      'Branch and department filtering',
      'Goal tracking and alerts',
    ],
    whoItsFor: [
      'Executives needing operational visibility',
      'Operations managers tracking performance',
      'Finance teams monitoring revenue',
      'Any organization wanting data-driven decisions',
    ],
    category: 'analytics',
  },
  {
    slug: 'audit-trail',
    title: 'Audit Trail & Compliance',
    shortDescription: 'Comprehensive activity logging for security, compliance, and accountability.',
    fullDescription: `Maintain complete visibility into system activity with XENON's comprehensive audit trail. Every significant action—from patient record access to configuration changes—is logged with timestamp, user identification, and relevant details.

Meet regulatory requirements with immutable audit logs that capture who did what, when, and from where. Generate compliance reports, investigate security incidents, and demonstrate accountability to auditors. Configurable retention policies ensure you keep records as long as required.`,
    icon: 'FileSearch',
    capabilities: [
      'Immutable activity logs',
      'User action tracking',
      'Record access logging',
      'Configuration change history',
      'IP address and device tracking',
      'Compliance report generation',
      'Configurable retention policies',
      'Search and filter capabilities',
    ],
    whoItsFor: [
      'Healthcare organizations with compliance requirements',
      'Financial services needing audit trails',
      'Any regulated industry',
      'Organizations prioritizing security and accountability',
    ],
    category: 'core',
  },
  {
    slug: 'integrations',
    title: 'Integrations & API',
    shortDescription: 'Connect with payment gateways, communication services, and third-party systems.',
    fullDescription: `Extend XENON's capabilities by connecting with the tools and services you already use. Our integration framework supports payment gateways, SMS and email providers, identity providers, and custom third-party systems.

For advanced requirements, our RESTful API provides programmatic access to core functionality, enabling custom integrations and automation. Webhook support allows XENON to notify external systems of events in real-time, enabling sophisticated workflows across your technology stack.`,
    icon: 'Plug',
    capabilities: [
      'Payment gateway integrations',
      'SMS and WhatsApp messaging',
      'Email service providers',
      'Single Sign-On (SSO) support',
      'RESTful API access',
      'Webhook notifications',
      'Custom integration development',
      'Pre-built connectors for popular services',
    ],
    whoItsFor: [
      'Organizations with existing technology investments',
      'Businesses requiring custom workflows',
      'Technical teams building integrations',
      'Companies needing payment processing',
    ],
    category: 'core',
  },
];

export function getFeatureBySlug(slug: string): FeatureContent | undefined {
  return features.find(f => f.slug === slug);
}

export function getFeaturesByCategory(category: FeatureContent['category']): FeatureContent[] {
  return features.filter(f => f.category === category);
}

export const featureCategories = [
  { key: 'core', label: 'Core Platform' },
  { key: 'clinical', label: 'Clinical' },
  { key: 'operations', label: 'Operations' },
  { key: 'analytics', label: 'Analytics' },
] as const;
