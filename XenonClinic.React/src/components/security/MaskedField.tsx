/**
 * PII Masking Components
 * Display sensitive information with masking and reveal controls
 */

import { useState, useCallback } from "react";
import { EyeIcon, EyeSlashIcon } from "@heroicons/react/24/outline";
import {
  maskSensitiveData,
  maskEmiratesId,
  maskPhoneNumber,
  maskEmail,
} from "../../lib/security/encryption";
import { auditLog, AuditAction, AuditResourceType } from "../../lib/security";

// ============================================
// TYPES
// ============================================

export type MaskType =
  | "emiratesId"
  | "phone"
  | "email"
  | "ssn"
  | "creditCard"
  | "custom";

export interface MaskedFieldProps {
  value: string;
  type?: MaskType;
  label?: string;
  allowReveal?: boolean;
  requiresJustification?: boolean;
  logAccess?: boolean;
  resourceType?: AuditResourceType;
  resourceId?: number | string;
  resourceDescription?: string;
  className?: string;
  revealDuration?: number; // Auto-hide after ms (0 = never)
}

// ============================================
// MASKING FUNCTIONS
// ============================================

const getMaskedValue = (value: string, type: MaskType): string => {
  if (!value) return "";

  switch (type) {
    case "emiratesId":
      return maskEmiratesId(value);
    case "phone":
      return maskPhoneNumber(value);
    case "email":
      return maskEmail(value);
    case "ssn":
      // Show last 4 digits
      return maskSensitiveData(value.replace(/\D/g, ""), { visibleEnd: 4 });
    case "creditCard":
      // Show last 4 digits
      return maskSensitiveData(value.replace(/\D/g, ""), { visibleEnd: 4 });
    case "custom":
    default:
      return maskSensitiveData(value);
  }
};

// ============================================
// MASKED FIELD COMPONENT
// ============================================

export const MaskedField = ({
  value,
  type = "custom",
  label,
  allowReveal = true,
  requiresJustification = false,
  logAccess = true,
  resourceType = AuditResourceType.PATIENT,
  resourceId,
  resourceDescription,
  className = "",
  revealDuration = 30000, // 30 seconds default
}: MaskedFieldProps) => {
  const [isRevealed, setIsRevealed] = useState(false);
  const [showJustificationModal, setShowJustificationModal] = useState(false);

  const maskedValue = getMaskedValue(value, type);

  const revealValue = useCallback(
    (justification: string) => {
      setIsRevealed(true);

      // Log the access
      if (logAccess) {
        auditLog.log(AuditAction.VIEW_DETAIL, resourceType, {
          resourceId,
          resourceDescription: resourceDescription || `Revealed ${type} field`,
          details: { fieldType: type, justification },
        });
      }

      // Auto-hide after duration
      if (revealDuration > 0) {
        setTimeout(() => {
          setIsRevealed(false);
        }, revealDuration);
      }
    },
    [
      logAccess,
      resourceType,
      resourceId,
      resourceDescription,
      type,
      revealDuration,
    ],
  );

  const handleReveal = useCallback(() => {
    if (requiresJustification) {
      setShowJustificationModal(true);
      return;
    }

    revealValue("Routine access");
  }, [requiresJustification, revealValue]);

  const handleHide = useCallback(() => {
    setIsRevealed(false);
  }, []);

  const handleJustificationSubmit = useCallback(
    (justification: string) => {
      setShowJustificationModal(false);
      revealValue(justification);
    },
    [revealValue],
  );

  return (
    <div className={`inline-flex items-center gap-2 ${className}`}>
      {label && <span className="text-sm text-gray-600">{label}:</span>}
      <span className={`font-mono ${isRevealed ? "" : "tracking-wider"}`}>
        {isRevealed ? value : maskedValue}
      </span>
      {allowReveal && value && (
        <button
          type="button"
          onClick={isRevealed ? handleHide : handleReveal}
          className="p-1 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded transition-colors"
          title={isRevealed ? "Hide" : "Reveal"}
        >
          {isRevealed ? (
            <EyeSlashIcon className="h-4 w-4" />
          ) : (
            <EyeIcon className="h-4 w-4" />
          )}
        </button>
      )}

      {/* Justification Modal */}
      {showJustificationModal && (
        <JustificationModal
          onSubmit={handleJustificationSubmit}
          onCancel={() => setShowJustificationModal(false)}
          _fieldType={type}
        />
      )}
    </div>
  );
};

