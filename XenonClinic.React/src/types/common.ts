// ============================================
// SHARED/COMMON TYPES
// These types are shared across multiple modules
// ============================================

/**
 * Unified PaymentMethod enum that includes all payment types
 * used across Financial, Sales, and other modules.
 */
export const PaymentMethod = {
  Cash: 0,
  Card: 1,
  BankTransfer: 2,
  Insurance: 3,
  Installment: 4,
  Cheque: 5,
  Other: 6,
} as const;

export type PaymentMethod = (typeof PaymentMethod)[keyof typeof PaymentMethod];

/**
 * Common status values used across modules
 */
export const CommonStatus = {
  Active: 0,
  Inactive: 1,
  Pending: 2,
  Cancelled: 3,
} as const;

export type CommonStatus = (typeof CommonStatus)[keyof typeof CommonStatus];

/**
 * Gender enum used in patient and employee records
 */
export const Gender = {
  Male: "M",
  Female: "F",
} as const;

export type Gender = (typeof Gender)[keyof typeof Gender];

/**
 * Priority levels used across modules
 */
export const Priority = {
  Low: 0,
  Normal: 1,
  High: 2,
  Urgent: 3,
  Critical: 4,
} as const;

export type Priority = (typeof Priority)[keyof typeof Priority];
