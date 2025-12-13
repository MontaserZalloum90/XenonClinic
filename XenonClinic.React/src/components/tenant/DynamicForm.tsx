import React, { useState, useCallback, useMemo, useEffect } from "react";
import { useT, useSchema, useFormLayout } from "../../contexts/TenantContext";
import type {
  UISchema,
  FormLayout,
  FormSection,
  FieldDefinition,
  ConditionalRule,
} from "../../types/tenant";

/**
 * DynamicForm - Renders a form based on schema and layout from tenant context
 *
 * Usage:
 * <DynamicForm
 *   entityName="patient"
 *   mode="create"
 *   onSubmit={(data) => savePatient(data)}
 * />
 *
 * With initial data:
 * <DynamicForm
 *   entityName="patient"
 *   mode="edit"
 *   initialData={patient}
 *   onSubmit={(data) => updatePatient(data)}
 * />
 */

interface DynamicFormProps {
  /** Entity name to look up schema/layout */
  entityName: string;
  /** Form mode */
  mode: "create" | "edit" | "view";
  /** Initial form data */
  initialData?: Record<string, unknown>;
  /** Called on form submission */
  onSubmit?: (data: Record<string, unknown>) => void | Promise<void>;
  /** Called on cancel */
  onCancel?: () => void;
  /** Called on delete (edit mode) */
  onDelete?: () => void;
  /** Custom schema override */
  schema?: UISchema;
  /** Custom layout override */
  layout?: FormLayout;
  /** Loading state */
  isLoading?: boolean;
  /** Additional className */
  className?: string;
}