// ============================================
// JUSTIFICATION MODAL
// ============================================

interface JustificationModalProps {
  onSubmit: (justification: string) => void;
  onCancel: () => void;
  _fieldType: MaskType;
}

const JustificationModal = ({
  onSubmit,
  onCancel,
  _fieldType,
}: JustificationModalProps) => {
  const [justification, setJustification] = useState("");
  const [selectedReason, setSelectedReason] = useState("");

  const reasons = [
    "Patient verification",
    "Medical record review",
    "Billing/Insurance processing",
    "Treatment planning",
    "Emergency access",
    "Other (specify)",
  ];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const finalJustification =
      selectedReason === "Other (specify)" ? justification : selectedReason;
    if (finalJustification) {
      onSubmit(finalJustification);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
        <div className="px-6 py-4 border-b">
          <h3 className="text-lg font-semibold text-gray-900">
            Access Justification Required
          </h3>
          <p className="text-sm text-gray-600 mt-1">
            Please provide a reason for viewing this sensitive information.
          </p>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="p-6 space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Reason for Access
              </label>
              <div className="space-y-2">
                {reasons.map((reason) => (
                  <label key={reason} className="flex items-center">
                    <input
                      type="radio"
                      name="reason"
                      value={reason}
                      checked={selectedReason === reason}
                      onChange={(e) => setSelectedReason(e.target.value)}
                      className="mr-2 text-primary-600 focus:ring-primary-500"
                    />
                    <span className="text-sm text-gray-700">{reason}</span>
                  </label>
                ))}
              </div>
            </div>

            {selectedReason === "Other (specify)" && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Please specify
                </label>
                <input
                  type="text"
                  value={justification}
                  onChange={(e) => setJustification(e.target.value)}
                  className="w-full px-3 py-2 border rounded-md focus:ring-primary-500 focus:border-primary-500"
                  placeholder="Enter justification..."
                  required
                />
              </div>
            )}

            <p className="text-xs text-gray-500">
              This access will be logged for compliance purposes.
            </p>
          </div>
          <div className="px-6 py-4 border-t bg-gray-50 flex justify-end gap-3">
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-md"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={
                !selectedReason ||
                (selectedReason === "Other (specify)" && !justification)
              }
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50"
            >
              Reveal Information
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// ============================================
// SPECIALIZED MASKED COMPONENTS
// ============================================

interface SimpleMaskedFieldProps {
  value: string;
  label?: string;
  allowReveal?: boolean;
  className?: string;
}

export const MaskedEmiratesId = ({
  value,
  label = "Emirates ID",
  ...props
}: SimpleMaskedFieldProps) => (
  <MaskedField
    value={value}
    type="emiratesId"
    label={label}
    resourceDescription="Emirates ID"
    requiresJustification={true}
    {...props}
  />
);

export const MaskedPhone = ({
  value,
  label = "Phone",
  ...props
}: SimpleMaskedFieldProps) => (
  <MaskedField
    value={value}
    type="phone"
    label={label}
    resourceDescription="Phone number"
    {...props}
  />
);

export const MaskedEmail = ({
  value,
  label = "Email",
  ...props
}: SimpleMaskedFieldProps) => (
  <MaskedField
    value={value}
    type="email"
    label={label}
    resourceDescription="Email address"
    {...props}
  />
);

export const MaskedSSN = ({
  value,
  label = "SSN",
  ...props
}: SimpleMaskedFieldProps) => (
  <MaskedField
    value={value}
    type="ssn"
    label={label}
    resourceDescription="Social Security Number"
    requiresJustification={true}
    {...props}
  />
);

// ============================================
// BULK MASKING HOOK
// ============================================

interface UseMaskedDataOptions {
  autoMask?: boolean;
  logAccess?: boolean;
}

export const useMaskedData = <T extends Record<string, unknown>>(
  data: T,
  fieldsToMask: Array<{ field: keyof T; type: MaskType }>,
  options: UseMaskedDataOptions = {},
): T => {
  const { autoMask = true } = options;

  if (!autoMask) return data;

  const maskedData = { ...data };

  for (const { field, type } of fieldsToMask) {
    const value = data[field];
    if (typeof value === "string") {
      (maskedData as Record<string, unknown>)[field as string] = getMaskedValue(
        value,
        type,
      );
    }
  }

  return maskedData;
};
