import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';

// Mock Dashboard component
const MockDashboard = ({
  stats = { patients: 0, appointments: 0, revenue: 0, pending: 0 },
  isLoading = false,
  user = { name: 'Admin', role: 'administrator' }
}: {
  stats?: { patients: number; appointments: number; revenue: number; pending: number };
  isLoading?: boolean;
  user?: { name: string; role: string };
}) => {
  if (isLoading) {
    return <div data-testid="dashboard-loading">Loading dashboard...</div>;
  }

  return (
    <div data-testid="dashboard">
      <header data-testid="dashboard-header">
        <h1>Welcome, {user.name}</h1>
        <span data-testid="user-role">{user.role}</span>
      </header>
      <div data-testid="dashboard-stats">
        <div data-testid="stat-patients">{stats.patients} Patients</div>
        <div data-testid="stat-appointments">{stats.appointments} Appointments</div>
        <div data-testid="stat-revenue">AED {stats.revenue.toLocaleString()}</div>
        <div data-testid="stat-pending">{stats.pending} Pending</div>
      </div>
    </div>
  );
};

// Mock PatientList component
const MockPatientList = ({
  patients = [],
  isLoading = false,
  onAdd,
  onEdit,
  onDelete,
  onSearch,
  searchTerm = ''
}: {
  patients?: any[];
  isLoading?: boolean;
  onAdd?: () => void;
  onEdit?: (patient: any) => void;
  onDelete?: (patient: any) => void;
  onSearch?: (term: string) => void;
  searchTerm?: string;
}) => {
  if (isLoading) {
    return <div data-testid="patient-list-loading">Loading patients...</div>;
  }

  return (
    <div data-testid="patient-list">
      <header>
        <h1>Patients</h1>
        <input
          data-testid="patient-search"
          placeholder="Search patients..."
          value={searchTerm}
          onChange={(e) => onSearch?.(e.target.value)}
        />
        <button data-testid="add-patient-btn" onClick={onAdd}>Add Patient</button>
      </header>
      {patients.length === 0 ? (
        <div data-testid="no-patients">No patients found</div>
      ) : (
        <table data-testid="patient-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Emirates ID</th>
              <th>Phone</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {patients.map((patient, i) => (
              <tr key={i} data-testid={`patient-row-${i}`}>
                <td>{patient.firstName} {patient.lastName}</td>
                <td>{patient.emiratesId}</td>
                <td>{patient.phone}</td>
                <td>
                  <button data-testid={`edit-patient-${i}`} onClick={() => onEdit?.(patient)}>Edit</button>
                  <button data-testid={`delete-patient-${i}`} onClick={() => onDelete?.(patient)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

// Mock AppointmentsList component
const MockAppointmentsList = ({
  appointments = [],
  isLoading = false,
  onAdd,
  onCancel,
  onCheckin,
  selectedDate
}: {
  appointments?: any[];
  isLoading?: boolean;
  onAdd?: () => void;
  onCancel?: (appointment: any) => void;
  onCheckin?: (appointment: any) => void;
  selectedDate?: Date;
}) => {
  if (isLoading) {
    return <div data-testid="appointments-loading">Loading appointments...</div>;
  }

  return (
    <div data-testid="appointments-list">
      <header>
        <h1>Appointments</h1>
        <input type="date" data-testid="date-picker" />
        <button data-testid="add-appointment-btn" onClick={onAdd}>Book Appointment</button>
      </header>
      {appointments.length === 0 ? (
        <div data-testid="no-appointments">No appointments scheduled</div>
      ) : (
        <div data-testid="appointment-cards">
          {appointments.map((apt, i) => (
            <div key={i} data-testid={`appointment-card-${i}`}>
              <span data-testid={`apt-time-${i}`}>{apt.time}</span>
              <span data-testid={`apt-patient-${i}`}>{apt.patientName}</span>
              <span data-testid={`apt-doctor-${i}`}>{apt.doctorName}</span>
              <span data-testid={`apt-type-${i}`}>{apt.type}</span>
              <span data-testid={`apt-status-${i}`}>{apt.status}</span>
              <button data-testid={`checkin-${i}`} onClick={() => onCheckin?.(apt)}>Check In</button>
              <button data-testid={`cancel-${i}`} onClick={() => onCancel?.(apt)}>Cancel</button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

// Mock LabResultsPage component
const MockLabResultsPage = ({
  results = [],
  isLoading = false,
  onView,
  onPrint,
  onVerify,
  filter = 'all'
}: {
  results?: any[];
  isLoading?: boolean;
  onView?: (result: any) => void;
  onPrint?: (result: any) => void;
  onVerify?: (result: any) => void;
  filter?: string;
}) => {
  if (isLoading) {
    return <div data-testid="lab-results-loading">Loading results...</div>;
  }

  return (
    <div data-testid="lab-results-page">
      <header>
        <h1>Lab Results</h1>
        <select data-testid="result-filter" defaultValue={filter}>
          <option value="all">All Results</option>
          <option value="pending">Pending</option>
          <option value="completed">Completed</option>
          <option value="verified">Verified</option>
        </select>
      </header>
      {results.length === 0 ? (
        <div data-testid="no-results">No lab results found</div>
      ) : (
        <table data-testid="results-table">
          <thead>
            <tr>
              <th>Patient</th>
              <th>Test</th>
              <th>Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {results.map((result, i) => (
              <tr key={i} data-testid={`result-row-${i}`}>
                <td>{result.patientName}</td>
                <td>{result.testName}</td>
                <td>{result.date}</td>
                <td data-testid={`result-status-${i}`}>{result.status}</td>
                <td>
                  <button data-testid={`view-result-${i}`} onClick={() => onView?.(result)}>View</button>
                  <button data-testid={`print-result-${i}`} onClick={() => onPrint?.(result)}>Print</button>
                  {result.status === 'pending' && (
                    <button data-testid={`verify-result-${i}`} onClick={() => onVerify?.(result)}>Verify</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

// Mock FinancialPage component
const MockFinancialPage = ({
  invoices = [],
  summary = { total: 0, paid: 0, pending: 0, overdue: 0 },
  isLoading = false,
  onCreateInvoice,
  onViewInvoice,
  onRecordPayment
}: {
  invoices?: any[];
  summary?: { total: number; paid: number; pending: number; overdue: number };
  isLoading?: boolean;
  onCreateInvoice?: () => void;
  onViewInvoice?: (invoice: any) => void;
  onRecordPayment?: (invoice: any) => void;
}) => {
  if (isLoading) {
    return <div data-testid="financial-loading">Loading financial data...</div>;
  }

  return (
    <div data-testid="financial-page">
      <header>
        <h1>Financial Management</h1>
        <button data-testid="create-invoice-btn" onClick={onCreateInvoice}>Create Invoice</button>
      </header>
      <div data-testid="financial-summary">
        <div data-testid="total-revenue">Total: AED {summary.total.toLocaleString()}</div>
        <div data-testid="paid-amount">Paid: AED {summary.paid.toLocaleString()}</div>
        <div data-testid="pending-amount">Pending: AED {summary.pending.toLocaleString()}</div>
        <div data-testid="overdue-amount">Overdue: AED {summary.overdue.toLocaleString()}</div>
      </div>
      {invoices.length === 0 ? (
        <div data-testid="no-invoices">No invoices found</div>
      ) : (
        <table data-testid="invoices-table">
          <tbody>
            {invoices.map((invoice, i) => (
              <tr key={i} data-testid={`invoice-row-${i}`}>
                <td>{invoice.number}</td>
                <td>{invoice.patientName}</td>
                <td>AED {invoice.amount}</td>
                <td data-testid={`invoice-status-${i}`}>{invoice.status}</td>
                <td>
                  <button data-testid={`view-invoice-${i}`} onClick={() => onViewInvoice?.(invoice)}>View</button>
                  {invoice.status === 'pending' && (
                    <button data-testid={`record-payment-${i}`} onClick={() => onRecordPayment?.(invoice)}>Record Payment</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

// Mock InventoryPage component
const MockInventoryPage = ({
  items = [],
  isLoading = false,
  lowStockCount = 0,
  onAddItem,
  onEditItem,
  onAdjustStock
}: {
  items?: any[];
  isLoading?: boolean;
  lowStockCount?: number;
  onAddItem?: () => void;
  onEditItem?: (item: any) => void;
  onAdjustStock?: (item: any) => void;
}) => {
  if (isLoading) {
    return <div data-testid="inventory-loading">Loading inventory...</div>;
  }

  return (
    <div data-testid="inventory-page">
      <header>
        <h1>Inventory Management</h1>
        {lowStockCount > 0 && (
          <span data-testid="low-stock-alert">⚠️ {lowStockCount} items low in stock</span>
        )}
        <button data-testid="add-item-btn" onClick={onAddItem}>Add Item</button>
      </header>
      {items.length === 0 ? (
        <div data-testid="no-items">No inventory items</div>
      ) : (
        <table data-testid="inventory-table">
          <tbody>
            {items.map((item, i) => (
              <tr key={i} data-testid={`item-row-${i}`}>
                <td>{item.name}</td>
                <td>{item.sku}</td>
                <td data-testid={`item-qty-${i}`}>{item.quantity}</td>
                <td>{item.unit}</td>
                <td>
                  <button data-testid={`edit-item-${i}`} onClick={() => onEditItem?.(item)}>Edit</button>
                  <button data-testid={`adjust-stock-${i}`} onClick={() => onAdjustStock?.(item)}>Adjust</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

// Mock HRPage component
const MockHRPage = ({
  employees = [],
  isLoading = false,
  onAddEmployee,
  onViewEmployee,
  onProcessPayroll,
  payrollStatus = 'not_started'
}: {
  employees?: any[];
  isLoading?: boolean;
  onAddEmployee?: () => void;
  onViewEmployee?: (employee: any) => void;
  onProcessPayroll?: () => void;
  payrollStatus?: string;
}) => {
  if (isLoading) {
    return <div data-testid="hr-loading">Loading HR data...</div>;
  }

  return (
    <div data-testid="hr-page">
      <header>
        <h1>HR Management</h1>
        <button data-testid="add-employee-btn" onClick={onAddEmployee}>Add Employee</button>
        <button data-testid="process-payroll-btn" onClick={onProcessPayroll}>Process Payroll</button>
        <span data-testid="payroll-status">Payroll: {payrollStatus}</span>
      </header>
      {employees.length === 0 ? (
        <div data-testid="no-employees">No employees found</div>
      ) : (
        <table data-testid="employees-table">
          <tbody>
            {employees.map((emp, i) => (
              <tr key={i} data-testid={`employee-row-${i}`}>
                <td>{emp.name}</td>
                <td>{emp.department}</td>
                <td>{emp.position}</td>
                <td>{emp.status}</td>
                <td>
                  <button data-testid={`view-employee-${i}`} onClick={() => onViewEmployee?.(emp)}>View</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

// Mock LoginPage component
const MockLoginPage = ({
  onLogin,
  isLoading = false,
  error = ''
}: {
  onLogin?: (credentials: { email: string; password: string }) => void;
  isLoading?: boolean;
  error?: string;
}) => (
  <div data-testid="login-page">
    <h1>Login</h1>
    <form onSubmit={(e) => {
      e.preventDefault();
      const form = e.target as HTMLFormElement;
      const email = (form.elements.namedItem('email') as HTMLInputElement).value;
      const password = (form.elements.namedItem('password') as HTMLInputElement).value;
      onLogin?.({ email, password });
    }}>
      <input name="email" type="email" placeholder="Email" data-testid="login-email" />
      <input name="password" type="password" placeholder="Password" data-testid="login-password" />
      {error && <div data-testid="login-error">{error}</div>}
      <button type="submit" disabled={isLoading} data-testid="login-btn">
        {isLoading ? 'Signing in...' : 'Sign In'}
      </button>
      <a href="/forgot-password" data-testid="forgot-password-link">Forgot Password?</a>
    </form>
  </div>
);

// Tests
describe('Dashboard Page Tests', () => {
  #region Rendering Tests
  describe('Rendering', () => {
    it('renders dashboard', () => {
      render(<MockDashboard />);
      expect(screen.getByTestId('dashboard')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockDashboard isLoading={true} />);
      expect(screen.getByTestId('dashboard-loading')).toBeInTheDocument();
    });

    it('displays user name', () => {
      render(<MockDashboard user={{ name: 'Dr. Ahmed', role: 'doctor' }} />);
      expect(screen.getByText(/Welcome, Dr. Ahmed/)).toBeInTheDocument();
    });

    it('displays user role', () => {
      render(<MockDashboard user={{ name: 'Admin', role: 'administrator' }} />);
      expect(screen.getByTestId('user-role')).toHaveTextContent('administrator');
    });

    it('displays patient count', () => {
      render(<MockDashboard stats={{ patients: 150, appointments: 0, revenue: 0, pending: 0 }} />);
      expect(screen.getByTestId('stat-patients')).toHaveTextContent('150 Patients');
    });

    it('displays appointment count', () => {
      render(<MockDashboard stats={{ patients: 0, appointments: 25, revenue: 0, pending: 0 }} />);
      expect(screen.getByTestId('stat-appointments')).toHaveTextContent('25 Appointments');
    });

    it('displays revenue', () => {
      render(<MockDashboard stats={{ patients: 0, appointments: 0, revenue: 50000, pending: 0 }} />);
      expect(screen.getByTestId('stat-revenue')).toHaveTextContent('AED 50,000');
    });

    it('displays pending count', () => {
      render(<MockDashboard stats={{ patients: 0, appointments: 0, revenue: 0, pending: 5 }} />);
      expect(screen.getByTestId('stat-pending')).toHaveTextContent('5 Pending');
    });

    it('renders dashboard header', () => {
      render(<MockDashboard />);
      expect(screen.getByTestId('dashboard-header')).toBeInTheDocument();
    });

    it('renders dashboard stats section', () => {
      render(<MockDashboard />);
      expect(screen.getByTestId('dashboard-stats')).toBeInTheDocument();
    });
  });
  #endregion

  #region Stats Display Tests
  describe('Stats Display', () => {
    it('displays zero stats correctly', () => {
      render(<MockDashboard stats={{ patients: 0, appointments: 0, revenue: 0, pending: 0 }} />);
      expect(screen.getByTestId('stat-patients')).toHaveTextContent('0 Patients');
    });

    it('displays large patient count', () => {
      render(<MockDashboard stats={{ patients: 10000, appointments: 0, revenue: 0, pending: 0 }} />);
      expect(screen.getByTestId('stat-patients')).toHaveTextContent('10000 Patients');
    });

    it('displays large revenue with formatting', () => {
      render(<MockDashboard stats={{ patients: 0, appointments: 0, revenue: 1000000, pending: 0 }} />);
      expect(screen.getByTestId('stat-revenue')).toHaveTextContent('AED 1,000,000');
    });
  });
  #endregion
});

describe('PatientList Page Tests', () => {
  const mockPatients = [
    { firstName: 'John', lastName: 'Doe', emiratesId: '784-1990-1234567-1', phone: '+971501234567' },
    { firstName: 'Jane', lastName: 'Smith', emiratesId: '784-1985-9876543-2', phone: '+971509876543' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders patient list', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('patient-list')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockPatientList isLoading={true} />);
      expect(screen.getByTestId('patient-list-loading')).toBeInTheDocument();
    });

    it('displays empty state when no patients', () => {
      render(<MockPatientList patients={[]} />);
      expect(screen.getByTestId('no-patients')).toBeInTheDocument();
    });

    it('renders patient table when patients exist', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('patient-table')).toBeInTheDocument();
    });

    it('renders patient rows', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('patient-row-0')).toBeInTheDocument();
      expect(screen.getByTestId('patient-row-1')).toBeInTheDocument();
    });

    it('displays patient names', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('Jane Smith')).toBeInTheDocument();
    });

    it('renders search input', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('patient-search')).toBeInTheDocument();
    });

    it('renders add patient button', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('add-patient-btn')).toBeInTheDocument();
    });

    it('renders edit buttons for each patient', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('edit-patient-0')).toBeInTheDocument();
      expect(screen.getByTestId('edit-patient-1')).toBeInTheDocument();
    });

    it('renders delete buttons for each patient', () => {
      render(<MockPatientList patients={mockPatients} />);
      expect(screen.getByTestId('delete-patient-0')).toBeInTheDocument();
      expect(screen.getByTestId('delete-patient-1')).toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onAdd when add button is clicked', async () => {
      const onAdd = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientList patients={[]} onAdd={onAdd} />);
      await user.click(screen.getByTestId('add-patient-btn'));
      expect(onAdd).toHaveBeenCalled();
    });

    it('calls onEdit when edit button is clicked', async () => {
      const onEdit = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientList patients={mockPatients} onEdit={onEdit} />);
      await user.click(screen.getByTestId('edit-patient-0'));
      expect(onEdit).toHaveBeenCalledWith(mockPatients[0]);
    });

    it('calls onDelete when delete button is clicked', async () => {
      const onDelete = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientList patients={mockPatients} onDelete={onDelete} />);
      await user.click(screen.getByTestId('delete-patient-1'));
      expect(onDelete).toHaveBeenCalledWith(mockPatients[1]);
    });

    it('calls onSearch when typing in search', async () => {
      const onSearch = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientList patients={mockPatients} onSearch={onSearch} />);
      await user.type(screen.getByTestId('patient-search'), 'John');
      expect(onSearch).toHaveBeenCalled();
    });
  });
  #endregion
});

describe('AppointmentsList Page Tests', () => {
  const mockAppointments = [
    { time: '09:00', patientName: 'John Doe', doctorName: 'Dr. Ahmed', type: 'Consultation', status: 'Booked' },
    { time: '10:30', patientName: 'Jane Smith', doctorName: 'Dr. Sarah', type: 'Follow-up', status: 'Confirmed' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders appointments list', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('appointments-list')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockAppointmentsList isLoading={true} />);
      expect(screen.getByTestId('appointments-loading')).toBeInTheDocument();
    });

    it('displays empty state when no appointments', () => {
      render(<MockAppointmentsList appointments={[]} />);
      expect(screen.getByTestId('no-appointments')).toBeInTheDocument();
    });

    it('renders appointment cards', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('appointment-cards')).toBeInTheDocument();
    });

    it('displays appointment times', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('apt-time-0')).toHaveTextContent('09:00');
      expect(screen.getByTestId('apt-time-1')).toHaveTextContent('10:30');
    });

    it('displays patient names', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('apt-patient-0')).toHaveTextContent('John Doe');
    });

    it('displays doctor names', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('apt-doctor-0')).toHaveTextContent('Dr. Ahmed');
    });

    it('displays appointment types', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('apt-type-0')).toHaveTextContent('Consultation');
    });

    it('displays appointment statuses', () => {
      render(<MockAppointmentsList appointments={mockAppointments} />);
      expect(screen.getByTestId('apt-status-0')).toHaveTextContent('Booked');
    });

    it('renders date picker', () => {
      render(<MockAppointmentsList appointments={[]} />);
      expect(screen.getByTestId('date-picker')).toBeInTheDocument();
    });

    it('renders add appointment button', () => {
      render(<MockAppointmentsList appointments={[]} />);
      expect(screen.getByTestId('add-appointment-btn')).toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onAdd when book appointment is clicked', async () => {
      const onAdd = vi.fn();
      const user = userEvent.setup();
      render(<MockAppointmentsList appointments={[]} onAdd={onAdd} />);
      await user.click(screen.getByTestId('add-appointment-btn'));
      expect(onAdd).toHaveBeenCalled();
    });

    it('calls onCheckin when check in is clicked', async () => {
      const onCheckin = vi.fn();
      const user = userEvent.setup();
      render(<MockAppointmentsList appointments={mockAppointments} onCheckin={onCheckin} />);
      await user.click(screen.getByTestId('checkin-0'));
      expect(onCheckin).toHaveBeenCalledWith(mockAppointments[0]);
    });

    it('calls onCancel when cancel is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockAppointmentsList appointments={mockAppointments} onCancel={onCancel} />);
      await user.click(screen.getByTestId('cancel-0'));
      expect(onCancel).toHaveBeenCalledWith(mockAppointments[0]);
    });
  });
  #endregion
});

describe('LabResults Page Tests', () => {
  const mockResults = [
    { patientName: 'John Doe', testName: 'CBC', date: '2025-01-15', status: 'pending' },
    { patientName: 'Jane Smith', testName: 'Lipid Panel', date: '2025-01-14', status: 'completed' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders lab results page', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.getByTestId('lab-results-page')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockLabResultsPage isLoading={true} />);
      expect(screen.getByTestId('lab-results-loading')).toBeInTheDocument();
    });

    it('displays empty state when no results', () => {
      render(<MockLabResultsPage results={[]} />);
      expect(screen.getByTestId('no-results')).toBeInTheDocument();
    });

    it('renders results table', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.getByTestId('results-table')).toBeInTheDocument();
    });

    it('renders filter dropdown', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.getByTestId('result-filter')).toBeInTheDocument();
    });

    it('displays result statuses', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.getByTestId('result-status-0')).toHaveTextContent('pending');
      expect(screen.getByTestId('result-status-1')).toHaveTextContent('completed');
    });

    it('renders verify button for pending results', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.getByTestId('verify-result-0')).toBeInTheDocument();
    });

    it('does not render verify button for completed results', () => {
      render(<MockLabResultsPage results={mockResults} />);
      expect(screen.queryByTestId('verify-result-1')).not.toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onView when view is clicked', async () => {
      const onView = vi.fn();
      const user = userEvent.setup();
      render(<MockLabResultsPage results={mockResults} onView={onView} />);
      await user.click(screen.getByTestId('view-result-0'));
      expect(onView).toHaveBeenCalledWith(mockResults[0]);
    });

    it('calls onPrint when print is clicked', async () => {
      const onPrint = vi.fn();
      const user = userEvent.setup();
      render(<MockLabResultsPage results={mockResults} onPrint={onPrint} />);
      await user.click(screen.getByTestId('print-result-0'));
      expect(onPrint).toHaveBeenCalledWith(mockResults[0]);
    });

    it('calls onVerify when verify is clicked', async () => {
      const onVerify = vi.fn();
      const user = userEvent.setup();
      render(<MockLabResultsPage results={mockResults} onVerify={onVerify} />);
      await user.click(screen.getByTestId('verify-result-0'));
      expect(onVerify).toHaveBeenCalledWith(mockResults[0]);
    });
  });
  #endregion
});

describe('Financial Page Tests', () => {
  const mockInvoices = [
    { number: 'INV-001', patientName: 'John Doe', amount: 500, status: 'pending' },
    { number: 'INV-002', patientName: 'Jane Smith', amount: 750, status: 'paid' }
  ];

  const mockSummary = { total: 50000, paid: 35000, pending: 10000, overdue: 5000 };

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders financial page', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('financial-page')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockFinancialPage isLoading={true} />);
      expect(screen.getByTestId('financial-loading')).toBeInTheDocument();
    });

    it('displays empty state when no invoices', () => {
      render(<MockFinancialPage invoices={[]} summary={mockSummary} />);
      expect(screen.getByTestId('no-invoices')).toBeInTheDocument();
    });

    it('displays financial summary', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('financial-summary')).toBeInTheDocument();
    });

    it('displays total revenue', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('total-revenue')).toHaveTextContent('AED 50,000');
    });

    it('displays paid amount', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('paid-amount')).toHaveTextContent('AED 35,000');
    });

    it('displays pending amount', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('pending-amount')).toHaveTextContent('AED 10,000');
    });

    it('displays overdue amount', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('overdue-amount')).toHaveTextContent('AED 5,000');
    });

    it('renders create invoice button', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('create-invoice-btn')).toBeInTheDocument();
    });

    it('renders record payment button for pending invoices', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.getByTestId('record-payment-0')).toBeInTheDocument();
    });

    it('does not render record payment button for paid invoices', () => {
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} />);
      expect(screen.queryByTestId('record-payment-1')).not.toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onCreateInvoice when create invoice is clicked', async () => {
      const onCreateInvoice = vi.fn();
      const user = userEvent.setup();
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} onCreateInvoice={onCreateInvoice} />);
      await user.click(screen.getByTestId('create-invoice-btn'));
      expect(onCreateInvoice).toHaveBeenCalled();
    });

    it('calls onViewInvoice when view is clicked', async () => {
      const onViewInvoice = vi.fn();
      const user = userEvent.setup();
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} onViewInvoice={onViewInvoice} />);
      await user.click(screen.getByTestId('view-invoice-0'));
      expect(onViewInvoice).toHaveBeenCalledWith(mockInvoices[0]);
    });

    it('calls onRecordPayment when record payment is clicked', async () => {
      const onRecordPayment = vi.fn();
      const user = userEvent.setup();
      render(<MockFinancialPage invoices={mockInvoices} summary={mockSummary} onRecordPayment={onRecordPayment} />);
      await user.click(screen.getByTestId('record-payment-0'));
      expect(onRecordPayment).toHaveBeenCalledWith(mockInvoices[0]);
    });
  });
  #endregion
});

