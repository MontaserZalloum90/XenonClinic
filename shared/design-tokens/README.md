# XenonClinic Design Tokens

Centralized design system tokens for all XenonClinic web applications.

## Purpose

This package provides a single source of truth for design decisions across:
- **Xenon.PublicWebsite** - Marketing & public-facing site
- **XenonClinic.React** - Clinical management application
- Future applications in the XenonClinic ecosystem

## Structure

```
design-tokens/
├── colors.js          # Color palette (primary, semantic, grays)
├── spacing.js         # Spacing scale & container widths
├── typography.js      # Font families, sizes, weights
├── shadows.js         # Box shadows & drop shadows
├── borderRadius.js    # Border radius scale
├── animations.js      # Keyframes, animations, transitions
└── index.js           # Main export file
```

## Usage

### In Tailwind Config

```javascript
// tailwind.config.js
const designTokens = require('../shared/design-tokens');

module.exports = {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: designTokens.theme,
  },
  plugins: [],
};
```

### Importing Specific Tokens

```javascript
const { colors, spacing } = require('../shared/design-tokens');

// Use in custom configuration
const myConfig = {
  primaryColor: colors.primary[600],
  padding: spacing[4],
};
```

## Benefits

1. **Consistency**: Same design language across all applications
2. **Maintainability**: Update once, apply everywhere
3. **Scalability**: Easy to add new applications
4. **Type Safety**: Can be extended with TypeScript definitions
5. **Version Control**: Track design system evolution

## Color System

### Primary Colors
- **Primary**: Blue (#2563eb) - Main brand color
- **Success**: Green (#22c55e) - Positive actions/states
- **Error**: Red (#ef4444) - Errors/destructive actions
- **Warning**: Yellow (#f59e0b) - Warnings/cautions
- **Info**: Blue (#3b82f6) - Informational messages

### Usage in Components
```jsx
// Instead of hardcoded colors
<div className="bg-blue-600 text-white">

// Use semantic tokens
<div className="bg-primary-600 text-white">
```

## Typography

- **Font Family**: Inter (sans-serif)
- **Scale**: xs (12px) to 9xl (128px)
- **Weights**: thin (100) to black (900)

## Spacing

Follows 8px grid system:
- 1 unit = 0.25rem (4px)
- Common values: 2 (8px), 4 (16px), 6 (24px), 8 (32px)

## Animations

All animations are optimized for performance:
- Use GPU-accelerated transforms
- Respects `prefers-reduced-motion`
- Consistent timing functions

## Updating Tokens

1. Edit the relevant token file
2. Test changes in both applications
3. Document breaking changes
4. Update version if needed

## Best Practices

1. **Don't hardcode values** - Always use design tokens
2. **Extend, don't override** - Use Tailwind's extend feature
3. **Semantic naming** - Use semantic color names (primary, success, error)
4. **Accessibility** - Ensure sufficient color contrast (WCAG AA)
5. **Documentation** - Document custom additions

## Roadmap

- [ ] Add TypeScript type definitions
- [ ] Create npm package for easier distribution
- [ ] Add dark mode color tokens
- [ ] Integration tests for token consistency
- [ ] Visual regression testing

## Contributing

When adding new tokens:
1. Follow existing naming conventions
2. Add documentation in this README
3. Test in all applications
4. Consider backwards compatibility

## License

Proprietary - XenonClinic Platform
