import React from "react";
import { useT } from "../../contexts/TenantContext";

/**
 * T Component - Renders translated/terminology text
 *
 * Usage:
 * <T k="entity.patient.singular" />
 * <T k="page.patients.title" fallback="Patients" />
 * <T k="nav.appointments" as="span" className="font-bold" />
 */

interface TProps {
  /** Terminology key */
  k: string;
  /** Fallback text if key not found */
  fallback?: string;
  /** HTML element to render as (default: span) */
  as?: keyof JSX.IntrinsicElements;
  /** Additional className */
  className?: string;
  /** Additional props */
  [key: string]: unknown;
}

export const T: React.FC<TProps> = ({
  k,
  fallback,
  as: Component = "span",
  className,
  ...props
}) => {
  const t = useT();
  const text = t(k, fallback);

  return React.createElement(Component, { className, ...props }, text);
};

/**
 * Hook + Component for terminology with interpolation
 *
 * Usage:
 * const message = tInterpolate('greeting.welcome', { name: 'John' });
 * // If terminology has "greeting.welcome": "Hello, {name}!"
 * // Result: "Hello, John!"
 */
export const interpolateTerminology = (
  template: string,
  values: Record<string, string | number>,
): string => {
  return template.replace(/\{(\w+)\}/g, (_, key) => {
    return values[key]?.toString() ?? `{${key}}`;
  });
};

/**
 * TInterpolate Component - Renders translated text with variable interpolation
 */
interface TInterpolateProps extends Omit<TProps, "fallback"> {
  values: Record<string, string | number>;
  fallback?: string;
}

export const TInterpolate: React.FC<TInterpolateProps> = ({
  k,
  values,
  fallback,
  as: Component = "span",
  className,
  ...props
}) => {
  const t = useT();
  const template = t(k, fallback);
  const text = interpolateTerminology(template, values);

  return React.createElement(Component, { className, ...props }, text);
};

/**
 * TPlural Component - Handles singular/plural forms
 *
 * Usage:
 * <TPlural count={5} singular="entity.patient.singular" plural="entity.patient.plural" />
 * // Shows "5 Patients" (if terminology configured)
 */
interface TPluralProps extends Omit<TProps, "k"> {
  count: number;
  singular: string;
  plural: string;
  showCount?: boolean;
}

export const TPlural: React.FC<TPluralProps> = ({
  count,
  singular,
  plural,
  showCount = true,
  as: Component = "span",
  className,
  ...props
}) => {
  const t = useT();
  const key = count === 1 ? singular : plural;
  const text = t(key);
  const displayText = showCount ? `${count} ${text}` : text;

  return React.createElement(Component, { className, ...props }, displayText);
};

export default T;