describe('Inventory Page Tests', () => {
  const mockItems = [
    { name: 'Bandages', sku: 'BND-001', quantity: 100, unit: 'pcs' },
    { name: 'Syringes', sku: 'SYR-001', quantity: 5, unit: 'box' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders inventory page', () => {
      render(<MockInventoryPage items={mockItems} />);
      expect(screen.getByTestId('inventory-page')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockInventoryPage isLoading={true} />);
      expect(screen.getByTestId('inventory-loading')).toBeInTheDocument();
    });

    it('displays empty state when no items', () => {
      render(<MockInventoryPage items={[]} />);
      expect(screen.getByTestId('no-items')).toBeInTheDocument();
    });

    it('displays low stock alert when applicable', () => {
      render(<MockInventoryPage items={mockItems} lowStockCount={3} />);
      expect(screen.getByTestId('low-stock-alert')).toHaveTextContent('3 items low in stock');
    });

    it('does not display low stock alert when count is 0', () => {
      render(<MockInventoryPage items={mockItems} lowStockCount={0} />);
      expect(screen.queryByTestId('low-stock-alert')).not.toBeInTheDocument();
    });

    it('renders inventory table', () => {
      render(<MockInventoryPage items={mockItems} />);
      expect(screen.getByTestId('inventory-table')).toBeInTheDocument();
    });

    it('displays item quantities', () => {
      render(<MockInventoryPage items={mockItems} />);
      expect(screen.getByTestId('item-qty-0')).toHaveTextContent('100');
      expect(screen.getByTestId('item-qty-1')).toHaveTextContent('5');
    });

    it('renders add item button', () => {
      render(<MockInventoryPage items={mockItems} />);
      expect(screen.getByTestId('add-item-btn')).toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onAddItem when add item is clicked', async () => {
      const onAddItem = vi.fn();
      const user = userEvent.setup();
      render(<MockInventoryPage items={mockItems} onAddItem={onAddItem} />);
      await user.click(screen.getByTestId('add-item-btn'));
      expect(onAddItem).toHaveBeenCalled();
    });

    it('calls onEditItem when edit is clicked', async () => {
      const onEditItem = vi.fn();
      const user = userEvent.setup();
      render(<MockInventoryPage items={mockItems} onEditItem={onEditItem} />);
      await user.click(screen.getByTestId('edit-item-0'));
      expect(onEditItem).toHaveBeenCalledWith(mockItems[0]);
    });

    it('calls onAdjustStock when adjust is clicked', async () => {
      const onAdjustStock = vi.fn();
      const user = userEvent.setup();
      render(<MockInventoryPage items={mockItems} onAdjustStock={onAdjustStock} />);
      await user.click(screen.getByTestId('adjust-stock-0'));
      expect(onAdjustStock).toHaveBeenCalledWith(mockItems[0]);
    });
  });
  #endregion
});

