# XenonClinic React SPA

Modern React-based frontend for XenonClinic Healthcare Management System.

## 🚀 Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool & dev server
- **Tailwind CSS** - Utility-first styling
- **React Router** - Client-side routing
- **React Query** - Server state management
- **Axios** - HTTP client
- **Zustand** - Global state management
- **React Hook Form** - Form handling

## 📋 Prerequisites

- Node.js 18+ and npm
- XenonClinic backend running on `https://localhost:5001`

## 🛠️ Installation

```bash
# Install dependencies
npm install
```

## 🏃 Development

```bash
# Start development server (default: http://localhost:5173)
npm run dev
```

The dev server will automatically proxy API requests to the backend.

## 🏗️ Build

```bash
# Build for production
npm run build

# Preview production build
npm run preview
```

## 📁 Project Structure

```
src/
├── components/      # Reusable components
│   ├── ui/         # UI primitives (Button, Input, etc.)
│   └── layout/     # Layout components
├── contexts/        # React contexts
│   └── AuthContext.tsx
├── hooks/           # Custom React hooks
├── lib/             # Utilities and configurations
│   └── api.ts      # API client
├── pages/           # Page components
│   ├── Login.tsx
│   └── Dashboard.tsx
├── types/           # TypeScript types
│   └── auth.ts
├── App.tsx          # Root component
└── main.tsx         # Entry point
```

## 🔐 Authentication

The app uses JWT token authentication:

1. User logs in with username/password
2. Backend returns JWT token
3. Token stored in localStorage
4. All API requests include token in Authorization header
5. Protected routes check authentication status

**Default Credentials** (if seeded):
- Username: `admin` / Password: `Admin@123`

## 🌐 Environment Variables

Create a `.env` file:

```env
VITE_API_URL=https://localhost:5001
VITE_APP_NAME=XenonClinic
```

## 📡 API Integration

The app connects to XenonClinic backend API endpoints:

- `POST /api/AuthApi/login` - Authentication
- `GET /api/AuthApi/me` - Current user
- `GET /api/AppointmentsApi` - Appointments
- And more...

## 🎨 Styling

Uses Tailwind CSS with custom theme:

- Primary color: Blue (`primary-500` to `primary-900`)
- Utility classes for buttons, inputs, cards
- Responsive design (mobile-first)

## 📝 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## 🔄 State Management

- **Auth State** - AuthContext (login, logout, user)
- **Server State** - React Query (API data caching)
- **Global State** - Zustand (if needed)

## 🚀 Deployment

The built files can be:
1. Served by the .NET backend as static files
2. Deployed to CDN/static hosting
3. Deployed to Vercel/Netlify

## 📖 Next Steps

- Implement remaining modules (Patients, Laboratory, etc.)
- Add comprehensive error handling
- Add loading states and skeletons
- Add toast notifications
- Implement role-based access control
- Add unit and integration tests

## 🤝 Contributing

This is part of the XenonClinic Healthcare Management System.

## 📄 License

Copyright © 2024 XenonClinic
