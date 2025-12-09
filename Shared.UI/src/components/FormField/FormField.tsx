import React from 'react';

interface FormFieldProps {
  label: string;
  name: string;
  error?: string;
  required?: boolean;
  helpText?: string;
  children: React.ReactNode;
  className?: string;
}

export const FormField: React.FC<FormFieldProps> = ({
  label,
  name,
  error,
  required = false,
  helpText,
  children,
  className = '',
}) => {
  return (
    <div className={`space-y-1 ${className}`}>
      <label htmlFor={name} className="block text-sm font-medium text-gray-700">
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </label>
      {children}
      {helpText && !error && (
        <p className="text-sm text-gray-500">{helpText}</p>
      )}
      {error && (
        <p className="text-sm text-red-600" role="alert">
          {error}
        </p>
      )}
    </div>
  );
};

// Input component
interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: boolean;
}

export const Input: React.FC<InputProps> = ({
  error,
  className = '',
  ...props
}) => {
  return (
    <input
      className={`
        block w-full px-3 py-2 border rounded-md shadow-sm placeholder-gray-400
        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
        disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed
        sm:text-sm transition-colors
        ${error ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300'}
        ${className}
      `}
      {...props}
    />
  );
};

// Textarea component
interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: boolean;
}

export const Textarea: React.FC<TextareaProps> = ({
  error,
  className = '',
  ...props
}) => {
  return (
    <textarea
      className={`
        block w-full px-3 py-2 border rounded-md shadow-sm placeholder-gray-400
        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
        disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed
        sm:text-sm transition-colors min-h-[100px] resize-y
        ${error ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300'}
        ${className}
      `}
      {...props}
    />
  );
};

// Select component
interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  error?: boolean;
  options: Array<{ value: string; label: string }>;
  placeholder?: string;
}

export const Select: React.FC<SelectProps> = ({
  error,
  options,
  placeholder,
  className = '',
  ...props
}) => {
  return (
    <select
      className={`
        block w-full px-3 py-2 border rounded-md shadow-sm
        focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500
        disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed
        sm:text-sm transition-colors appearance-none bg-white
        ${error ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : 'border-gray-300'}
        ${className}
      `}
      {...props}
    >
      {placeholder && (
        <option value="" disabled>
          {placeholder}
        </option>
      )}
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  );
};

// Checkbox component
interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string;
  description?: string;
}

export const Checkbox: React.FC<CheckboxProps> = ({
  label,
  description,
  className = '',
  ...props
}) => {
  return (
    <div className={`flex items-start ${className}`}>
      <div className="flex items-center h-5">
        <input
          type="checkbox"
          className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500 disabled:opacity-50"
          {...props}
        />
      </div>
      <div className="ml-3 text-sm">
        <label htmlFor={props.id} className="font-medium text-gray-700">
          {label}
        </label>
        {description && (
          <p className="text-gray-500">{description}</p>
        )}
      </div>
    </div>
  );
};

// Radio group component
interface RadioOption {
  value: string;
  label: string;
  description?: string;
}

interface RadioGroupProps {
  name: string;
  options: RadioOption[];
  value?: string;
  onChange?: (value: string) => void;
  className?: string;
}

export const RadioGroup: React.FC<RadioGroupProps> = ({
  name,
  options,
  value,
  onChange,
  className = '',
}) => {
  return (
    <div className={`space-y-2 ${className}`}>
      {options.map((option) => (
        <div key={option.value} className="flex items-start">
          <div className="flex items-center h-5">
            <input
              type="radio"
              id={`${name}-${option.value}`}
              name={name}
              value={option.value}
              checked={value === option.value}
              onChange={(e) => onChange?.(e.target.value)}
              className="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
            />
          </div>
          <div className="ml-3 text-sm">
            <label
              htmlFor={`${name}-${option.value}`}
              className="font-medium text-gray-700"
            >
              {option.label}
            </label>
            {option.description && (
              <p className="text-gray-500">{option.description}</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};
