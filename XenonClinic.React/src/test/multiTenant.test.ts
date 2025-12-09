import { describe, it, expect, vi, beforeEach } from 'vitest';
import type {
  TenantContext,
  FeatureMap,
  UISchema,
  FormLayout,
  ListLayout,
  NavItem,
} from '../types/tenant';

// ============================================
// Mock Tenant Context Data
// ============================================

const mockClinicContext: TenantContext = {
  tenantId: 1,
  tenantName: 'Test Clinic',
  companyId: 1,
  companyName: 'Medical Center',
  companyType: 'CLINIC',
  clinicType: 'AUDIOLOGY',
  branchId: 1,
  branchName: 'Main Branch',
  logoUrl: null,
  primaryColor: '#1F6FEB',
  secondaryColor: '#6B7280',
  userId: 'user-1',
  userName: 'Dr. Smith',
  userRoles: ['Admin', 'Doctor'],
  userPermissions: [],
  features: {
    patients: { enabled: true },
    appointments: { enabled: true },
    audiogram: { enabled: true },
    hearingDevices: { enabled: true },
    billing: { enabled: true },
    dental: { enabled: false },
  },
  terminology: {
    'entity.patient.singular': 'Patient',
    'entity.patient.plural': 'Patients',
    'nav.patients': 'Patients',
    'nav.appointments': 'Appointments',
    'action.addPatient': 'Add Patient',
  },
  navigation: [
    { id: 'patients', label: 'nav.patients', icon: 'Users', route: '/patients', featureCode: 'patients', sortOrder: 0 },
    { id: 'appointments', label: 'nav.appointments', icon: 'Calendar', route: '/appointments', featureCode: 'appointments', sortOrder: 1 },
  ],
  uiSchemas: {
    patient: {
      entityName: 'patient',
      displayName: 'entity.patient.singular',
      displayNamePlural: 'entity.patient.plural',
      primaryField: 'fullName',
      fields: [
        { name: 'firstName', type: 'text', label: 'form.firstName', validation: { required: true } },
        { name: 'lastName', type: 'text', label: 'form.lastName', validation: { required: true } },
        { name: 'email', type: 'email', label: 'form.email' },
      ],
    },
  },
  formLayouts: {
    patient: {
      entityName: 'patient',
      sections: [
        { id: 'basic', title: 'Basic Info', columns: 2, fields: ['firstName', 'lastName', 'email'] },
      ],
    },
  },
  listLayouts: {
    patient: {
      entityName: 'patient',
      columns: [
        { field: 'fullName', sortable: true },
        { field: 'email' },
      ],
      actions: {
        row: [{ id: 'edit', label: 'Edit', icon: 'Pencil', type: 'primary' }],
        bulk: [],
        header: [{ id: 'create', label: 'Add', icon: 'Plus', type: 'primary' }],
      },
      filters: [],
      defaultPageSize: 25,
      pageSizeOptions: [10, 25, 50],
      showSearch: true,
      searchFields: ['firstName', 'lastName'],
    },
  },
  settings: {
    currency: 'AED',
    timezone: 'Arabian Standard Time',
    dateFormat: 'dd/MM/yyyy',
    timeFormat: 'HH:mm',
    language: 'en',
  },
};

const mockTradingContext: TenantContext = {
  ...mockClinicContext,
  companyType: 'TRADING',
  clinicType: null,
  terminology: {
    'entity.patient.singular': 'Customer',
    'entity.patient.plural': 'Customers',
    'nav.patients': 'Customers',
    'nav.appointments': 'Meetings',
    'action.addPatient': 'Add Customer',
  },
  features: {
    customers: { enabled: true },
    orders: { enabled: true },
    products: { enabled: true },
    patients: { enabled: false },
    audiogram: { enabled: false },
  },
};

// ============================================
// Terminology Tests
// ============================================

describe('Terminology System', () => {
  // Simple t() function for testing
  const createT = (terminology: Record<string, string>) => {
    return (key: string, fallback?: string): string => {
      return terminology[key] ?? fallback ?? key;
    };
  };

  describe('Clinic Terminology', () => {
    const t = createT(mockClinicContext.terminology);

    it('returns "Patient" for clinic entity', () => {
      expect(t('entity.patient.singular')).toBe('Patient');
    });

    it('returns "Patients" for plural', () => {
      expect(t('entity.patient.plural')).toBe('Patients');
    });

    it('returns "Add Patient" for action', () => {
      expect(t('action.addPatient')).toBe('Add Patient');
    });
  });

  describe('Trading Terminology', () => {
    const t = createT(mockTradingContext.terminology);

    it('returns "Customer" instead of "Patient" for trading', () => {
      expect(t('entity.patient.singular')).toBe('Customer');
    });

    it('returns "Customers" for plural', () => {
      expect(t('entity.patient.plural')).toBe('Customers');
    });

    it('returns "Meetings" instead of "Appointments" for trading', () => {
      expect(t('nav.appointments')).toBe('Meetings');
    });

    it('returns "Add Customer" instead of "Add Patient"', () => {
      expect(t('action.addPatient')).toBe('Add Customer');
    });
  });

  describe('Fallback Behavior', () => {
    const t = createT({});

    it('returns fallback when key not found', () => {
      expect(t('unknown.key', 'Fallback')).toBe('Fallback');
    });

    it('returns key when no fallback provided', () => {
      expect(t('unknown.key')).toBe('unknown.key');
    });
  });
});