describe('HR Page Tests', () => {
  const mockEmployees = [
    { name: 'Ahmed Ali', department: 'Medical', position: 'Doctor', status: 'Active' },
    { name: 'Sara Khan', department: 'Nursing', position: 'Nurse', status: 'Active' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders HR page', () => {
      render(<MockHRPage employees={mockEmployees} />);
      expect(screen.getByTestId('hr-page')).toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockHRPage isLoading={true} />);
      expect(screen.getByTestId('hr-loading')).toBeInTheDocument();
    });

    it('displays empty state when no employees', () => {
      render(<MockHRPage employees={[]} />);
      expect(screen.getByTestId('no-employees')).toBeInTheDocument();
    });

    it('displays payroll status', () => {
      render(<MockHRPage employees={mockEmployees} payrollStatus="processed" />);
      expect(screen.getByTestId('payroll-status')).toHaveTextContent('Payroll: processed');
    });

    it('renders employees table', () => {
      render(<MockHRPage employees={mockEmployees} />);
      expect(screen.getByTestId('employees-table')).toBeInTheDocument();
    });

    it('renders add employee button', () => {
      render(<MockHRPage employees={mockEmployees} />);
      expect(screen.getByTestId('add-employee-btn')).toBeInTheDocument();
    });

    it('renders process payroll button', () => {
      render(<MockHRPage employees={mockEmployees} />);
      expect(screen.getByTestId('process-payroll-btn')).toBeInTheDocument();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('calls onAddEmployee when add employee is clicked', async () => {
      const onAddEmployee = vi.fn();
      const user = userEvent.setup();
      render(<MockHRPage employees={mockEmployees} onAddEmployee={onAddEmployee} />);
      await user.click(screen.getByTestId('add-employee-btn'));
      expect(onAddEmployee).toHaveBeenCalled();
    });

    it('calls onViewEmployee when view is clicked', async () => {
      const onViewEmployee = vi.fn();
      const user = userEvent.setup();
      render(<MockHRPage employees={mockEmployees} onViewEmployee={onViewEmployee} />);
      await user.click(screen.getByTestId('view-employee-0'));
      expect(onViewEmployee).toHaveBeenCalledWith(mockEmployees[0]);
    });

    it('calls onProcessPayroll when process payroll is clicked', async () => {
      const onProcessPayroll = vi.fn();
      const user = userEvent.setup();
      render(<MockHRPage employees={mockEmployees} onProcessPayroll={onProcessPayroll} />);
      await user.click(screen.getByTestId('process-payroll-btn'));
      expect(onProcessPayroll).toHaveBeenCalled();
    });
  });
  #endregion
});

