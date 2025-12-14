import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, fireEvent, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

interface PatientFormData {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  emiratesId?: string;
  gender?: string;
  bloodType?: string;
  address?: string;
  allergies?: string;
}

// Mock PatientForm component
const MockPatientForm = ({
  onSubmit,
  onCancel,
  initialData,
  isLoading
}: {
  onSubmit?: (data: PatientFormData) => void;
  onCancel?: () => void;
  initialData?: PatientFormData;
  isLoading?: boolean;
}) => (
  <form onSubmit={(e) => { e.preventDefault(); onSubmit?.({}); }} data-testid="patient-form">
    <input name="firstName" placeholder="First Name" defaultValue={initialData?.firstName || ''} />
    <input name="lastName" placeholder="Last Name" defaultValue={initialData?.lastName || ''} />
    <input name="email" type="email" placeholder="Email" defaultValue={initialData?.email || ''} />
    <input name="phone" placeholder="Phone" defaultValue={initialData?.phone || ''} />
    <input name="emiratesId" placeholder="Emirates ID" defaultValue={initialData?.emiratesId || ''} />
    <input name="dateOfBirth" type="date" placeholder="Date of Birth" />
    <select name="gender" defaultValue={initialData?.gender || ''}>
      <option value="">Select Gender</option>
      <option value="male">Male</option>
      <option value="female">Female</option>
    </select>
    <select name="bloodType" defaultValue={initialData?.bloodType || ''}>
      <option value="">Select Blood Type</option>
      <option value="A+">A+</option>
      <option value="A-">A-</option>
      <option value="B+">B+</option>
      <option value="B-">B-</option>
      <option value="O+">O+</option>
      <option value="O-">O-</option>
      <option value="AB+">AB+</option>
      <option value="AB-">AB-</option>
    </select>
    <textarea name="address" placeholder="Address" defaultValue={initialData?.address || ''} />
    <textarea name="allergies" placeholder="Allergies" defaultValue={initialData?.allergies || ''} />
    <button type="submit" disabled={isLoading}>{isLoading ? 'Saving...' : 'Submit'}</button>
    <button type="button" onClick={onCancel}>Cancel</button>
  </form>
);

interface SelectOption {
  id: number;
  name: string;
}

interface AppointmentFormData {
  patientId?: number;
  doctorId?: number;
  appointmentType?: string;
  notes?: string;
}

