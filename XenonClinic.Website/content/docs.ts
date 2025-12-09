import type { DocPage } from '@/types';

export const docs: DocPage[] = [
  {
    slug: 'getting-started',
    title: 'Getting Started',
    description: 'Welcome to XENON! Learn how to set up your account and start using the platform.',
    category: 'Introduction',
    order: 1,
    content: `
# Getting Started with XENON

Welcome to XENON, the configurable ERP and Clinic CRM platform designed for Gulf SMBs. This guide will help you set up your account and start using the platform effectively.

## Creating Your Account

After signing up for XENON, you'll be guided through the initial setup process:

1. **Verify your email** - Check your inbox for the verification link
2. **Create your tenant** - Set up your organization with basic details
3. **Configure company type** - Choose between Clinic or Trading company
4. **Add your first branch** - Set up your primary location
5. **Invite team members** - Add users and assign roles

## Your Dashboard

Once setup is complete, you'll land on your dashboard. The dashboard provides:

- Quick access to common actions
- Today's appointments or orders
- Key metrics and alerts
- Recent activity feed

## Navigation

The main navigation menu on the left provides access to all modules. Available modules depend on your subscription plan and company type:

- **Patients/Customers** - Manage your client records
- **Appointments** - Schedule and manage bookings
- **Billing** - Create invoices and track payments
- **Inventory** - Track stock and supplies
- **Reports** - Access analytics and reports
- **Settings** - Configure your organization

## Getting Help

If you need assistance:

- Browse this documentation
- Contact support at support@xenon.ae
- Access in-app help with the ? icon
    `,
  },
  {
    slug: 'admin-setup',
    title: 'Admin Setup',
    description: 'Configure your organization settings, branches, and system preferences.',
    category: 'Administration',
    order: 2,
    content: `
# Administrative Setup

This guide covers the essential administrative tasks for configuring your XENON tenant.

## Organization Settings

Access Settings > Organization to configure:

### Basic Information
- Organization name and legal entity
- Contact information
- Logo and branding
- Timezone and regional settings

### Business Settings
- Default currency
- Tax configuration
- Invoice numbering format
- Appointment durations

## Branch Configuration

Each branch in XENON operates semi-independently:

### Adding a Branch
1. Navigate to Settings > Branches
2. Click "Add Branch"
3. Enter branch details (name, address, contact)
4. Configure branch-specific settings
5. Assign staff to the branch

### Branch Settings
Each branch can have its own:
- Operating hours
- Resources and rooms
- Inventory
- Staff assignments

## Module Configuration

Enable or disable modules based on your needs:

- Patient/Customer Management (always enabled)
- Appointments & Scheduling
- Billing & Invoicing
- Inventory Management
- Laboratory Module (clinical only)
- Analytics Dashboard

## Data Import

Import existing data from spreadsheets or other systems:

1. Download our import templates
2. Format your data according to the template
3. Upload via Settings > Import Data
4. Review and confirm the import
5. Verify imported records
    `,
  },
  {
    slug: 'users-roles',
    title: 'Users & Roles',
    description: 'Manage team members, assign roles, and configure access permissions.',
    category: 'Administration',
    order: 3,
    content: `
# Users and Roles Management

XENON uses role-based access control (RBAC) to manage what users can see and do within the system.

## Understanding Roles

### System Roles
- **System Admin** - Platform administrators (internal only)

### Tenant Roles
- **Tenant Admin** - Full access to tenant settings and user management
- **Tenant Manager** - Operational management without admin settings
- **Tenant User** - Standard user with role-based permissions

### Custom Roles
Create custom roles to match your organizational structure:
- Receptionist
- Nurse
- Doctor/Physician
- Lab Technician
- Accountant
- Branch Manager

## Adding Users

1. Navigate to Settings > Users
2. Click "Invite User"
3. Enter email address
4. Select role(s)
5. Assign to branch(es)
6. Send invitation

## Permission Configuration

Permissions control access to:

### Features
- Which modules the user can access
- Which actions they can perform

### Data
- Which records they can view
- Which branches they can access

### Actions
- Create, read, update, delete permissions
- Approve/reject workflows
- Export capabilities

## Best Practices

- Follow the principle of least privilege
- Review permissions regularly
- Use audit logs to monitor access
- Separate duties for sensitive operations
    `,
  },
  {
    slug: 'configuration',
    title: 'Configuration',
    description: 'Customize forms, fields, workflows, and system behavior.',
    category: 'Administration',
    order: 4,
    content: `
# System Configuration

XENON's configuration-driven architecture allows you to customize the platform without coding.

## Dynamic Forms

Customize data collection forms:

### Field Configuration
- Add custom fields to entities
- Set field types (text, number, date, select, etc.)
- Configure validation rules
- Control visibility conditions

### Layout Configuration
- Organize fields into sections
- Set column layouts
- Control field ordering
- Define collapsible sections

## Terminology

Adapt the interface language to your business:

### Company Type Terminology
- **Clinic**: Patient, Visit, Doctor
- **Trading**: Customer, Order, Sales Rep

### Custom Labels
Override default labels for:
- Navigation items
- Page titles
- Form labels
- Action buttons

## Workflows

Configure automated workflows:

### Appointment Reminders
- SMS/Email timing
- Message templates
- Follow-up sequences

### Status Transitions
- Define valid status changes
- Require approvals
- Trigger notifications

## Integrations

Configure third-party integrations:

- Payment gateways
- SMS providers
- Email services
- Calendar sync
    `,
  },
  {
    slug: 'integrations',
    title: 'Integrations',
    description: 'Connect XENON with payment gateways, messaging services, and external systems.',
    category: 'Technical',
    order: 5,
    content: `
# Integrations Guide

XENON supports various integrations to extend functionality and connect with your existing tools.

## Payment Gateways

### Supported Gateways
- Stripe (Global)
- Checkout.com (UAE/Global)
- Network International (UAE)
- PayTabs (GCC)

### Configuration
1. Navigate to Settings > Integrations
2. Select payment provider
3. Enter API credentials
4. Configure webhook URLs
5. Test with sandbox mode
6. Enable for production

## Messaging Services

### SMS Providers
- Twilio
- Unifonic (UAE)
- Local telco APIs

### WhatsApp Business
- Send appointment reminders
- Share invoices and receipts
- Two-way messaging support

### Email Services
- SendGrid
- Amazon SES
- SMTP configuration

## Single Sign-On (SSO)

Enterprise customers can enable SSO:

### Supported Protocols
- SAML 2.0
- OAuth 2.0 / OIDC

### Identity Providers
- Azure Active Directory
- Okta
- Google Workspace
- Custom SAML/OIDC providers

## API Access

For custom integrations:

### REST API
- Full CRUD operations
- Secure authentication (JWT)
- Rate limiting
- Comprehensive documentation

### Webhooks
- Real-time event notifications
- Configurable endpoints
- Retry logic
- Signature verification
    `,
  },
  {
    slug: 'deployment',
    title: 'Deployment Options',
    description: 'Learn about cloud hosting and on-premises deployment options.',
    category: 'Technical',
    order: 6,
    content: `
# Deployment Options

XENON offers flexible deployment options to meet your security, compliance, and operational requirements.

## Cloud Hosted

Our default cloud offering provides:

### Benefits
- No infrastructure management
- Automatic updates and patches
- Built-in backup and disaster recovery
- Scalable resources
- 99.9% uptime SLA

### Data Residency
- UAE data center available
- Data remains in-region
- Compliance with local regulations

### Security
- TLS 1.3 encryption in transit
- AES-256 encryption at rest
- SOC 2 Type II practices
- Regular security assessments

## On-Premises

For organizations requiring full control:

### Requirements
- Windows Server 2019+ or Linux (Ubuntu 22.04+)
- SQL Server 2019+ or PostgreSQL 14+
- 8GB RAM minimum (16GB recommended)
- 100GB storage (scalable)

### Installation
Our team provides:
- Installation and configuration
- Initial data migration
- Training and documentation
- Ongoing support contract

### Updates
- Quarterly update packages
- Security patches as needed
- Upgrade assistance included

## Hybrid Options

Combine cloud and on-prem:

- Core system on-premises
- Specific modules in cloud
- Disaster recovery in cloud
- Development/testing in cloud

## Migration Services

Moving from another system:

- Data assessment and mapping
- Automated migration tools
- Validation and testing
- Go-live support
    `,
  },
];

export function getDocBySlug(slug: string): DocPage | undefined {
  return docs.find(d => d.slug === slug);
}

export function getDocsByCategory(category: string): DocPage[] {
  return docs.filter(d => d.category === category).sort((a, b) => a.order - b.order);
}

export const docCategories = [
  'Introduction',
  'Administration',
  'Technical',
] as const;

export function getDocNavigation() {
  return docCategories.map(category => ({
    category,
    pages: getDocsByCategory(category),
  }));
}
