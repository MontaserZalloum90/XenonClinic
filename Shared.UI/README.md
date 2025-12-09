# @xenon/ui

Shared UI components, utilities, and styles for Xenon applications.

## Installation

Add to your `package.json`:

```json
{
  "dependencies": {
    "@xenon/ui": "file:../Shared.UI"
  }
}
```

## Components

### Toast Notifications

```tsx
import { ToastProvider, useToast } from '@xenon/ui';

// Wrap your app
function App() {
  return (
    <ToastProvider>
      <MyComponent />
    </ToastProvider>
  );
}

// Use in components
function MyComponent() {
  const { showToast } = useToast();

  return (
    <button onClick={() => showToast('success', 'Operation completed!')}>
      Show Toast
    </button>
  );
}
```

**Toast Types:** `success`, `error`, `info`, `warning`

### Loading Skeletons

```tsx
import { LoadingSkeleton, SkeletonText, SkeletonCard, SkeletonTable } from '@xenon/ui';

// Basic skeleton
<LoadingSkeleton variant="text" width="200px" />
<LoadingSkeleton variant="rectangular" height="100px" />
<LoadingSkeleton variant="circular" width="40px" height="40px" />

// Pre-built patterns
<SkeletonText lines={3} />
<SkeletonCard />
<SkeletonTable rows={5} columns={4} />
```

### Modal

```tsx
import { Modal } from '@xenon/ui';

<Modal
  isOpen={isOpen}
  onClose={() => setIsOpen(false)}
  title="Modal Title"
  size="md" // sm, md, lg, xl
>
  <p>Modal content goes here</p>
</Modal>
```

### Confirm Dialog

```tsx
import { ConfirmDialog, useConfirmDialog } from '@xenon/ui';

function MyComponent() {
  const { dialogState, showConfirm, hideConfirm, handleConfirm } = useConfirmDialog();

  const handleDelete = () => {
    showConfirm(
      'Delete Item',
      'Are you sure you want to delete this item?',
      () => deleteItem(),
      'danger' // danger, warning, info
    );
  };

  return (
    <>
      <button onClick={handleDelete}>Delete</button>
      <ConfirmDialog
        {...dialogState}
        onClose={hideConfirm}
        onConfirm={handleConfirm}
      />
    </>
  );
}
```

### Empty State

```tsx
import { EmptyState } from '@xenon/ui';

<EmptyState
  icon={<SearchIcon />}
  title="No results found"
  description="Try adjusting your search criteria"
  action={{
    label: 'Clear filters',
    onClick: () => clearFilters()
  }}
/>
```

### Badge

```tsx
import { Badge, StatusBadge } from '@xenon/ui';

// Generic badge
<Badge variant="success">Active</Badge>
<Badge variant="warning" dot>Pending</Badge>
<Badge variant="danger" size="lg">Critical</Badge>

// Pre-configured status badge
<StatusBadge status="active" />   // green
<StatusBadge status="pending" />  // yellow
<StatusBadge status="error" />    // red
```

**Badge Variants:** `default`, `primary`, `secondary`, `success`, `warning`, `danger`, `info`

**Badge Sizes:** `sm`, `md`, `lg`

## Styles

### Tailwind Preset

Use the shared Tailwind configuration:

```js
// tailwind.config.js
import sharedPreset from '../Shared.UI/src/theme/tailwind-preset.js';

export default {
  presets: [sharedPreset],
  content: [
    "./src/**/*.{js,ts,jsx,tsx}",
    "../Shared.UI/src/**/*.{js,ts,jsx,tsx}",
  ],
}
```

### CSS Utility Classes

Import shared component styles in your CSS:

```css
@import '@xenon/ui/styles';
```

**Available Classes:**

| Category | Classes |
|----------|---------|
| Buttons | `.btn`, `.btn-primary`, `.btn-secondary`, `.btn-danger`, `.btn-success`, `.btn-outline`, `.btn-ghost`, `.btn-sm`, `.btn-lg` |
| Inputs | `.input`, `.input-error`, `.input-lg`, `.textarea`, `.select` |
| Cards | `.card`, `.card-body`, `.card-header`, `.card-footer`, `.card-hover` |
| Badges | `.badge`, `.badge-primary`, `.badge-success`, `.badge-warning`, `.badge-danger`, `.badge-info` |
| Typography | `.heading-1`, `.heading-2`, `.heading-3`, `.heading-4`, `.text-muted` |
| Links | `.link`, `.link-secondary` |
| Layout | `.container-app`, `.section-padding`, `.divider`, `.divider-vertical` |

## API Utilities

### Create API Client

```tsx
import { createApiClient, buildQueryString } from '@xenon/ui';

const api = createApiClient('https://api.example.com');

// GET request
const users = await api.get<User[]>('/users');

// POST request
const newUser = await api.post<User>('/users', { name: 'John' });

// With query string
const query = buildQueryString({ page: 1, limit: 10 });
const results = await api.get(`/search${query}`);
```

### Token Storage

```tsx
import { tokenStorage } from '@xenon/ui';

tokenStorage.setToken('jwt-token');
tokenStorage.getToken(); // 'jwt-token'
tokenStorage.isAuthenticated(); // true
tokenStorage.removeToken();
```

### Error Handling

```tsx
import { isApiError, getErrorMessage, withRetry } from '@xenon/ui';

// Check error type
if (isApiError(error)) {
  console.log(error.status, error.message);
}

// Get error message safely
const message = getErrorMessage(error);

// Retry failed requests
const result = await withRetry(
  () => api.get('/flaky-endpoint'),
  { maxRetries: 3, delay: 1000, backoff: 2 }
);
```

## Development

```bash
# Type check
npm run typecheck

# Run tests
npm run test

# Run tests once
npm run test:run
```

## License

UNLICENSED - Internal use only