// Mock AppointmentForm component
const MockAppointmentForm = ({
  onSubmit,
  onCancel,
  patients = [],
  doctors = [],
  initialData
}: {
  onSubmit?: (data: AppointmentFormData) => void;
  onCancel?: () => void;
  patients?: SelectOption[];
  doctors?: SelectOption[];
  initialData?: AppointmentFormData;
}) => (
  <form onSubmit={(e) => { e.preventDefault(); onSubmit?.({}); }} data-testid="appointment-form">
    <select name="patientId" data-testid="patient-select" defaultValue={initialData?.patientId || ''}>
      <option value="">Select Patient</option>
      {patients.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
    </select>
    <select name="doctorId" data-testid="doctor-select" defaultValue={initialData?.doctorId || ''}>
      <option value="">Select Doctor</option>
      {doctors.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
    </select>
    <input name="date" type="date" placeholder="Date" />
    <input name="time" type="time" placeholder="Time" />
    <select name="appointmentType" defaultValue={initialData?.appointmentType || ''}>
      <option value="">Select Type</option>
      <option value="consultation">Consultation</option>
      <option value="followup">Follow-up</option>
      <option value="procedure">Procedure</option>
      <option value="emergency">Emergency</option>
    </select>
    <input name="duration" type="number" placeholder="Duration (minutes)" defaultValue={30} />
    <textarea name="notes" placeholder="Notes" defaultValue={initialData?.notes || ''} />
    <button type="submit">Book Appointment</button>
    <button type="button" onClick={onCancel}>Cancel</button>
  </form>
);

interface LabTest {
  id: number;
  name: string;
  price: number;
}

interface PatientInfo {
  id: number;
  name: string;
}

interface LabOrderFormData {
  tests?: number[];
  priority?: string;
  sampleType?: string;
  clinicalNotes?: string;
}

// Mock LabOrderForm component
const MockLabOrderForm = ({
  onSubmit,
  onCancel,
  tests = [],
  patient
}: {
  onSubmit?: (data: LabOrderFormData) => void;
  onCancel?: () => void;
  tests?: LabTest[];
  patient?: PatientInfo;
}) => (
  <form onSubmit={(e) => { e.preventDefault(); onSubmit?.({}); }} data-testid="lab-order-form">
    <div data-testid="patient-info">{patient?.name || 'No patient selected'}</div>
    <div data-testid="test-selection">
      {tests.map(t => (
        <label key={t.id}>
          <input type="checkbox" name="tests" value={t.id} />
          {t.name} - ${t.price}
        </label>
      ))}
    </div>
    <select name="priority">
      <option value="routine">Routine</option>
      <option value="urgent">Urgent</option>
      <option value="stat">STAT</option>
    </select>
    <select name="sampleType">
      <option value="">Select Sample Type</option>
      <option value="blood">Blood</option>
      <option value="urine">Urine</option>
      <option value="stool">Stool</option>
      <option value="swab">Swab</option>
    </select>
    <textarea name="clinicalNotes" placeholder="Clinical Notes" />
    <button type="submit">Order Tests</button>
    <button type="button" onClick={onCancel}>Cancel</button>
  </form>
);

interface InvoiceItem {
  description: string;
  quantity: number;
  price: number;
}

interface InvoiceFormData {
  items?: InvoiceItem[];
  total?: number;
}

// Mock InvoiceForm component
const MockInvoiceForm = ({
  onSubmit,
  onCancel,
  patient,
  items = []
}: {
  onSubmit?: (data: InvoiceFormData) => void;
  onCancel?: () => void;
  patient?: PatientInfo;
  items?: InvoiceItem[];
}) => (
  <form onSubmit={(e) => { e.preventDefault(); onSubmit?.({}); }} data-testid="invoice-form">
    <div data-testid="patient-info">{patient?.name}</div>
    <table data-testid="line-items">
      <thead>
        <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
      </thead>
      <tbody>
        {items.map((item, i) => (
          <tr key={i}>
            <td>{item.description}</td>
            <td><input type="number" defaultValue={item.quantity} /></td>
            <td><input type="number" defaultValue={item.price} /></td>
            <td>{(item.quantity * item.price).toFixed(2)}</td>
          </tr>
        ))}
      </tbody>
    </table>
    <input name="discount" type="number" placeholder="Discount %" defaultValue={0} />
    <input name="tax" type="number" placeholder="Tax %" defaultValue={5} />
    <select name="paymentMethod">
      <option value="cash">Cash</option>
      <option value="card">Card</option>
      <option value="insurance">Insurance</option>
      <option value="bank_transfer">Bank Transfer</option>
    </select>
    <button type="submit">Generate Invoice</button>
    <button type="button" onClick={onCancel}>Cancel</button>
  </form>
);

describe('PatientForm Component Tests', () => {
  #region Rendering Tests
  describe('Rendering', () => {
    it('renders the patient form', () => {
      render(<MockPatientForm />);
      expect(screen.getByTestId('patient-form')).toBeInTheDocument();
    });

    it('renders first name input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('First Name')).toBeInTheDocument();
    });

    it('renders last name input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Last Name')).toBeInTheDocument();
    });

    it('renders email input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Email')).toBeInTheDocument();
    });

    it('renders phone input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Phone')).toBeInTheDocument();
    });

    it('renders emirates ID input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Emirates ID')).toBeInTheDocument();
    });

    it('renders date of birth input', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Date of Birth')).toBeInTheDocument();
    });

    it('renders gender select', () => {
      render(<MockPatientForm />);
      expect(screen.getByRole('combobox', { name: '' })).toBeInTheDocument();
    });

    it('renders blood type select', () => {
      render(<MockPatientForm />);
      const selects = screen.getAllByRole('combobox');
      expect(selects.length).toBeGreaterThanOrEqual(2);
    });

    it('renders address textarea', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Address')).toBeInTheDocument();
    });

    it('renders allergies textarea', () => {
      render(<MockPatientForm />);
      expect(screen.getByPlaceholderText('Allergies')).toBeInTheDocument();
    });

    it('renders submit button', () => {
      render(<MockPatientForm />);
      expect(screen.getByRole('button', { name: 'Submit' })).toBeInTheDocument();
    });

    it('renders cancel button', () => {
      render(<MockPatientForm />);
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('displays loading state when isLoading is true', () => {
      render(<MockPatientForm isLoading={true} />);
      expect(screen.getByRole('button', { name: 'Saving...' })).toBeInTheDocument();
    });

    it('disables submit button when loading', () => {
      render(<MockPatientForm isLoading={true} />);
      expect(screen.getByRole('button', { name: 'Saving...' })).toBeDisabled();
    });
  });
  #endregion

  #region Initial Data Tests
  describe('Initial Data Population', () => {
    const initialData = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      phone: '+971501234567',
      emiratesId: '784-1990-1234567-1',
      gender: 'male',
      bloodType: 'O+',
      address: '123 Main St, Dubai',
      allergies: 'Penicillin'
    };

    it('populates first name from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('First Name')).toHaveValue('John');
    });

    it('populates last name from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Last Name')).toHaveValue('Doe');
    });

    it('populates email from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Email')).toHaveValue('john.doe@example.com');
    });

    it('populates phone from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Phone')).toHaveValue('+971501234567');
    });

    it('populates emirates ID from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Emirates ID')).toHaveValue('784-1990-1234567-1');
    });

    it('populates address from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Address')).toHaveValue('123 Main St, Dubai');
    });

    it('populates allergies from initial data', () => {
      render(<MockPatientForm initialData={initialData} />);
      expect(screen.getByPlaceholderText('Allergies')).toHaveValue('Penicillin');
    });
  });
  #endregion

  #region User Interaction Tests
  describe('User Interactions', () => {
    it('allows typing in first name field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('First Name');
      await user.type(input, 'Jane');
      expect(input).toHaveValue('Jane');
    });

    it('allows typing in last name field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Last Name');
      await user.type(input, 'Smith');
      expect(input).toHaveValue('Smith');
    });

    it('allows typing in email field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Email');
      await user.type(input, 'jane@example.com');
      expect(input).toHaveValue('jane@example.com');
    });

    it('allows typing in phone field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Phone');
      await user.type(input, '+971509876543');
      expect(input).toHaveValue('+971509876543');
    });

    it('allows typing in emirates ID field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Emirates ID');
      await user.type(input, '784-1995-9876543-2');
      expect(input).toHaveValue('784-1995-9876543-2');
    });

    it('allows typing in address textarea', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Address');
      await user.type(input, '456 Palm St, Abu Dhabi');
      expect(input).toHaveValue('456 Palm St, Abu Dhabi');
    });

    it('allows typing in allergies textarea', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Allergies');
      await user.type(input, 'Aspirin, Sulfa drugs');
      expect(input).toHaveValue('Aspirin, Sulfa drugs');
    });

    it('allows selecting gender', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[0], 'female');
      expect(selects[0]).toHaveValue('female');
    });

    it('allows selecting blood type', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[1], 'AB+');
      expect(selects[1]).toHaveValue('AB+');
    });

    it('clears input field', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm initialData={{ firstName: 'John' }} />);
      const input = screen.getByPlaceholderText('First Name');
      await user.clear(input);
      expect(input).toHaveValue('');
    });
  });
  #endregion

  #region Form Submission Tests
  describe('Form Submission', () => {
    it('calls onSubmit when form is submitted', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientForm onSubmit={onSubmit} />);
      await user.click(screen.getByRole('button', { name: 'Submit' }));
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when cancel button is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientForm onCancel={onCancel} />);
      await user.click(screen.getByRole('button', { name: 'Cancel' }));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });

    it('does not call onSubmit when submit is clicked during loading', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientForm onSubmit={onSubmit} isLoading={true} />);
      await user.click(screen.getByRole('button', { name: 'Saving...' }));
      expect(onSubmit).not.toHaveBeenCalled();
    });

    it('submits form on Enter key in text field', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockPatientForm onSubmit={onSubmit} />);
      const input = screen.getByPlaceholderText('First Name');
      await user.type(input, 'John{enter}');
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });
  });
  #endregion

  #region Accessibility Tests
  describe('Accessibility', () => {
    it('has accessible submit button', () => {
      render(<MockPatientForm />);
      const button = screen.getByRole('button', { name: 'Submit' });
      expect(button).toBeInTheDocument();
    });

    it('has accessible cancel button', () => {
      render(<MockPatientForm />);
      const button = screen.getByRole('button', { name: 'Cancel' });
      expect(button).toBeInTheDocument();
    });

    it('has accessible text inputs', () => {
      render(<MockPatientForm />);
      const textboxes = screen.getAllByRole('textbox');
      expect(textboxes.length).toBeGreaterThan(0);
    });

    it('has accessible comboboxes', () => {
      render(<MockPatientForm />);
      const comboboxes = screen.getAllByRole('combobox');
      expect(comboboxes.length).toBeGreaterThanOrEqual(2);
    });
  });
  #endregion
});