// ============================================
// Feature Guard Tests
// ============================================

describe('Feature Guard', () => {
  // Simple hasFeature function for testing
  const createHasFeature = (features: FeatureMap) => {
    return (featureCode: string): boolean => {
      return features[featureCode]?.enabled ?? false;
    };
  };

  describe('Clinic Features', () => {
    const hasFeature = createHasFeature(mockClinicContext.features);

    it('allows access to enabled features', () => {
      expect(hasFeature('patients')).toBe(true);
      expect(hasFeature('appointments')).toBe(true);
      expect(hasFeature('audiogram')).toBe(true);
    });

    it('blocks access to disabled features', () => {
      expect(hasFeature('dental')).toBe(false);
    });

    it('blocks access to non-existent features', () => {
      expect(hasFeature('nonExistentFeature')).toBe(false);
    });
  });

  describe('Trading Features', () => {
    const hasFeature = createHasFeature(mockTradingContext.features);

    it('allows trading features', () => {
      expect(hasFeature('customers')).toBe(true);
      expect(hasFeature('orders')).toBe(true);
      expect(hasFeature('products')).toBe(true);
    });

    it('blocks clinic-specific features for trading', () => {
      expect(hasFeature('patients')).toBe(false);
      expect(hasFeature('audiogram')).toBe(false);
    });
  });
});

// ============================================
// Navigation Tests
// ============================================

describe('Dynamic Navigation', () => {
  // Filter navigation based on features and roles
  const filterNavigation = (
    items: NavItem[],
    features: FeatureMap,
    userRoles: string[]
  ): NavItem[] => {
    return items.filter(item => {
      // Check feature
      if (item.featureCode && !features[item.featureCode]?.enabled) {
        return false;
      }
      // Check roles
      if (item.requiredRoles && item.requiredRoles.length > 0) {
        if (!item.requiredRoles.some(role => userRoles.includes(role))) {
          return false;
        }
      }
      return true;
    });
  };

  it('filters navigation by enabled features', () => {
    const navItems: NavItem[] = [
      { id: 'patients', label: 'Patients', icon: 'Users', route: '/patients', featureCode: 'patients', sortOrder: 0 },
      { id: 'dental', label: 'Dental', icon: 'Smile', route: '/dental', featureCode: 'dental', sortOrder: 1 },
    ];

    const filtered = filterNavigation(navItems, mockClinicContext.features, []);

    expect(filtered).toHaveLength(1);
    expect(filtered[0].id).toBe('patients');
  });

  it('filters navigation by user roles', () => {
    const navItems: NavItem[] = [
      { id: 'dashboard', label: 'Dashboard', icon: 'Home', route: '/', featureCode: 'dashboard', sortOrder: 0 },
      { id: 'admin', label: 'Admin', icon: 'Settings', route: '/admin', featureCode: 'admin', requiredRoles: ['Admin'], sortOrder: 1 },
    ];

    const features: FeatureMap = {
      dashboard: { enabled: true },
      admin: { enabled: true },
    };

    // User with Admin role
    const adminFiltered = filterNavigation(navItems, features, ['Admin']);
    expect(adminFiltered).toHaveLength(2);

    // User without Admin role
    const userFiltered = filterNavigation(navItems, features, ['Doctor']);
    expect(userFiltered).toHaveLength(1);
    expect(userFiltered[0].id).toBe('dashboard');
  });
});

// ============================================
// UI Schema Tests
// ============================================

describe('UI Schema', () => {
  const schema = mockClinicContext.uiSchemas.patient;

  it('has correct entity name', () => {
    expect(schema.entityName).toBe('patient');
  });

  it('has display name terminology key', () => {
    expect(schema.displayName).toBe('entity.patient.singular');
  });

  it('has fields with validation', () => {
    const firstNameField = schema.fields.find(f => f.name === 'firstName');
    expect(firstNameField).toBeDefined();
    expect(firstNameField?.validation?.required).toBe(true);
  });

  it('has field types', () => {
    const emailField = schema.fields.find(f => f.name === 'email');
    expect(emailField?.type).toBe('email');
  });
});

// ============================================
// Form Layout Tests
// ============================================

describe('Form Layout', () => {
  const layout = mockClinicContext.formLayouts.patient;

  it('has sections', () => {
    expect(layout.sections).toHaveLength(1);
    expect(layout.sections[0].id).toBe('basic');
  });

  it('sections have column count', () => {
    expect(layout.sections[0].columns).toBe(2);
  });

  it('sections reference field names', () => {
    expect(layout.sections[0].fields).toContain('firstName');
    expect(layout.sections[0].fields).toContain('lastName');
  });
});

// ============================================
// List Layout Tests
// ============================================