export const DynamicForm: React.FC<DynamicFormProps> = ({
  entityName,
  mode,
  initialData = {},
  onSubmit,
  onCancel,
  onDelete,
  schema: schemaProp,
  layout: layoutProp,
  isLoading = false,
  className = "",
}) => {
  const t = useT();
  const contextSchema = useSchema(entityName);
  const contextLayout = useFormLayout(entityName);

  const schema = schemaProp ?? contextSchema;
  const layout = layoutProp ?? contextLayout;

  const [formData, setFormData] =
    useState<Record<string, unknown>>(initialData);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [collapsedSections, setCollapsedSections] = useState<Set<string>>(
    new Set(),
  );

  // Initialize collapsed sections
  useEffect(() => {
    if (layout?.sections) {
      const collapsed = new Set<string>();
      layout.sections.forEach((section) => {
        if (section.defaultCollapsed) {
          collapsed.add(section.id);
        }
      });
      setCollapsedSections(collapsed);
    }
  }, [layout]);

  // Update form data when initial data changes
  useEffect(() => {
    setFormData(initialData);
  }, [initialData]);

  // Field change handler
  const handleFieldChange = useCallback(
    (fieldName: string, value: unknown) => {
      setFormData((prev) => ({ ...prev, [fieldName]: value }));
      // Clear error when field changes
      if (errors[fieldName]) {
        setErrors((prev) => {
          const next = { ...prev };
          delete next[fieldName];
          return next;
        });
      }
    },
    [errors],
  );

  // Validate form
  const validate = useCallback((): boolean => {
    if (!schema) return true;

    const newErrors: Record<string, string> = {};

    for (const field of schema.fields) {
      const value = formData[field.name];
      const isVisible = evaluateVisibility(field.visible, formData);

      if (!isVisible) continue;

      // Required validation
      if (field.validation?.required) {
        if (value === undefined || value === null || value === "") {
          const label = t(field.label ?? field.name, field.name);
          newErrors[field.name] = `${label} is required`;
          continue;
        }
      }

      // String validations
      if (typeof value === "string") {
        if (
          field.validation?.minLength &&
          value.length < field.validation.minLength
        ) {
          newErrors[field.name] =
            `Minimum ${field.validation.minLength} characters`;
        }
        if (
          field.validation?.maxLength &&
          value.length > field.validation.maxLength
        ) {
          newErrors[field.name] =
            `Maximum ${field.validation.maxLength} characters`;
        }
        if (field.validation?.pattern) {
          const regex = new RegExp(field.validation.pattern);
          if (!regex.test(value)) {
            newErrors[field.name] =
              field.validation.patternMessage ?? "Invalid format";
          }
        }
      }

      // Number validations
      if (typeof value === "number") {
        if (
          field.validation?.min !== undefined &&
          value < field.validation.min
        ) {
          newErrors[field.name] = `Minimum value is ${field.validation.min}`;
        }
        if (
          field.validation?.max !== undefined &&
          value > field.validation.max
        ) {
          newErrors[field.name] = `Maximum value is ${field.validation.max}`;
        }
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [schema, formData, t]);

  // Submit handler
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (mode === "view") return;
    if (!validate()) return;

    setIsSubmitting(true);
    try {
      await onSubmit?.(formData);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Toggle section collapse
  const toggleSection = (sectionId: string) => {
    setCollapsedSections((prev) => {
      const next = new Set(prev);
      if (next.has(sectionId)) {
        next.delete(sectionId);
      } else {
        next.add(sectionId);
      }
      return next;
    });
  };

  // Build field map for quick lookup
  const fieldMap = useMemo(() => {
    if (!schema) return new Map<string, FieldDefinition>();
    return new Map(schema.fields.map((f) => [f.name, f]));
  }, [schema]);

  if (!schema) {
    return (
      <div className="p-4 text-center text-gray-500">
        Schema not found for entity: {entityName}
      </div>
    );
  }

  // If no layout, render all fields in a single section
  const sections: FormSection[] = layout?.sections ?? [
    {
      id: "default",
      title: t(schema.displayName),
      columns: 2,
      fields: schema.fields.map((f) => f.name),
    },
  ];

  const isViewMode = mode === "view";
  const submitLabel =
    layout?.submitLabel ??
    (mode === "create" ? "action.create" : "action.save");
  const cancelLabel = layout?.cancelLabel ?? "action.cancel";

  return (
    <form onSubmit={handleSubmit} className={`space-y-6 ${className}`}>
      {sections.map((section) => {
        const isSectionVisible = evaluateVisibility(section.visible, formData);
        if (!isSectionVisible) return null;

        const isCollapsed =
          section.collapsible && collapsedSections.has(section.id);

        return (
          <div
            key={section.id}
            className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border"
          >
            {/* Section Header */}
            <div
              className={`px-4 py-3 border-b flex items-center justify-between ${
                section.collapsible
                  ? "cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700"
                  : ""
              }`}
              onClick={() => section.collapsible && toggleSection(section.id)}
            >
              <div>
                <h3 className="text-lg font-medium text-gray-900 dark:text-white">
                  {t(section.title, section.title)}
                </h3>
                {section.description && (
                  <p className="text-sm text-gray-500">
                    {t(section.description)}
                  </p>
                )}
              </div>
              {section.collapsible && (
                <svg
                  className={`h-5 w-5 transform transition-transform ${isCollapsed ? "" : "rotate-180"}`}
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              )}
            </div>

            {/* Section Fields */}
            {!isCollapsed && (
              <div className="p-4">
                <div
                  className={`grid gap-4 ${getColumnsClass(section.columns ?? 2)}`}
                >
                  {section.fields.map((fieldName) => {
                    const field = fieldMap.get(fieldName);
                    if (!field) return null;

                    const isVisible = evaluateVisibility(
                      field.visible,
                      formData,
                    );
                    if (!isVisible) return null;

                    const isDisabled =
                      isViewMode ||
                      evaluateVisibility(field.disabled, formData);

                    return (
                      <div
                        key={fieldName}
                        className={getFieldWidthClass(field.width)}
                      >
                        <FormField
                          field={field}
                          value={formData[fieldName]}
                          error={errors[fieldName]}
                          disabled={isDisabled}
                          readOnly={field.readOnly || isViewMode}
                          onChange={(value) =>
                            handleFieldChange(fieldName, value)
                          }
                          t={t}
                        />
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        );
      })}

      {/* Form Actions */}
      {!isViewMode && (
        <div className="flex items-center justify-end space-x-3 pt-4">
          {onDelete && mode === "edit" && layout?.showDelete && (
            <button
              type="button"
              onClick={onDelete}
              className="px-4 py-2 text-sm font-medium text-red-600 hover:text-red-700 hover:bg-red-50 rounded-md"
            >
              {t("action.delete")}
            </button>
          )}
          <div className="flex-1" />
          {onCancel && (
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
            >
              {t(cancelLabel)}
            </button>
          )}
          <button
            type="submit"
            disabled={isSubmitting || isLoading}
            className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700 disabled:opacity-50"
          >
            {isSubmitting ? t("common.loading") : t(submitLabel)}
          </button>
        </div>
      )}
    </form>
  );
};

// ============================================
// Form Field Component
// ============================================

interface FormFieldProps {
  field: FieldDefinition;
  value: unknown;
  error?: string;
  disabled?: boolean;
  readOnly?: boolean;
  onChange: (value: unknown) => void;
  t: (key: string, fallback?: string) => string;
}

const FormField: React.FC<FormFieldProps> = ({
  field,
  value,
  error,
  disabled,
  readOnly,
  onChange,
  t,
}) => {
  const label = t(field.label ?? field.name, field.name);
  const placeholder = field.placeholder ? t(field.placeholder) : undefined;
  const isRequired = field.validation?.required;

  const baseInputClass = `
    block w-full rounded-md shadow-sm
    ${
      error
        ? "border-red-300 focus:border-red-500 focus:ring-red-500"
        : "border-gray-300 focus:border-primary-500 focus:ring-primary-500"
    }
    ${disabled ? "bg-gray-100 cursor-not-allowed" : ""}
    ${readOnly ? "bg-gray-50" : ""}
    sm:text-sm
  `;

  const renderInput = () => {
    switch (field.type) {
      case "text":
      case "email":
      case "url":
      case "phone":
      case "emiratesId":
      case "passport":
        return (
          <input
            type={
              field.type === "email"
                ? "email"
                : field.type === "url"
                  ? "url"
                  : "text"
            }
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            readOnly={readOnly}
            className={baseInputClass}
          />
        );

      case "number":
      case "currency":
      case "percentage":
        return (
          <div className="relative">
            {field.type === "currency" && (
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">
                {field.currency ?? "AED"}
              </span>
            )}
            <input
              type="number"
              value={(value as number) ?? ""}
              onChange={(e) =>
                onChange(e.target.value ? Number(e.target.value) : undefined)
              }
              placeholder={placeholder}
              disabled={disabled}
              readOnly={readOnly}
              step={field.decimals ? Math.pow(10, -field.decimals) : 1}
              className={`${baseInputClass} ${field.type === "currency" ? "pl-12" : ""} ${field.type === "percentage" ? "pr-8" : ""}`}
            />
            {field.type === "percentage" && (
              <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500">
                %
              </span>
            )}
          </div>
        );

      case "textarea":
        return (
          <textarea
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            readOnly={readOnly}
            rows={4}
            className={baseInputClass}
          />
        );

      case "date":
        return (
          <input
            type="date"
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            disabled={disabled}
            readOnly={readOnly}
            className={baseInputClass}
          />
        );

      case "datetime":
        return (
          <input
            type="datetime-local"
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            disabled={disabled}
            readOnly={readOnly}
            className={baseInputClass}
          />
        );

      case "time":
        return (
          <input
            type="time"
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            disabled={disabled}
            readOnly={readOnly}
            className={baseInputClass}
          />
        );

      case "select":
        return (
          <select
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            disabled={disabled}
            className={baseInputClass}
          >
            <option value="">{placeholder ?? `Select ${label}`}</option>
            {field.options?.map((opt) => (
              <option key={String(opt.value)} value={String(opt.value)}>
                {opt.label}
              </option>
            ))}
          </select>
        );

      case "multiselect": {
        const selectedValues = (value as unknown[]) ?? [];
        return (
          <div className="space-y-2">
            {field.options?.map((opt) => (
              <label key={String(opt.value)} className="flex items-center">
                <input
                  type="checkbox"
                  checked={selectedValues.includes(opt.value)}
                  onChange={(e) => {
                    if (e.target.checked) {
                      onChange([...selectedValues, opt.value]);
                    } else {
                      onChange(selectedValues.filter((v) => v !== opt.value));
                    }
                  }}
                  disabled={disabled}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">{opt.label}</span>
              </label>
            ))}
          </div>
        );
      }

      case "checkbox":
        return (
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={Boolean(value)}
              onChange={(e) => onChange(e.target.checked)}
              disabled={disabled}
              className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
            />
            <span className="ml-2 text-sm text-gray-700">{label}</span>
          </label>
        );

      case "toggle":
        return (
          <button
            type="button"
            onClick={() => !disabled && onChange(!value)}
            className={`
              relative inline-flex h-6 w-11 items-center rounded-full
              ${value ? "bg-primary-600" : "bg-gray-200"}
              ${disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}
            `}
          >
            <span
              className={`
                inline-block h-4 w-4 transform rounded-full bg-white transition
                ${value ? "translate-x-6" : "translate-x-1"}
              `}
            />
          </button>
        );

      default:
        return (
          <input
            type="text"
            value={(value as string) ?? ""}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            readOnly={readOnly}
            className={baseInputClass}
          />
        );
    }
  };

  // Don't show label for checkbox/toggle (they have inline labels)
  if (field.type === "checkbox") {
    return (
      <div>
        {renderInput()}
        {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
      </div>
    );
  }

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
        {label}
        {isRequired && <span className="text-red-500 ml-1">*</span>}
      </label>
      {renderInput()}
      {field.helpText && !error && (
        <p className="mt-1 text-sm text-gray-500">{t(field.helpText)}</p>
      )}
      {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
    </div>
  );
};

// ============================================
// Helper Functions
// ============================================

function evaluateVisibility(
  condition: boolean | ConditionalRule[] | undefined,
  data: Record<string, unknown>,
): boolean {
  if (condition === undefined) return true;
  if (typeof condition === "boolean") return condition;

  // Evaluate conditional rules
  return condition.every((rule) => {
    const fieldValue = data[rule.field];

    switch (rule.operator) {
      case "eq":
        return fieldValue === rule.value;
      case "neq":
        return fieldValue !== rule.value;
      case "gt":
        return Number(fieldValue) > Number(rule.value);
      case "gte":
        return Number(fieldValue) >= Number(rule.value);
      case "lt":
        return Number(fieldValue) < Number(rule.value);
      case "lte":
        return Number(fieldValue) <= Number(rule.value);
      case "in":
        return Array.isArray(rule.value) && rule.value.includes(fieldValue);
      case "notIn":
        return Array.isArray(rule.value) && !rule.value.includes(fieldValue);
      case "contains":
        return String(fieldValue).includes(String(rule.value));
      case "empty":
        return (
          fieldValue === undefined || fieldValue === null || fieldValue === ""
        );
      case "notEmpty":
        return (
          fieldValue !== undefined && fieldValue !== null && fieldValue !== ""
        );
      default:
        return true;
    }
  });
}

function getColumnsClass(columns: number): string {
  switch (columns) {
    case 1:
      return "grid-cols-1";
    case 3:
      return "grid-cols-1 md:grid-cols-3";
    case 4:
      return "grid-cols-1 md:grid-cols-2 lg:grid-cols-4";
    default:
      return "grid-cols-1 md:grid-cols-2";
  }
}

function getFieldWidthClass(width?: string): string {
  switch (width) {
    case "full":
      return "col-span-full";
    case "half":
      return "";
    case "third":
      return "md:col-span-1";
    case "quarter":
      return "lg:col-span-1";
    default:
      return "";
  }
}

export default DynamicForm;