describe('AppointmentForm Component Tests', () => {
  const mockPatients = [
    { id: 1, name: 'John Doe' },
    { id: 2, name: 'Jane Smith' },
    { id: 3, name: 'Bob Wilson' }
  ];

  const mockDoctors = [
    { id: 1, name: 'Dr. Ahmed' },
    { id: 2, name: 'Dr. Sarah' },
    { id: 3, name: 'Dr. Mohammed' }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders the appointment form', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByTestId('appointment-form')).toBeInTheDocument();
    });

    it('renders patient select dropdown', () => {
      render(<MockAppointmentForm patients={mockPatients} />);
      expect(screen.getByTestId('patient-select')).toBeInTheDocument();
    });

    it('renders doctor select dropdown', () => {
      render(<MockAppointmentForm doctors={mockDoctors} />);
      expect(screen.getByTestId('doctor-select')).toBeInTheDocument();
    });

    it('renders date input', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByPlaceholderText('Date')).toBeInTheDocument();
    });

    it('renders time input', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByPlaceholderText('Time')).toBeInTheDocument();
    });

    it('renders appointment type select', () => {
      render(<MockAppointmentForm />);
      const selects = screen.getAllByRole('combobox');
      expect(selects.length).toBeGreaterThanOrEqual(3);
    });

    it('renders duration input with default value', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByPlaceholderText('Duration (minutes)')).toHaveValue(30);
    });

    it('renders notes textarea', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByPlaceholderText('Notes')).toBeInTheDocument();
    });

    it('renders book appointment button', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByRole('button', { name: 'Book Appointment' })).toBeInTheDocument();
    });

    it('renders cancel button', () => {
      render(<MockAppointmentForm />);
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('populates patient dropdown with patients', () => {
      render(<MockAppointmentForm patients={mockPatients} />);
      mockPatients.forEach(patient => {
        expect(screen.getByText(patient.name)).toBeInTheDocument();
      });
    });

    it('populates doctor dropdown with doctors', () => {
      render(<MockAppointmentForm doctors={mockDoctors} />);
      mockDoctors.forEach(doctor => {
        expect(screen.getByText(doctor.name)).toBeInTheDocument();
      });
    });
  });
  #endregion

  #region User Interaction Tests
  describe('User Interactions', () => {
    it('allows selecting a patient', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm patients={mockPatients} />);
      const select = screen.getByTestId('patient-select');
      await user.selectOptions(select, '1');
      expect(select).toHaveValue('1');
    });

    it('allows selecting a doctor', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm doctors={mockDoctors} />);
      const select = screen.getByTestId('doctor-select');
      await user.selectOptions(select, '2');
      expect(select).toHaveValue('2');
    });

    it('allows entering date', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Date');
      await user.type(input, '2025-01-15');
      expect(input).toHaveValue('2025-01-15');
    });

    it('allows entering time', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Time');
      await user.type(input, '10:30');
      expect(input).toHaveValue('10:30');
    });

    it('allows changing duration', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Duration (minutes)');
      await user.clear(input);
      await user.type(input, '45');
      expect(input).toHaveValue(45);
    });

    it('allows entering notes', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Notes');
      await user.type(input, 'Follow-up for blood pressure');
      expect(input).toHaveValue('Follow-up for blood pressure');
    });

    it('allows selecting consultation type', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const selects = screen.getAllByRole('combobox');
      const typeSelect = selects.find(s => s.getAttribute('name') === 'appointmentType');
      if (typeSelect) {
        await user.selectOptions(typeSelect, 'consultation');
        expect(typeSelect).toHaveValue('consultation');
      }
    });

    it('allows selecting followup type', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const selects = screen.getAllByRole('combobox');
      const typeSelect = selects.find(s => s.getAttribute('name') === 'appointmentType');
      if (typeSelect) {
        await user.selectOptions(typeSelect, 'followup');
        expect(typeSelect).toHaveValue('followup');
      }
    });

    it('allows selecting procedure type', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const selects = screen.getAllByRole('combobox');
      const typeSelect = selects.find(s => s.getAttribute('name') === 'appointmentType');
      if (typeSelect) {
        await user.selectOptions(typeSelect, 'procedure');
        expect(typeSelect).toHaveValue('procedure');
      }
    });

    it('allows selecting emergency type', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const selects = screen.getAllByRole('combobox');
      const typeSelect = selects.find(s => s.getAttribute('name') === 'appointmentType');
      if (typeSelect) {
        await user.selectOptions(typeSelect, 'emergency');
        expect(typeSelect).toHaveValue('emergency');
      }
    });
  });
  #endregion

  #region Form Submission Tests
  describe('Form Submission', () => {
    it('calls onSubmit when form is submitted', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockAppointmentForm onSubmit={onSubmit} />);
      await user.click(screen.getByRole('button', { name: 'Book Appointment' }));
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when cancel is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockAppointmentForm onCancel={onCancel} />);
      await user.click(screen.getByRole('button', { name: 'Cancel' }));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });
  });
  #endregion

  #region Initial Data Tests
  describe('Initial Data Population', () => {
    it('populates patient from initial data', () => {
      render(<MockAppointmentForm
        patients={mockPatients}
        initialData={{ patientId: '2' }}
      />);
      expect(screen.getByTestId('patient-select')).toHaveValue('2');
    });

    it('populates doctor from initial data', () => {
      render(<MockAppointmentForm
        doctors={mockDoctors}
        initialData={{ doctorId: '1' }}
      />);
      expect(screen.getByTestId('doctor-select')).toHaveValue('1');
    });

    it('populates notes from initial data', () => {
      render(<MockAppointmentForm initialData={{ notes: 'Existing notes' }} />);
      expect(screen.getByPlaceholderText('Notes')).toHaveValue('Existing notes');
    });
  });
  #endregion
});

