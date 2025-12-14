import { useState } from 'react';
import { BookOpen, Search } from 'lucide-react';

interface GlossaryTerm {
  term: string;
  definition: string;
  category: string;
  relatedTerms?: string[];
}

const glossaryTerms: GlossaryTerm[] = [
  // A
  {
    term: 'Appointment Slot',
    definition: 'A specific time block reserved for patient consultations. Slots are configured per provider and can have different durations based on appointment type.',
    category: 'Appointments',
    relatedTerms: ['Provider Schedule', 'Booking'],
  },
  {
    term: 'Audit Log',
    definition: 'A chronological record of system activities including user actions, data access, and changes. Audit logs are retained for 7 years for compliance.',
    category: 'Security',
    relatedTerms: ['Compliance', 'HIPAA'],
  },
  {
    term: 'Audiogram',
    definition: 'A graph showing hearing test results at different frequencies. Records air conduction, bone conduction, and speech recognition thresholds.',
    category: 'Audiology',
  },
  // B
  {
    term: 'Branch',
    definition: 'A physical location or clinic within a tenant organization. Each branch can have its own staff, schedules, and inventory.',
    category: 'Multi-Tenancy',
    relatedTerms: ['Tenant', 'Location'],
  },
  {
    term: 'Break the Glass',
    definition: 'Emergency access protocol allowing authorized users to bypass normal access restrictions to view patient data. Requires documented justification and triggers compliance review.',
    category: 'Security',
    relatedTerms: ['Emergency Access', 'PHI'],
  },
  // C
  {
    term: 'Chief Complaint',
    definition: 'The primary reason a patient seeks medical attention, documented in their own words or as described to the provider.',
    category: 'Clinical',
    relatedTerms: ['SOAP Note', 'Clinical Visit'],
  },
  {
    term: 'Clinical Visit',
    definition: 'A documented patient encounter including vitals, examination, diagnosis, and treatment plan. Forms the core of the patient medical record.',
    category: 'Clinical',
  },
  {
    term: 'CPT Code',
    definition: 'Current Procedural Terminology code used to describe medical, surgical, and diagnostic services for billing purposes.',
    category: 'Billing',
    relatedTerms: ['ICD Code', 'Claim'],
  },
  // D
  {
    term: 'Dashboard',
    definition: 'A personalized home screen showing key metrics, pending tasks, and quick actions relevant to the user\'s role.',
    category: 'General',
  },
  {
    term: 'DEA Number',
    definition: 'Drug Enforcement Administration registration number required for prescribing controlled substances. Verified during provider setup.',
    category: 'Pharmacy',
    relatedTerms: ['Controlled Substance', 'Prescription'],
  },
  // E
  {
    term: 'EHR',
    definition: 'Electronic Health Record - digital version of a patient\'s medical history maintained by the healthcare provider.',
    category: 'General',
  },
  {
    term: 'Encounter',
    definition: 'Any interaction between a patient and healthcare provider, including visits, phone calls, and telehealth sessions.',
    category: 'Clinical',
  },
  // H
  {
    term: 'HIPAA',
    definition: 'Health Insurance Portability and Accountability Act - US legislation providing data privacy and security provisions for safeguarding medical information.',
    category: 'Compliance',
    relatedTerms: ['PHI', 'Audit Log'],
  },
  // I
  {
    term: 'ICD Code',
    definition: 'International Classification of Diseases code used to classify diagnoses and health conditions for billing and statistics.',
    category: 'Billing',
    relatedTerms: ['CPT Code', 'Diagnosis'],
  },
  {
    term: 'Insurance Claim',
    definition: 'A request submitted to an insurance company for payment of healthcare services rendered to a patient.',
    category: 'Billing',
    relatedTerms: ['ERA', 'Adjudication'],
  },
  // J
  {
    term: 'Journey',
    definition: 'A documented workflow showing the step-by-step process for completing a common task, from start to finish.',
    category: 'General',
  },
  // L
  {
    term: 'Lab Order',
    definition: 'A request for laboratory tests on patient samples. Includes specimen requirements, priority level, and ordering provider.',
    category: 'Laboratory',
    relatedTerms: ['Lab Result', 'Sample'],
  },
  {
    term: 'Lab Result',
    definition: 'The outcome of laboratory analysis, including values, reference ranges, and interpretation notes.',
    category: 'Laboratory',
  },
  // M
  {
    term: 'MFA',
    definition: 'Multi-Factor Authentication - security mechanism requiring two or more verification factors to access an account.',
    category: 'Security',
  },
  {
    term: 'Module',
    definition: 'A functional component of XenonClinic that handles a specific area of clinic operations (e.g., Appointments, Laboratory, Billing).',
    category: 'General',
  },
  // P
  {
    term: 'Patient Portal',
    definition: 'Secure online platform where patients can view their health information, schedule appointments, and communicate with providers.',
    category: 'Patient',
  },
  {
    term: 'Permission',
    definition: 'A specific capability granted to a role, such as "patients:view" or "appointments:create". Controls access to features and data.',
    category: 'Security',
    relatedTerms: ['Role', 'RBAC'],
  },
  {
    term: 'PHI',
    definition: 'Protected Health Information - any individually identifiable health information that is transmitted or maintained in any form.',
    category: 'Compliance',
    relatedTerms: ['HIPAA', 'Privacy'],
  },
  {
    term: 'Provider',
    definition: 'A healthcare professional who delivers clinical services, including physicians, nurses, and specialists.',
    category: 'Clinical',
  },
  // R
  {
    term: 'RBAC',
    definition: 'Role-Based Access Control - security approach where permissions are assigned to roles, and users are assigned to roles.',
    category: 'Security',
    relatedTerms: ['Permission', 'Role'],
  },
  {
    term: 'Referral',
    definition: 'A recommendation from one provider to another for specialized care or services for a patient.',
    category: 'Clinical',
  },
  {
    term: 'Role',
    definition: 'A defined set of permissions assigned to users based on their job function (e.g., Doctor, Nurse, Receptionist).',
    category: 'Security',
    relatedTerms: ['Permission', 'RBAC'],
  },
  // S
  {
    term: 'SOAP Note',
    definition: 'Documentation format: Subjective (patient symptoms), Objective (exam findings), Assessment (diagnosis), Plan (treatment).',
    category: 'Clinical',
  },
  {
    term: 'Specimen',
    definition: 'A biological sample collected from a patient for laboratory analysis (blood, urine, tissue, etc.).',
    category: 'Laboratory',
    relatedTerms: ['Lab Order', 'Sample Collection'],
  },
  // T
  {
    term: 'Tenant',
    definition: 'An organization (clinic or hospital group) using XenonClinic. Each tenant has isolated data and can have multiple branches.',
    category: 'Multi-Tenancy',
    relatedTerms: ['Branch', 'Organization'],
  },
  {
    term: 'Triage',
    definition: 'The process of determining the priority of patient treatment based on the severity of their condition.',
    category: 'Clinical',
  },
  // V
  {
    term: 'Vitals',
    definition: 'Basic physiological measurements including blood pressure, heart rate, temperature, respiratory rate, and oxygen saturation.',
    category: 'Clinical',
  },
  // W
  {
    term: 'Workflow',
    definition: 'An automated sequence of tasks triggered by events, with conditional logic and notifications.',
    category: 'General',
    relatedTerms: ['Automation', 'Trigger'],
  },
  {
    term: 'Write-off',
    definition: 'The process of removing uncollectible amounts from patient accounts, typically requiring supervisor approval.',
    category: 'Billing',
  },
];