describe('List Layout', () => {
  const layout = mockClinicContext.listLayouts.patient;

  it('has columns', () => {
    expect(layout.columns).toHaveLength(2);
    expect(layout.columns[0].field).toBe('fullName');
  });

  it('has sortable columns', () => {
    expect(layout.columns[0].sortable).toBe(true);
  });

  it('has row actions', () => {
    expect(layout.actions.row).toHaveLength(1);
    expect(layout.actions.row[0].id).toBe('edit');
  });

  it('has header actions', () => {
    expect(layout.actions.header).toHaveLength(1);
    expect(layout.actions.header[0].id).toBe('create');
  });

  it('has search configuration', () => {
    expect(layout.showSearch).toBe(true);
    expect(layout.searchFields).toContain('firstName');
  });
});

// ============================================
// Conditional Visibility Tests
// ============================================

describe('Conditional Rules', () => {
  // Evaluate conditional rules
  const evaluateCondition = (
    condition: { field: string; operator: string; value?: unknown },
    data: Record<string, unknown>
  ): boolean => {
    const fieldValue = data[condition.field];

    switch (condition.operator) {
      case 'eq': return fieldValue === condition.value;
      case 'neq': return fieldValue !== condition.value;
      case 'gt': return Number(fieldValue) > Number(condition.value);
      case 'gte': return Number(fieldValue) >= Number(condition.value);
      case 'lt': return Number(fieldValue) < Number(condition.value);
      case 'lte': return Number(fieldValue) <= Number(condition.value);
      case 'empty': return fieldValue === undefined || fieldValue === null || fieldValue === '';
      case 'notEmpty': return fieldValue !== undefined && fieldValue !== null && fieldValue !== '';
      default: return true;
    }
  };

  it('evaluates eq condition', () => {
    expect(evaluateCondition({ field: 'status', operator: 'eq', value: 'active' }, { status: 'active' })).toBe(true);
    expect(evaluateCondition({ field: 'status', operator: 'eq', value: 'active' }, { status: 'inactive' })).toBe(false);
  });

  it('evaluates neq condition', () => {
    expect(evaluateCondition({ field: 'status', operator: 'neq', value: 'deleted' }, { status: 'active' })).toBe(true);
    expect(evaluateCondition({ field: 'status', operator: 'neq', value: 'active' }, { status: 'active' })).toBe(false);
  });

  it('evaluates numeric comparisons', () => {
    expect(evaluateCondition({ field: 'age', operator: 'gt', value: 18 }, { age: 25 })).toBe(true);
    expect(evaluateCondition({ field: 'age', operator: 'gt', value: 18 }, { age: 15 })).toBe(false);
    expect(evaluateCondition({ field: 'age', operator: 'gte', value: 18 }, { age: 18 })).toBe(true);
    expect(evaluateCondition({ field: 'age', operator: 'lt', value: 18 }, { age: 15 })).toBe(true);
    expect(evaluateCondition({ field: 'age', operator: 'lte', value: 18 }, { age: 18 })).toBe(true);
  });

  it('evaluates empty/notEmpty conditions', () => {
    expect(evaluateCondition({ field: 'email', operator: 'empty' }, { email: '' })).toBe(true);
    expect(evaluateCondition({ field: 'email', operator: 'empty' }, { email: null })).toBe(true);
    expect(evaluateCondition({ field: 'email', operator: 'empty' }, { email: undefined })).toBe(true);
    expect(evaluateCondition({ field: 'email', operator: 'empty' }, { email: 'test@test.com' })).toBe(false);
    expect(evaluateCondition({ field: 'email', operator: 'notEmpty' }, { email: 'test@test.com' })).toBe(true);
  });
});

// ============================================
// Company Type Tests
// ============================================

describe('Company Type Detection', () => {
  it('identifies clinic type correctly', () => {
    expect(mockClinicContext.companyType).toBe('CLINIC');
    expect(mockClinicContext.clinicType).toBe('AUDIOLOGY');
  });

  it('identifies trading type correctly', () => {
    expect(mockTradingContext.companyType).toBe('TRADING');
    expect(mockTradingContext.clinicType).toBeNull();
  });

  it('helper functions work correctly', () => {
    const isClinic = (ctx: TenantContext) => ctx.companyType === 'CLINIC';
    const isTrading = (ctx: TenantContext) => ctx.companyType === 'TRADING';

    expect(isClinic(mockClinicContext)).toBe(true);
    expect(isTrading(mockClinicContext)).toBe(false);
    expect(isClinic(mockTradingContext)).toBe(false);
    expect(isTrading(mockTradingContext)).toBe(true);
  });
});

// ============================================
// Settings Tests
// ============================================

describe('Tenant Settings', () => {
  it('has currency setting', () => {
    expect(mockClinicContext.settings.currency).toBe('AED');
  });

  it('has date format setting', () => {
    expect(mockClinicContext.settings.dateFormat).toBe('dd/MM/yyyy');
  });

  it('has timezone setting', () => {
    expect(mockClinicContext.settings.timezone).toBe('Arabian Standard Time');
  });
});