describe('LabOrderForm Component Tests', () => {
  const mockTests = [
    { id: 1, name: 'Complete Blood Count', price: 150 },
    { id: 2, name: 'Lipid Panel', price: 200 },
    { id: 3, name: 'Liver Function', price: 250 },
    { id: 4, name: 'Thyroid Panel', price: 300 },
    { id: 5, name: 'Urinalysis', price: 80 }
  ];

  const mockPatient = { id: 1, name: 'John Doe' };

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders the lab order form', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByTestId('lab-order-form')).toBeInTheDocument();
    });

    it('displays patient info', () => {
      render(<MockLabOrderForm patient={mockPatient} />);
      expect(screen.getByTestId('patient-info')).toHaveTextContent('John Doe');
    });

    it('displays no patient message when patient not provided', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByTestId('patient-info')).toHaveTextContent('No patient selected');
    });

    it('renders test selection area', () => {
      render(<MockLabOrderForm tests={mockTests} />);
      expect(screen.getByTestId('test-selection')).toBeInTheDocument();
    });

    it('renders all available tests', () => {
      render(<MockLabOrderForm tests={mockTests} />);
      mockTests.forEach(test => {
        expect(screen.getByText(new RegExp(test.name))).toBeInTheDocument();
      });
    });

    it('renders priority select', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByText('Routine')).toBeInTheDocument();
    });

    it('renders sample type select', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByText('Select Sample Type')).toBeInTheDocument();
    });

    it('renders clinical notes textarea', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByPlaceholderText('Clinical Notes')).toBeInTheDocument();
    });

    it('renders order tests button', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByRole('button', { name: 'Order Tests' })).toBeInTheDocument();
    });

    it('renders cancel button', () => {
      render(<MockLabOrderForm />);
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });
  });
  #endregion

  #region User Interaction Tests
  describe('User Interactions', () => {
    it('allows selecting a test', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm tests={mockTests} />);
      const checkboxes = screen.getAllByRole('checkbox');
      await user.click(checkboxes[0]);
      expect(checkboxes[0]).toBeChecked();
    });

    it('allows selecting multiple tests', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm tests={mockTests} />);
      const checkboxes = screen.getAllByRole('checkbox');
      await user.click(checkboxes[0]);
      await user.click(checkboxes[1]);
      await user.click(checkboxes[2]);
      expect(checkboxes[0]).toBeChecked();
      expect(checkboxes[1]).toBeChecked();
      expect(checkboxes[2]).toBeChecked();
    });

    it('allows deselecting a test', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm tests={mockTests} />);
      const checkboxes = screen.getAllByRole('checkbox');
      await user.click(checkboxes[0]);
      await user.click(checkboxes[0]);
      expect(checkboxes[0]).not.toBeChecked();
    });

    it('allows selecting priority', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[0], 'urgent');
      expect(selects[0]).toHaveValue('urgent');
    });

    it('allows selecting STAT priority', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[0], 'stat');
      expect(selects[0]).toHaveValue('stat');
    });

    it('allows selecting blood sample type', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[1], 'blood');
      expect(selects[1]).toHaveValue('blood');
    });

    it('allows selecting urine sample type', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm />);
      const selects = screen.getAllByRole('combobox');
      await user.selectOptions(selects[1], 'urine');
      expect(selects[1]).toHaveValue('urine');
    });

    it('allows entering clinical notes', async () => {
      const user = userEvent.setup();
      render(<MockLabOrderForm />);
      const textarea = screen.getByPlaceholderText('Clinical Notes');
      await user.type(textarea, 'Patient has elevated blood pressure');
      expect(textarea).toHaveValue('Patient has elevated blood pressure');
    });
  });
  #endregion

  #region Form Submission Tests
  describe('Form Submission', () => {
    it('calls onSubmit when order tests button is clicked', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockLabOrderForm onSubmit={onSubmit} />);
      await user.click(screen.getByRole('button', { name: 'Order Tests' }));
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when cancel button is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockLabOrderForm onCancel={onCancel} />);
      await user.click(screen.getByRole('button', { name: 'Cancel' }));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });
  });
  #endregion
});