const categories = Array.from(new Set(glossaryTerms.map((t) => t.category))).sort();

export default function Glossary() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const sortedTerms = [...glossaryTerms].sort((a, b) => a.term.localeCompare(b.term));

  const filteredTerms = sortedTerms.filter((term) => {
    const matchesSearch = searchQuery === '' ||
      term.term.toLowerCase().includes(searchQuery.toLowerCase()) ||
      term.definition.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === 'all' || term.category === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  // Group by first letter
  const groupedTerms = filteredTerms.reduce((acc, term) => {
    const letter = term.term[0].toUpperCase();
    if (!acc[letter]) {
      acc[letter] = [];
    }
    acc[letter].push(term);
    return acc;
  }, {} as Record<string, GlossaryTerm[]>);

  const letters = Object.keys(groupedTerms).sort();

  return (
    <div className="space-y-10">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Glossary</h1>
        <p className="text-lg text-gray-600">
          Definitions of terms and concepts used throughout XenonClinic.
        </p>
      </div>

      {/* Search and Filter */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search terms..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="all">All Categories</option>
          {categories.map((category) => (
            <option key={category} value={category}>
              {category}
            </option>
          ))}
        </select>
      </div>

      {/* Letter Navigation */}
      <div className="flex flex-wrap gap-1">
        {Array.from('ABCDEFGHIJKLMNOPQRSTUVWXYZ').map((letter) => {
          const hasTerms = groupedTerms[letter]?.length > 0;
          return (
            <a
              key={letter}
              href={hasTerms ? `#letter-${letter}` : undefined}
              className={`w-8 h-8 flex items-center justify-center rounded text-sm font-medium ${
                hasTerms
                  ? 'bg-primary-100 text-primary-700 hover:bg-primary-200'
                  : 'bg-gray-100 text-gray-400 cursor-not-allowed'
              }`}
            >
              {letter}
            </a>
          );
        })}
      </div>

      {/* Terms List */}
      {letters.length === 0 ? (
        <div className="text-center py-12">
          <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <p className="text-gray-600">No terms found matching your search.</p>
        </div>
      ) : (
        <div className="space-y-8">
          {letters.map((letter) => (
            <section key={letter} id={`letter-${letter}`}>
              <h2 className="text-2xl font-bold text-primary-600 mb-4 border-b border-gray-200 pb-2">
                {letter}
              </h2>
              <div className="space-y-4">
                {groupedTerms[letter].map((item) => (
                  <div
                    key={item.term}
                    className="bg-white border border-gray-200 rounded-xl p-5"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h3 className="font-semibold text-gray-900 text-lg">
                          {item.term}
                        </h3>
                        <p className="text-gray-600 mt-1">{item.definition}</p>
                        {item.relatedTerms && item.relatedTerms.length > 0 && (
                          <div className="mt-3 flex items-center gap-2">
                            <span className="text-sm text-gray-500">Related:</span>
                            <div className="flex flex-wrap gap-1">
                              {item.relatedTerms.map((related) => (
                                <span
                                  key={related}
                                  className="px-2 py-0.5 bg-gray-100 text-gray-600 rounded text-sm"
                                >
                                  {related}
                                </span>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                      <span className="px-2.5 py-0.5 bg-primary-100 text-primary-700 rounded-full text-xs font-medium whitespace-nowrap">
                        {item.category}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </section>
          ))}
        </div>
      )}

      {/* Stats */}
      <div className="bg-gray-50 rounded-xl p-5 text-center">
        <p className="text-gray-600">
          <span className="font-semibold text-gray-900">{glossaryTerms.length}</span> terms across{' '}
          <span className="font-semibold text-gray-900">{categories.length}</span> categories
        </p>
      </div>
    </div>
  );
}
