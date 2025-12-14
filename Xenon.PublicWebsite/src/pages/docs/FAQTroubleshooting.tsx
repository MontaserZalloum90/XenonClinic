import { Link } from 'react-router-dom';
import { HelpCircle, AlertTriangle, Mail, Phone, MessageSquare, ChevronDown, ChevronRight } from 'lucide-react';
import { useState } from 'react';

interface FAQItem {
  question: string;
  answer: string;
  category: string;
}

const faqs: FAQItem[] = [
  {
    category: 'Login & Access',
    question: 'I forgot my password. How do I reset it?',
    answer: 'Click the "Forgot Password" link on the login page. Enter your email address, and you will receive a password reset link. The link expires after 15 minutes. If you don\'t receive the email, check your spam folder or contact your administrator.',
  },
  {
    category: 'Login & Access',
    question: 'My account is locked. What should I do?',
    answer: 'Accounts are locked after 5 failed login attempts as a security measure. Wait 15 minutes for the automatic unlock, or contact your administrator to manually unlock your account.',
  },
  {
    category: 'Login & Access',
    question: 'How do I set up Multi-Factor Authentication (MFA)?',
    answer: 'Go to Settings → Security → Multi-Factor Authentication. Click "Enable MFA" and follow the setup wizard. You can use an authenticator app (recommended) or SMS verification.',
  },
  {
    category: 'Patients',
    question: 'How do I merge duplicate patient records?',
    answer: 'Only administrators can merge patient records. Go to Administration → Patient Management → Merge Patients. Search for both records, select which data to keep, and confirm the merge. This action is logged and cannot be undone.',
  },
  {
    category: 'Patients',
    question: 'Why can\'t I see a patient\'s complete medical history?',
    answer: 'Access to medical history is role-based. If you need access to specific information, check with your supervisor about your permission level. In emergencies, physicians and nurses can use "Break the Glass" access with documented justification.',
  },
  {
    category: 'Appointments',
    question: 'How do I block time on a doctor\'s schedule?',
    answer: 'Go to Appointments → Schedule Management. Select the doctor and date, then click "Block Time". Enter the reason (meeting, vacation, etc.) and the time range. Blocked time will appear as unavailable for patient bookings.',
  },
  {
    category: 'Appointments',
    question: 'A patient didn\'t show up. How do I mark a no-show?',
    answer: 'In the appointment details, click the status dropdown and select "No Show". This updates the patient\'s attendance history and may affect future booking policies based on your clinic\'s configuration.',
  },
  {
    category: 'Laboratory',
    question: 'Lab results are pending for too long. What\'s wrong?',
    answer: 'Check the Lab Queue for the sample status. Common issues include: sample not received by lab, results pending approval, or interface issues with external labs. Contact your lab supervisor if results are overdue.',
  },
  {
    category: 'Laboratory',
    question: 'How do I mark a lab result as critical?',
    answer: 'When entering results, check the "Critical Value" checkbox if results fall outside critical ranges. This triggers an immediate notification to the ordering physician and documents the communication.',
  },
  {
    category: 'Billing',
    question: 'How do I process a refund?',
    answer: 'Go to Financial → Payments → find the original payment. Click "Refund" and enter the amount (full or partial). Select the refund method and add a reason. Refunds require approval from a supervisor based on amount thresholds.',
  },
  {
    category: 'Billing',
    question: 'Insurance claim was rejected. How do I resubmit?',
    answer: 'Go to Financial → Insurance Claims → Rejected Claims. Review the rejection reason, correct any errors in the claim, and click "Resubmit". Common issues include invalid diagnosis codes, missing authorization, or eligibility problems.',
  },
  {
    category: 'Technical',
    question: 'The system is running slowly. What can I do?',
    answer: 'Try refreshing the page, clearing your browser cache, or using a different browser. If issues persist, check your internet connection. Report ongoing performance issues to IT support with the date/time and specific pages affected.',
  },
  {
    category: 'Technical',
    question: 'My report won\'t export. What\'s happening?',
    answer: 'Large reports may take time to generate. Check your Downloads folder for partial files. Try reducing the date range or filtering criteria. If exporting to Excel fails, try PDF format instead.',
  },
];

const commonErrors = [
  {
    code: 'AUTH_001',
    message: 'Session expired',
    solution: 'Your session has timed out due to inactivity. Log in again to continue.',
  },
  {
    code: 'AUTH_002',
    message: 'Insufficient permissions',
    solution: 'You don\'t have permission for this action. Contact your administrator to request access.',
  },
  {
    code: 'PAT_001',
    message: 'Duplicate patient detected',
    solution: 'A patient with similar information exists. Review existing records before creating a new one.',
  },
  {
    code: 'APT_001',
    message: 'Time slot unavailable',
    solution: 'The selected time has been booked by another user. Choose a different time slot.',
  },
  {
    code: 'LAB_001',
    message: 'Sample barcode not found',
    solution: 'Verify the barcode is correct or manually enter the sample ID. The sample may not be registered.',
  },
  {
    code: 'FIN_001',
    message: 'Payment gateway timeout',
    solution: 'The payment processor didn\'t respond. Wait a moment and try again. Check if the payment was processed before retrying.',
  },
  {
    code: 'SYS_001',
    message: 'Service temporarily unavailable',
    solution: 'System maintenance may be in progress. Wait a few minutes and try again. Check system status page for updates.',
  },
];

