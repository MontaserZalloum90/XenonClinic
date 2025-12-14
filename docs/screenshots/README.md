# XenonClinic Documentation Screenshots

This directory contains the screenshot automation pipeline for the XenonClinic documentation portal.

## Overview

Screenshots are automatically captured from the running application and used in the documentation to illustrate features, workflows, and UI elements.

## Directory Structure

```
screenshots/
├── README.md                    # This file
├── screenshot-config.json       # Configuration for all screenshots
├── capture-screenshots.ts       # Main capture script
└── output/                      # Generated screenshots (gitignored)
    ├── manifest.json            # Index of all captured screenshots
    ├── patient-management/      # Screenshots by module
    ├── appointments/
    ├── laboratory/
    └── ...
```

## Prerequisites

1. **Application Running**: Both admin and public apps must be running
   ```bash
   npm run dev -w XenonClinic.React      # Admin app on :5173
   npm run dev -w Xenon.PublicWebsite    # Public site on :5174
   ```

2. **Test Data**: Database should be seeded with test data for realistic screenshots

3. **Playwright**: Install Playwright if not already available
   ```bash
   npx playwright install chromium
   ```

## Usage

### Capture All Screenshots

```bash
npx ts-node docs/screenshots/capture-screenshots.ts
```

### Capture Screenshots for Specific Module

```bash
npx ts-node docs/screenshots/capture-screenshots.ts --module=patient-management
npx ts-node docs/screenshots/capture-screenshots.ts --module=appointments
npx ts-node docs/screenshots/capture-screenshots.ts --module=laboratory
```

### Available Modules

- `patient-management`
- `appointments`
- `clinical-visits`
- `laboratory`
- `pharmacy`
- `inventory`
- `financial`
- `hr-management`
- `analytics`
- `workflow-engine`
- `patient-portal`
- `security-audit`

## Configuration

Edit `screenshot-config.json` to add or modify screenshots:

```json
{
  "id": "unique-screenshot-id",
  "module": "module-name",
  "description": "What this screenshot shows",
  "url": "/path/in/app",
  "app": "admin",
  "selector": "main",
  "delay": 1000,
  "requiresTestData": false
}
```

### Configuration Options

| Option | Type | Description |
|--------|------|-------------|
| `id` | string | Unique identifier for the screenshot |
| `module` | string | Module this screenshot belongs to |
| `description` | string | Human-readable description |
| `url` | string | URL path to navigate to |
| `app` | string | Which app: "admin" or "public" |
| `selector` | string | CSS selector for element to capture (optional) |
| `action` | string | Action to perform before capture, e.g., "click:.btn" |
| `delay` | number | Milliseconds to wait before capture |
| `requiresTestData` | boolean | Whether screenshot needs specific test data |

## Using Screenshots in Documentation

Screenshots are referenced in documentation pages using the output path:

```tsx
<img
  src="/docs/screenshots/patient-management/patient-list.png"
  alt="Patient listing view"
/>
```

Or use the `DocScreenshot` component:

```tsx
<DocScreenshot
  module="patient-management"
  id="patient-list"
  caption="Patient listing with search and filters"
/>
```

## Automation

Screenshots can be regenerated as part of CI/CD:

```yaml
# .github/workflows/docs-screenshots.yml
- name: Generate Screenshots
  run: |
    npm run dev &
    sleep 10
    npx ts-node docs/screenshots/capture-screenshots.ts
```

## Best Practices

1. **Consistent Data**: Use seeded test data for reproducible screenshots
2. **Wait for Load**: Set appropriate `delay` for dynamic content
3. **Responsive**: Capture both desktop and mobile viewports where relevant
4. **Clean UI**: Hide tooltips, close modals, reset filters before capture
5. **Annotations**: Add annotations using image editing tools post-capture

## Troubleshooting

### Screenshot is blank or missing elements
- Increase the `delay` value
- Check if the `selector` matches an element

### App not responding
- Ensure both apps are running
- Check the `baseUrls` in config match your setup

### Playwright errors
- Run `npx playwright install` to install browsers
- Check for browser-specific issues with `--headed` flag