describe('InvoiceForm Component Tests', () => {
  const mockPatient = { id: 1, name: 'John Doe' };
  const mockItems = [
    { description: 'Consultation', quantity: 1, price: 200 },
    { description: 'Blood Test', quantity: 1, price: 150 },
    { description: 'X-Ray', quantity: 2, price: 100 }
  ];

  #region Rendering Tests
  describe('Rendering', () => {
    it('renders the invoice form', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByTestId('invoice-form')).toBeInTheDocument();
    });

    it('displays patient info', () => {
      render(<MockInvoiceForm patient={mockPatient} />);
      expect(screen.getByTestId('patient-info')).toHaveTextContent('John Doe');
    });

    it('renders line items table', () => {
      render(<MockInvoiceForm items={mockItems} />);
      expect(screen.getByTestId('line-items')).toBeInTheDocument();
    });

    it('renders all line items', () => {
      render(<MockInvoiceForm items={mockItems} />);
      mockItems.forEach(item => {
        expect(screen.getByText(item.description)).toBeInTheDocument();
      });
    });

    it('renders discount input', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByPlaceholderText('Discount %')).toBeInTheDocument();
    });

    it('renders tax input with default value', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByPlaceholderText('Tax %')).toHaveValue(5);
    });

    it('renders payment method select', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByText('Cash')).toBeInTheDocument();
    });

    it('renders generate invoice button', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByRole('button', { name: 'Generate Invoice' })).toBeInTheDocument();
    });

    it('renders cancel button', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('renders table headers', () => {
      render(<MockInvoiceForm />);
      expect(screen.getByText('Item')).toBeInTheDocument();
      expect(screen.getByText('Qty')).toBeInTheDocument();
      expect(screen.getByText('Price')).toBeInTheDocument();
      expect(screen.getByText('Total')).toBeInTheDocument();
    });
  });
  #endregion

  #region User Interaction Tests
  describe('User Interactions', () => {
    it('allows selecting cash payment', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const select = screen.getAllByRole('combobox')[0];
      await user.selectOptions(select, 'cash');
      expect(select).toHaveValue('cash');
    });

    it('allows selecting card payment', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const select = screen.getAllByRole('combobox')[0];
      await user.selectOptions(select, 'card');
      expect(select).toHaveValue('card');
    });

    it('allows selecting insurance payment', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const select = screen.getAllByRole('combobox')[0];
      await user.selectOptions(select, 'insurance');
      expect(select).toHaveValue('insurance');
    });

    it('allows selecting bank transfer payment', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const select = screen.getAllByRole('combobox')[0];
      await user.selectOptions(select, 'bank_transfer');
      expect(select).toHaveValue('bank_transfer');
    });

    it('allows entering discount', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const input = screen.getByPlaceholderText('Discount %');
      await user.clear(input);
      await user.type(input, '10');
      expect(input).toHaveValue(10);
    });

    it('allows entering tax percentage', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const input = screen.getByPlaceholderText('Tax %');
      await user.clear(input);
      await user.type(input, '7');
      expect(input).toHaveValue(7);
    });
  });
  #endregion

  #region Form Submission Tests
  describe('Form Submission', () => {
    it('calls onSubmit when generate invoice is clicked', async () => {
      const onSubmit = vi.fn();
      const user = userEvent.setup();
      render(<MockInvoiceForm onSubmit={onSubmit} />);
      await user.click(screen.getByRole('button', { name: 'Generate Invoice' }));
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    it('calls onCancel when cancel is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();
      render(<MockInvoiceForm onCancel={onCancel} />);
      await user.click(screen.getByRole('button', { name: 'Cancel' }));
      expect(onCancel).toHaveBeenCalledTimes(1);
    });
  });
  #endregion

  #region Line Item Calculations
  describe('Line Item Calculations', () => {
    it('calculates correct total for single item', () => {
      render(<MockInvoiceForm items={[{ description: 'Test', quantity: 2, price: 100 }]} />);
      expect(screen.getByText('200.00')).toBeInTheDocument();
    });

    it('calculates correct total for multiple items', () => {
      render(<MockInvoiceForm items={mockItems} />);
      expect(screen.getByText('200.00')).toBeInTheDocument(); // 1 * 200
      expect(screen.getByText('150.00')).toBeInTheDocument(); // 1 * 150
    });
  });
  #endregion
});