describe('Login Page Tests', () => {
  #region Rendering Tests
  describe('Rendering', () => {
    it('renders login page', () => {
      render(<MockLoginPage />);
      expect(screen.getByTestId('login-page')).toBeInTheDocument();
    });

    it('renders email input', () => {
      render(<MockLoginPage />);
      expect(screen.getByTestId('login-email')).toBeInTheDocument();
    });

    it('renders password input', () => {
      render(<MockLoginPage />);
      expect(screen.getByTestId('login-password')).toBeInTheDocument();
    });

    it('renders login button', () => {
      render(<MockLoginPage />);
      expect(screen.getByTestId('login-btn')).toBeInTheDocument();
    });

    it('renders forgot password link', () => {
      render(<MockLoginPage />);
      expect(screen.getByTestId('forgot-password-link')).toBeInTheDocument();
    });

    it('displays error message when provided', () => {
      render(<MockLoginPage error="Invalid credentials" />);
      expect(screen.getByTestId('login-error')).toHaveTextContent('Invalid credentials');
    });

    it('does not display error when not provided', () => {
      render(<MockLoginPage />);
      expect(screen.queryByTestId('login-error')).not.toBeInTheDocument();
    });

    it('displays loading state', () => {
      render(<MockLoginPage isLoading={true} />);
      expect(screen.getByTestId('login-btn')).toHaveTextContent('Signing in...');
    });

    it('disables button when loading', () => {
      render(<MockLoginPage isLoading={true} />);
      expect(screen.getByTestId('login-btn')).toBeDisabled();
    });
  });
  #endregion

  #region Interaction Tests
  describe('Interactions', () => {
    it('allows typing email', async () => {
      const user = userEvent.setup();
      render(<MockLoginPage />);
      const input = screen.getByTestId('login-email');
      await user.type(input, 'test@example.com');
      expect(input).toHaveValue('test@example.com');
    });

    it('allows typing password', async () => {
      const user = userEvent.setup();
      render(<MockLoginPage />);
      const input = screen.getByTestId('login-password');
      await user.type(input, 'password123');
      expect(input).toHaveValue('password123');
    });

    it('calls onLogin with credentials when form is submitted', async () => {
      const onLogin = vi.fn();
      const user = userEvent.setup();
      render(<MockLoginPage onLogin={onLogin} />);

      await user.type(screen.getByTestId('login-email'), 'test@example.com');
      await user.type(screen.getByTestId('login-password'), 'password123');
      await user.click(screen.getByTestId('login-btn'));

      expect(onLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123'
      });
    });

    it('submits form on enter key in password field', async () => {
      const onLogin = vi.fn();
      const user = userEvent.setup();
      render(<MockLoginPage onLogin={onLogin} />);

      await user.type(screen.getByTestId('login-email'), 'test@example.com');
      await user.type(screen.getByTestId('login-password'), 'password123{enter}');

      expect(onLogin).toHaveBeenCalled();
    });
  });
  #endregion

  #region Edge Cases
  describe('Edge Cases', () => {
    it('handles empty email submission', async () => {
      const onLogin = vi.fn();
      const user = userEvent.setup();
      render(<MockLoginPage onLogin={onLogin} />);

      await user.type(screen.getByTestId('login-password'), 'password123');
      await user.click(screen.getByTestId('login-btn'));

      expect(onLogin).toHaveBeenCalledWith({
        email: '',
        password: 'password123'
      });
    });

    it('handles empty password submission', async () => {
      const onLogin = vi.fn();
      const user = userEvent.setup();
      render(<MockLoginPage onLogin={onLogin} />);

      await user.type(screen.getByTestId('login-email'), 'test@example.com');
      await user.click(screen.getByTestId('login-btn'));

      expect(onLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: ''
      });
    });

    it('handles special characters in email', async () => {
      const user = userEvent.setup();
      render(<MockLoginPage />);

      await user.type(screen.getByTestId('login-email'), 'test+special@example.com');
      expect(screen.getByTestId('login-email')).toHaveValue('test+special@example.com');
    });
  });
  #endregion
});
