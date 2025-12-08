import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { hrApi } from '../lib/api';
import type { Employee, EmployeeFormData, EmployeeStatus, EmployeeRole } from '../types/hr';
import { format } from 'date-fns';

interface HRFormProps {
  employee?: Employee;
  onSuccess: () => void;
  onCancel: () => void;
}

export const HRForm = ({ employee, onSuccess, onCancel }: HRFormProps) => {
  const queryClient = useQueryClient();
  const isEditing = !!employee;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmployeeFormData>({
    defaultValues: employee
      ? {
          employeeNumber: employee.employeeNumber,
          fullName: employee.fullName,
          email: employee.email,
          phoneNumber: employee.phoneNumber || '',
          role: employee.role,
          department: employee.department || '',
          status: employee.status,
          hireDate: format(new Date(employee.hireDate), 'yyyy-MM-dd'),
          salary: employee.salary,
          emergencyContact: employee.emergencyContact || '',
          emergencyPhone: employee.emergencyPhone || '',
          address: employee.address || '',
          nationalId: employee.nationalId || '',
          dateOfBirth: employee.dateOfBirth ? format(new Date(employee.dateOfBirth), 'yyyy-MM-dd') : '',
        }
      : {
          status: 0 as EmployeeStatus,
          role: 0 as EmployeeRole,
        },
  });

  const createMutation = useMutation({
    mutationFn: (data: EmployeeFormData) => hrApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hr-employees'] });
      queryClient.invalidateQueries({ queryKey: ['hr-stats'] });
      onSuccess();
    },
  });

  const updateMutation = useMutation({
    mutationFn: (data: EmployeeFormData) => hrApi.update(employee!.id, { ...data, id: employee!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hr-employees'] });
      queryClient.invalidateQueries({ queryKey: ['hr-stats'] });
      onSuccess();
    },
  });

  const onSubmit = (data: EmployeeFormData) => {
    if (isEditing) {
      updateMutation.mutate(data);
    } else {
      createMutation.mutate(data);
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;
  const error = createMutation.error || updateMutation.error;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {error && (
        <div className="p-3 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-600">
            Error: {error instanceof Error ? error.message : 'An error occurred'}
          </p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Employee Number */}
        <div>
          <label htmlFor="employeeNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Employee Number *
          </label>
          <input
            type="text"
            id="employeeNumber"
            {...register('employeeNumber', { required: 'Employee number is required' })}
            className="input"
            placeholder="EMP-001"
          />
          {errors.employeeNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.employeeNumber.message}</p>
          )}
        </div>

        {/* Full Name */}
        <div>
          <label htmlFor="fullName" className="block text-sm font-medium text-gray-700 mb-1">
            Full Name *
          </label>
          <input
            type="text"
            id="fullName"
            {...register('fullName', { required: 'Full name is required' })}
            className="input"
            placeholder="John Doe"
          />
          {errors.fullName && (
            <p className="mt-1 text-sm text-red-600">{errors.fullName.message}</p>
          )}
        </div>

        {/* Email */}
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
            Email *
          </label>
          <input
            type="email"
            id="email"
            {...register('email', { required: 'Email is required' })}
            className="input"
            placeholder="john@xenonclinic.com"
          />
          {errors.email && (
            <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
          )}
        </div>

        {/* Phone Number */}
        <div>
          <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700 mb-1">
            Phone Number
          </label>
          <input
            type="tel"
            id="phoneNumber"
            {...register('phoneNumber')}
            className="input"
            placeholder="+971 50 123 4567"
          />
        </div>

        {/* Role */}
        <div>
          <label htmlFor="role" className="block text-sm font-medium text-gray-700 mb-1">
            Role *
          </label>
          <select id="role" {...register('role', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Doctor</option>
            <option value={1}>Nurse</option>
            <option value={2}>Receptionist</option>
            <option value={3}>Technician</option>
            <option value={4}>Administrator</option>
            <option value={5}>Other</option>
          </select>
        </div>

        {/* Department */}
        <div>
          <label htmlFor="department" className="block text-sm font-medium text-gray-700 mb-1">
            Department
          </label>
          <input
            type="text"
            id="department"
            {...register('department')}
            className="input"
            placeholder="Audiology"
          />
        </div>

        {/* Status */}
        <div>
          <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-1">
            Status *
          </label>
          <select id="status" {...register('status', { required: true, valueAsNumber: true })} className="input">
            <option value={0}>Active</option>
            <option value={1}>On Leave</option>
            <option value={2}>Suspended</option>
            <option value={3}>Terminated</option>
          </select>
        </div>

        {/* Hire Date */}
        <div>
          <label htmlFor="hireDate" className="block text-sm font-medium text-gray-700 mb-1">
            Hire Date *
          </label>
          <input
            type="date"
            id="hireDate"
            {...register('hireDate', { required: 'Hire date is required' })}
            className="input"
          />
          {errors.hireDate && (
            <p className="mt-1 text-sm text-red-600">{errors.hireDate.message}</p>
          )}
        </div>

        {/* National ID */}
        <div>
          <label htmlFor="nationalId" className="block text-sm font-medium text-gray-700 mb-1">
            National ID
          </label>
          <input
            type="text"
            id="nationalId"
            {...register('nationalId')}
            className="input"
            placeholder="784-XXXX-XXXXXXX-X"
          />
        </div>

        {/* Date of Birth */}
        <div>
          <label htmlFor="dateOfBirth" className="block text-sm font-medium text-gray-700 mb-1">
            Date of Birth
          </label>
          <input
            type="date"
            id="dateOfBirth"
            {...register('dateOfBirth')}
            className="input"
          />
        </div>

        {/* Emergency Contact */}
        <div>
          <label htmlFor="emergencyContact" className="block text-sm font-medium text-gray-700 mb-1">
            Emergency Contact
          </label>
          <input
            type="text"
            id="emergencyContact"
            {...register('emergencyContact')}
            className="input"
            placeholder="Contact Name"
          />
        </div>

        {/* Emergency Phone */}
        <div>
          <label htmlFor="emergencyPhone" className="block text-sm font-medium text-gray-700 mb-1">
            Emergency Phone
          </label>
          <input
            type="tel"
            id="emergencyPhone"
            {...register('emergencyPhone')}
            className="input"
            placeholder="+971 50 XXX XXXX"
          />
        </div>
      </div>

      {/* Address */}
      <div>
        <label htmlFor="address" className="block text-sm font-medium text-gray-700 mb-1">
          Address
        </label>
        <textarea
          id="address"
          {...register('address')}
          rows={2}
          className="input"
          placeholder="Full address..."
        />
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className="btn btn-secondary"
        >
          Cancel
        </button>
        <button type="submit" disabled={isPending} className="btn btn-primary">
          {isPending ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
              {isEditing ? 'Updating...' : 'Creating...'}
            </div>
          ) : isEditing ? (
            'Update Employee'
          ) : (
            'Add Employee'
          )}
        </button>
      </div>
    </form>
  );
};