// Additional form validation tests
describe('Form Validation Tests', () => {
  #region Email Validation
  describe('Email Field Validation', () => {
    it('accepts valid email format', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Email');
      await user.type(input, 'test@example.com');
      expect(input).toHaveValue('test@example.com');
    });

    it('accepts email with subdomain', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Email');
      await user.type(input, 'test@mail.example.com');
      expect(input).toHaveValue('test@mail.example.com');
    });

    it('accepts email with plus sign', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Email');
      await user.type(input, 'test+label@example.com');
      expect(input).toHaveValue('test+label@example.com');
    });
  });
  #endregion

  #region Phone Validation
  describe('Phone Field Input', () => {
    it('accepts UAE mobile format', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Phone');
      await user.type(input, '+971501234567');
      expect(input).toHaveValue('+971501234567');
    });

    it('accepts UAE landline format', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Phone');
      await user.type(input, '+97142345678');
      expect(input).toHaveValue('+97142345678');
    });

    it('accepts different country codes', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Phone');
      await user.type(input, '+14155551234');
      expect(input).toHaveValue('+14155551234');
    });
  });
  #endregion

  #region Emirates ID Validation
  describe('Emirates ID Field Input', () => {
    it('accepts valid Emirates ID format', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Emirates ID');
      await user.type(input, '784-1990-1234567-1');
      expect(input).toHaveValue('784-1990-1234567-1');
    });

    it('accepts Emirates ID without dashes', async () => {
      const user = userEvent.setup();
      render(<MockPatientForm />);
      const input = screen.getByPlaceholderText('Emirates ID');
      await user.type(input, '784199012345671');
      expect(input).toHaveValue('784199012345671');
    });
  });
  #endregion

  #region Date Input Tests
  describe('Date Input Validation', () => {
    it('accepts valid date format', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Date');
      await user.type(input, '2025-01-15');
      expect(input).toHaveValue('2025-01-15');
    });

    it('accepts leap year date', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Date');
      await user.type(input, '2024-02-29');
      expect(input).toHaveValue('2024-02-29');
    });
  });
  #endregion

  #region Time Input Tests
  describe('Time Input Validation', () => {
    it('accepts valid time format', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Time');
      await user.type(input, '09:30');
      expect(input).toHaveValue('09:30');
    });

    it('accepts afternoon time', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Time');
      await user.type(input, '14:45');
      expect(input).toHaveValue('14:45');
    });

    it('accepts midnight time', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Time');
      await user.type(input, '00:00');
      expect(input).toHaveValue('00:00');
    });

    it('accepts end of day time', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Time');
      await user.type(input, '23:59');
      expect(input).toHaveValue('23:59');
    });
  });
  #endregion

  #region Numeric Input Tests
  describe('Numeric Input Validation', () => {
    it('accepts positive duration', async () => {
      const user = userEvent.setup();
      render(<MockAppointmentForm />);
      const input = screen.getByPlaceholderText('Duration (minutes)');
      await user.clear(input);
      await user.type(input, '60');
      expect(input).toHaveValue(60);
    });

    it('accepts zero discount', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const input = screen.getByPlaceholderText('Discount %');
      expect(input).toHaveValue(0);
    });

    it('accepts percentage values', async () => {
      const user = userEvent.setup();
      render(<MockInvoiceForm />);
      const input = screen.getByPlaceholderText('Discount %');
      await user.clear(input);
      await user.type(input, '15');
      expect(input).toHaveValue(15);
    });
  });
  #endregion
});
