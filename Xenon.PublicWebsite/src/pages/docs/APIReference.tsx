import { Code, Lock, Zap, FileJson, ExternalLink } from 'lucide-react';

const endpoints = [
  { module: 'Appointments', base: '/api/appointments', methods: ['GET', 'POST', 'PUT', 'DELETE'] },
  { module: 'Patients', base: '/api/patient', methods: ['GET', 'POST', 'PUT', 'DELETE'] },
  { module: 'Laboratory', base: '/api/laboratory', methods: ['GET', 'POST', 'PUT'] },
  { module: 'Financial', base: '/api/financial', methods: ['GET', 'POST', 'PUT'] },
  { module: 'HR', base: '/api/hr', methods: ['GET', 'POST', 'PUT', 'DELETE'] },
  { module: 'Inventory', base: '/api/inventory', methods: ['GET', 'POST', 'PUT', 'DELETE'] },
  { module: 'Radiology', base: '/api/radiology', methods: ['GET', 'POST', 'PUT'] },
  { module: 'Analytics', base: '/api/analytics', methods: ['GET', 'POST'] },
  { module: 'Security', base: '/api/security', methods: ['GET', 'POST', 'PUT', 'DELETE'] },
  { module: 'Workflows', base: '/api/workflows', methods: ['GET', 'POST', 'PUT'] },
];

const methodColors: Record<string, string> = {
  GET: 'bg-green-100 text-green-700',
  POST: 'bg-blue-100 text-blue-700',
  PUT: 'bg-yellow-100 text-yellow-700',
  DELETE: 'bg-red-100 text-red-700',
};

export default function APIReference() {
  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">API Reference</h1>
        <p className="text-lg text-gray-600">
          XenonClinic provides a comprehensive REST API for integration with external
          systems. All endpoints require authentication and follow RESTful conventions.
        </p>
      </div>

      {/* Quick Info */}
      <div className="grid sm:grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Code className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">REST API</h3>
          <p className="text-sm text-gray-600 mt-1">
            Standard REST endpoints with JSON payloads
          </p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Lock className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">JWT Authentication</h3>
          <p className="text-sm text-gray-600 mt-1">
            Bearer token authentication required
          </p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Zap className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Rate Limited</h3>
          <p className="text-sm text-gray-600 mt-1">
            100 requests/minute global limit
          </p>
        </div>
      </div>

      {/* Base URL */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Base URL</h2>
        <div className="bg-gray-900 text-gray-100 rounded-xl p-4 font-mono">
          <code>https://api.yourcompany.xenonclinic.com</code>
        </div>
      </section>

      {/* Authentication */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Authentication</h2>
        <div className="bg-gray-900 text-gray-100 rounded-xl p-5 font-mono text-sm overflow-x-auto">
          <pre>
{`# Request Token
POST /api/auth/login
Content-Type: application/json

{
  "username": "user@example.com",
  "password": "your-password"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "refreshToken": "abc123..."
}

# Use Token
GET /api/patients
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`}
          </pre>
        </div>
      </section>

      {/* Endpoints */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Available Endpoints</h2>
        <div className="space-y-4">
          {endpoints.map((endpoint) => (
            <div key={endpoint.base} className="bg-white border border-gray-200 rounded-xl p-5">
              <div className="flex items-center justify-between mb-3">
                <h3 className="font-semibold text-gray-900">{endpoint.module}</h3>
                <code className="bg-gray-100 px-3 py-1 rounded text-sm">{endpoint.base}</code>
              </div>
              <div className="flex flex-wrap gap-2">
                {endpoint.methods.map((method) => (
                  <span
                    key={method}
                    className={`px-2 py-0.5 rounded text-xs font-medium ${methodColors[method]}`}
                  >
                    {method}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Response Format */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Response Format</h2>
        <div className="bg-gray-900 text-gray-100 rounded-xl p-5 font-mono text-sm overflow-x-auto">
          <pre>
{`# Success Response
{
  "success": true,
  "data": { ... },
  "message": null,
  "timestamp": "2024-12-14T10:30:00Z",
  "traceId": "abc-123-def"
}

# Error Response
{
  "success": false,
  "data": null,
  "error": "Validation failed",
  "validationErrors": {
    "email": ["Invalid email format"]
  },
  "timestamp": "2024-12-14T10:30:00Z",
  "traceId": "abc-123-def"
}

# Paginated Response
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}`}
          </pre>
        </div>
      </section>

      {/* Error Codes */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">HTTP Status Codes</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Meaning</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              <tr><td className="px-4 py-3 font-mono text-green-600">200</td><td className="px-4 py-3 text-gray-600">Success</td></tr>
              <tr><td className="px-4 py-3 font-mono text-green-600">201</td><td className="px-4 py-3 text-gray-600">Created</td></tr>
              <tr><td className="px-4 py-3 font-mono text-yellow-600">400</td><td className="px-4 py-3 text-gray-600">Bad Request - Validation error</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">401</td><td className="px-4 py-3 text-gray-600">Unauthorized - Invalid/missing token</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">403</td><td className="px-4 py-3 text-gray-600">Forbidden - Insufficient permissions</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">404</td><td className="px-4 py-3 text-gray-600">Not Found</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">429</td><td className="px-4 py-3 text-gray-600">Rate Limit Exceeded</td></tr>
              <tr><td className="px-4 py-3 font-mono text-red-600">500</td><td className="px-4 py-3 text-gray-600">Internal Server Error</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      {/* Swagger Link */}
      <section className="bg-primary-50 border border-primary-200 rounded-xl p-6">
        <div className="flex items-start gap-4">
          <FileJson className="h-8 w-8 text-primary-600 flex-shrink-0" />
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Interactive API Documentation</h3>
            <p className="text-gray-600 mt-1">
              Access the full Swagger/OpenAPI documentation for testing endpoints directly.
            </p>
            <a
              href="#"
              className="inline-flex items-center gap-2 mt-3 text-primary-600 font-medium hover:underline"
            >
              Open Swagger UI <ExternalLink className="h-4 w-4" />
            </a>
          </div>
        </div>
      </section>
    </div>
  );
}
