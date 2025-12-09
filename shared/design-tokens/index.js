/**
 * XenonClinic Design Tokens
 * Centralized design system tokens for all web applications
 *
 * Usage in Tailwind config:
 * import designTokens from '../shared/design-tokens/index.js';
 *
 * export default {
 *   theme: {
 *     extend: designTokens.theme,
 *   },
 * };
 */

import colors from './colors.js';
import spacingTokens from './spacing.js';
import typographyTokens from './typography.js';
import shadowTokens from './shadows.js';
import borderRadiusTokens from './borderRadius.js';
import animationTokens from './animations.js';

const { spacing, container } = spacingTokens;
const { fontFamily, fontSize, fontWeight, letterSpacing, lineHeight } = typographyTokens;
const { boxShadow, dropShadow } = shadowTokens;
const { borderRadius } = borderRadiusTokens;
const { keyframes, animation, transitionDuration, transitionTimingFunction } = animationTokens;

export default {
  // All tokens combined for easy import
  theme: {
    colors,
    spacing,
    container,
    fontFamily,
    fontSize,
    fontWeight,
    letterSpacing,
    lineHeight,
    boxShadow,
    dropShadow,
    borderRadius,
    keyframes,
    animation,
    transitionDuration,
    transitionTimingFunction,
  },

  // Individual exports for granular control
  colors,
  spacing,
  container,
  fontFamily,
  fontSize,
  fontWeight,
  letterSpacing,
  lineHeight,
  boxShadow,
  dropShadow,
  borderRadius,
  keyframes,
  animation,
  transitionDuration,
  transitionTimingFunction,
};