const categories = Array.from(new Set(faqs.map((faq) => faq.category)));

export default function FAQTroubleshooting() {
  const [expandedQuestions, setExpandedQuestions] = useState<Set<number>>(new Set());
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const toggleQuestion = (index: number) => {
    const newExpanded = new Set(expandedQuestions);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedQuestions(newExpanded);
  };

  const filteredFaqs = selectedCategory === 'all'
    ? faqs
    : faqs.filter((faq) => faq.category === selectedCategory);

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">FAQ & Troubleshooting</h1>
        <p className="text-lg text-gray-600">
          Find answers to common questions and solutions to known issues.
        </p>
      </div>

      {/* Quick Help */}
      <div className="grid sm:grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Mail className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Email Support</h3>
          <p className="text-sm text-gray-600 mt-1">
            support@xenonclinic.com
          </p>
          <p className="text-xs text-gray-500 mt-1">Response within 24 hours</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <Phone className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Phone Support</h3>
          <p className="text-sm text-gray-600 mt-1">
            Available 8 AM - 8 PM
          </p>
          <p className="text-xs text-gray-500 mt-1">Check your contract for number</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <MessageSquare className="h-6 w-6 text-primary-600 mb-3" />
          <h3 className="font-semibold text-gray-900">Live Chat</h3>
          <p className="text-sm text-gray-600 mt-1">
            In-app chat available
          </p>
          <p className="text-xs text-gray-500 mt-1">Click help icon in application</p>
        </div>
      </div>

      {/* Category Filter */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Frequently Asked Questions</h2>
        <div className="flex flex-wrap gap-2 mb-4">
          <button
            onClick={() => setSelectedCategory('all')}
            className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
              selectedCategory === 'all'
                ? 'bg-primary-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            All
          </button>
          {categories.map((category) => (
            <button
              key={category}
              onClick={() => setSelectedCategory(category)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                selectedCategory === category
                  ? 'bg-primary-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {category}
            </button>
          ))}
        </div>

        {/* FAQ List */}
        <div className="space-y-2">
          {filteredFaqs.map((faq, index) => (
            <div
              key={index}
              className="bg-white border border-gray-200 rounded-xl overflow-hidden"
            >
              <button
                onClick={() => toggleQuestion(index)}
                className="w-full px-5 py-4 flex items-center justify-between text-left hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <HelpCircle className="h-5 w-5 text-primary-600 flex-shrink-0" />
                  <span className="font-medium text-gray-900">{faq.question}</span>
                </div>
                {expandedQuestions.has(index) ? (
                  <ChevronDown className="h-5 w-5 text-gray-400" />
                ) : (
                  <ChevronRight className="h-5 w-5 text-gray-400" />
                )}
              </button>
              {expandedQuestions.has(index) && (
                <div className="px-5 pb-4 pl-13">
                  <p className="text-gray-600 ml-8">{faq.answer}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      </section>

      {/* Common Error Codes */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Common Error Codes</h2>
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Code</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Message</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Solution</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {commonErrors.map((error) => (
                <tr key={error.code}>
                  <td className="px-4 py-3">
                    <code className="bg-red-100 text-red-700 px-2 py-0.5 rounded text-sm">
                      {error.code}
                    </code>
                  </td>
                  <td className="px-4 py-3 font-medium text-gray-900">{error.message}</td>
                  <td className="px-4 py-3 text-gray-600">{error.solution}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {/* Troubleshooting Tips */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">General Troubleshooting</h2>
        <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-5">
          <div className="flex items-start gap-3">
            <AlertTriangle className="h-6 w-6 text-yellow-600 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-gray-900">Before Contacting Support</h3>
              <ul className="mt-2 space-y-2 text-gray-600">
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">1.</span>
                  Refresh the page (Ctrl+F5 or Cmd+Shift+R for hard refresh)
                </li>
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">2.</span>
                  Clear browser cache and cookies for this site
                </li>
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">3.</span>
                  Try a different browser (Chrome, Firefox, Edge)
                </li>
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">4.</span>
                  Check your internet connection
                </li>
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">5.</span>
                  Try logging out and back in
                </li>
                <li className="flex items-start gap-2">
                  <span className="font-medium text-yellow-700">6.</span>
                  Note the exact error message and steps to reproduce
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* Related Pages */}
      <section>
        <h2 className="text-2xl font-semibold text-gray-900 mb-4">Related Resources</h2>
        <div className="grid sm:grid-cols-2 gap-4">
          <Link
            to="/docs/getting-started"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Getting Started Guide
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Basic setup and first steps
            </p>
          </Link>
          <Link
            to="/docs/security-rbac"
            className="p-4 bg-white border border-gray-200 rounded-lg hover:border-primary-300 transition-all group"
          >
            <h3 className="font-medium text-gray-900 group-hover:text-primary-600">
              Security & Permissions
            </h3>
            <p className="text-sm text-gray-500 mt-1">
              Understanding access control
            </p>
          </Link>
        </div>
      </section>
    </div>
  );
}
