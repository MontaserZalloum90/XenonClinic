import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import {
  FormField,
  Input,
  Textarea,
  Select,
  Checkbox,
  RadioGroup,
} from '../components/FormField/FormField';

const meta: Meta<typeof FormField> = {
  title: 'Components/FormField',
  component: FormField,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof FormField>;

export const Default: Story = {
  render: () => (
    <FormField label="Email" name="email">
      <Input type="email" id="email" name="email" placeholder="Enter your email" />
    </FormField>
  ),
};

export const Required: Story = {
  render: () => (
    <FormField label="Full Name" name="name" required>
      <Input type="text" id="name" name="name" placeholder="Enter your full name" />
    </FormField>
  ),
};

export const WithHelpText: Story = {
  render: () => (
    <FormField
      label="Password"
      name="password"
      required
      helpText="Must be at least 8 characters long"
    >
      <Input type="password" id="password" name="password" placeholder="Enter password" />
    </FormField>
  ),
};

export const WithError: Story = {
  render: () => (
    <FormField
      label="Email"
      name="email"
      required
      error="Please enter a valid email address"
    >
      <Input type="email" id="email" name="email" error placeholder="Enter your email" />
    </FormField>
  ),
};

export const TextInput: Story = {
  name: 'Input - Text',
  render: () => (
    <div className="space-y-4 max-w-md">
      <Input type="text" placeholder="Default input" />
      <Input type="text" placeholder="Disabled input" disabled />
      <Input type="text" placeholder="With error" error />
    </div>
  ),
};

export const TextareaExample: Story = {
  name: 'Textarea',
  render: () => (
    <div className="space-y-4 max-w-md">
      <FormField label="Description" name="description">
        <Textarea
          id="description"
          name="description"
          placeholder="Enter a description..."
          rows={4}
        />
      </FormField>
      <FormField label="Notes" name="notes" error="This field is required">
        <Textarea id="notes" name="notes" placeholder="Add notes..." error />
      </FormField>
    </div>
  ),
};

export const SelectExample: Story = {
  name: 'Select',
  render: () => {
    const [value, setValue] = useState('');
    const options = [
      { value: 'us', label: 'United States' },
      { value: 'ca', label: 'Canada' },
      { value: 'uk', label: 'United Kingdom' },
      { value: 'au', label: 'Australia' },
    ];

    return (
      <div className="space-y-4 max-w-md">
        <FormField label="Country" name="country" required>
          <Select
            id="country"
            name="country"
            options={options}
            placeholder="Select a country"
            value={value}
            onChange={(e) => setValue(e.target.value)}
          />
        </FormField>
        <FormField label="Country" name="country-error" error="Please select a country">
          <Select
            id="country-error"
            name="country-error"
            options={options}
            placeholder="Select a country"
            error
          />
        </FormField>
      </div>
    );
  },
};

export const CheckboxExample: Story = {
  name: 'Checkbox',
  render: () => {
    const [checked1, setChecked1] = useState(false);
    const [checked2, setChecked2] = useState(true);

    return (
      <div className="space-y-4">
        <Checkbox
          id="terms"
          label="I agree to the terms and conditions"
          checked={checked1}
          onChange={(e) => setChecked1(e.target.checked)}
        />
        <Checkbox
          id="newsletter"
          label="Subscribe to newsletter"
          description="Receive updates about new features and promotions"
          checked={checked2}
          onChange={(e) => setChecked2(e.target.checked)}
        />
        <Checkbox
          id="disabled"
          label="Disabled checkbox"
          disabled
        />
      </div>
    );
  },
};

export const RadioGroupExample: Story = {
  name: 'RadioGroup',
  render: () => {
    const [value, setValue] = useState('email');
    const options = [
      { value: 'email', label: 'Email', description: 'Receive notifications via email' },
      { value: 'sms', label: 'SMS', description: 'Receive notifications via text message' },
      { value: 'push', label: 'Push', description: 'Receive push notifications on your device' },
    ];

    return (
      <div className="space-y-4">
        <FormField label="Notification Preferences" name="notifications">
          <RadioGroup
            name="notifications"
            options={options}
            value={value}
            onChange={setValue}
          />
        </FormField>
      </div>
    );
  },
};

export const CompleteForm: Story = {
  name: 'Complete Form Example',
  render: () => {
    const [formData, setFormData] = useState({
      firstName: '',
      lastName: '',
      email: '',
      role: '',
      bio: '',
      notifications: 'email',
      terms: false,
    });

    const handleChange = (field: string, value: string | boolean) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
    };

    return (
      <form className="space-y-6 max-w-lg">
        <div className="grid grid-cols-2 gap-4">
          <FormField label="First Name" name="firstName" required>
            <Input
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={(e) => handleChange('firstName', e.target.value)}
              placeholder="John"
            />
          </FormField>
          <FormField label="Last Name" name="lastName" required>
            <Input
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={(e) => handleChange('lastName', e.target.value)}
              placeholder="Doe"
            />
          </FormField>
        </div>

        <FormField
          label="Email"
          name="email"
          required
          helpText="We'll never share your email"
        >
          <Input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={(e) => handleChange('email', e.target.value)}
            placeholder="john@example.com"
          />
        </FormField>

        <FormField label="Role" name="role" required>
          <Select
            id="role"
            name="role"
            options={[
              { value: 'doctor', label: 'Doctor' },
              { value: 'nurse', label: 'Nurse' },
              { value: 'admin', label: 'Administrator' },
              { value: 'staff', label: 'Staff' },
            ]}
            placeholder="Select a role"
            value={formData.role}
            onChange={(e) => handleChange('role', e.target.value)}
          />
        </FormField>

        <FormField label="Bio" name="bio">
          <Textarea
            id="bio"
            name="bio"
            value={formData.bio}
            onChange={(e) => handleChange('bio', e.target.value)}
            placeholder="Tell us about yourself..."
            rows={3}
          />
        </FormField>

        <FormField label="Notification Preferences" name="notifications">
          <RadioGroup
            name="notifications"
            options={[
              { value: 'email', label: 'Email' },
              { value: 'sms', label: 'SMS' },
              { value: 'none', label: 'None' },
            ]}
            value={formData.notifications}
            onChange={(v) => handleChange('notifications', v)}
          />
        </FormField>

        <Checkbox
          id="terms"
          label="I agree to the terms and conditions"
          checked={formData.terms}
          onChange={(e) => handleChange('terms', e.target.checked)}
        />

        <button
          type="submit"
          className="w-full px-4 py-2 text-white bg-primary-600 rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
          onClick={(e) => {
            e.preventDefault();
            alert(JSON.stringify(formData, null, 2));
          }}
        >
          Submit
        </button>
      </form>
    );
  },
};
