import React from 'react';
import { Shield, Lock, Award, CheckCircle } from 'lucide-react';

interface TrustBadge {
  icon: React.ReactNode;
  title: string;
  description: string;
}

const badges: TrustBadge[] = [
  {
    icon: <Shield className="w-8 h-8" />,
    title: 'HIPAA Compliant',
    description: 'Full compliance with healthcare data protection standards',
  },
  {
    icon: <Lock className="w-8 h-8" />,
    title: 'Bank-Level Security',
    description: '256-bit SSL encryption for all data transmissions',
  },
  {
    icon: <Award className="w-8 h-8" />,
    title: 'ISO 27001 Certified',
    description: 'International standard for information security',
  },
  {
    icon: <CheckCircle className="w-8 h-8" />,
    title: '99.9% Uptime',
    description: 'Reliable service with redundant infrastructure',
  },
];

interface TrustBadgesProps {
  className?: string;
}

export const TrustBadges: React.FC<TrustBadgesProps> = ({ className = '' }) => {
  return (
    <div className={`bg-gray-50 py-12 ${className}`}>
      <div className="container mx-auto px-6">
        <div className="text-center mb-8">
          <h3 className="text-2xl font-bold text-gray-900 mb-2">
            Trusted by Healthcare Professionals
          </h3>
          <p className="text-gray-600">
            Enterprise-grade security and compliance you can count on
          </p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {badges.map((badge, index) => (
            <div
              key={index}
              className="bg-white rounded-lg p-6 text-center shadow-sm hover:shadow-md transition-shadow duration-300"
            >
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-blue-100 text-blue-600 mb-4">
                {badge.icon}
              </div>
              <h4 className="font-semibold text-gray-900 mb-2">{badge.title}</h4>
              <p className="text-sm text-gray-600">{badge.description}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
