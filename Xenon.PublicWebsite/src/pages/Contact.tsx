import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Mail, Phone, MapPin, Clock, Send, CheckCircle, AlertCircle } from 'lucide-react';
import { brand } from '@/lib/brand';
import { submitContactInquiry } from '@/lib/platform-api';

const contactInfo = [
  { icon: Mail, title: 'Email', value: brand.contact.email, link: `mailto:${brand.contact.email}` },
  { icon: Phone, title: 'Phone', value: brand.contact.phone, link: `tel:${brand.contact.phone.replace(/\s/g, '')}` },
  { icon: MapPin, title: 'Address', value: brand.legal.address, link: null },
  { icon: Clock, title: 'Business Hours', value: 'Sun-Thu: 9am-6pm GST', link: null },
];

const inquiryTypes = [
  { value: 'sales', label: 'Sales Inquiry' },
  { value: 'support', label: 'Technical Support' },
  { value: 'partnership', label: 'Partnership' },
  { value: 'other', label: 'Other' },
];

export default function ContactPage() {
  const [formState, setFormState] = useState({
    name: '',
    email: '',
    company: '',
    phone: '',
    inquiryType: 'sales',
    message: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    setFormState((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError('');

    try {
      const response = await submitContactInquiry({
        name: formState.name,
        email: formState.email,
        company: formState.company || undefined,
        phone: formState.phone || undefined,
        inquiryType: formState.inquiryType,
        message: formState.message,
      });

      if (response.success) {
        setIsSubmitted(true);
      } else {
        setError(response.error || 'Failed to submit. Please try again.');
      }
    } catch {
      setError('Network error. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      {/* Hero */}
      <section className="relative py-16 md:py-24 bg-gradient-to-b from-gray-50 to-white">
        <div className="container-marketing">
          <div className="max-w-3xl mx-auto text-center">
            <div className="badge-primary mb-4">Contact</div>
            <h1 className="heading-1 text-gray-900 mb-6">Get in touch</h1>
            <p className="text-lg md:text-xl text-gray-600">
              Have questions about {brand.name}? We'd love to hear from you.
            </p>
          </div>
        </div>
      </section>

      {/* Contact Form */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-3 gap-12">
            {/* Contact Info */}
            <div>
              <h2 className="text-xl font-bold text-gray-900 mb-6">Contact Information</h2>
              <div className="space-y-6">
                {contactInfo.map((info) => (
                  <div key={info.title} className="flex gap-4">
                    <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                      <info.icon className="h-5 w-5" />
                    </div>
                    <div>
                      <div className="text-sm text-gray-500">{info.title}</div>
                      {info.link ? (
                        <a href={info.link} className="font-medium text-gray-900 hover:text-primary-600">
                          {info.value}
                        </a>
                      ) : (
                        <div className="font-medium text-gray-900">{info.value}</div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-10">
                <h3 className="font-semibold text-gray-900 mb-4">Quick Links</h3>
                <ul className="space-y-3">
                  <li><Link to="/demo" className="link text-sm">Start a free trial</Link></li>
                  <li><Link to="/pricing" className="link text-sm">See pricing plans</Link></li>
                </ul>
              </div>
            </div>

            {/* Form */}
            <div className="lg:col-span-2">
              <div className="card">
                {isSubmitted ? (
                  <div className="text-center py-12">
                    <div className="h-16 w-16 rounded-full bg-green-100 text-green-600 flex items-center justify-center mx-auto mb-6">
                      <CheckCircle className="h-8 w-8" />
                    </div>
                    <h3 className="text-xl font-bold text-gray-900 mb-2">Message Sent!</h3>
                    <p className="text-gray-600 mb-6">We'll get back to you within 24 hours.</p>
                    <button
                      onClick={() => {
                        setIsSubmitted(false);
                        setFormState({ name: '', email: '', company: '', phone: '', inquiryType: 'sales', message: '' });
                      }}
                      className="btn-secondary"
                    >
                      Send Another Message
                    </button>
                  </div>
                ) : (
                  <form onSubmit={handleSubmit} className="space-y-6">
                    {error && (
                      <div className="p-3 rounded-lg bg-red-50 border border-red-200 flex items-center gap-2 text-sm text-red-700">
                        <AlertCircle className="h-4 w-4" />
                        {error}
                      </div>
                    )}
                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Full Name *</label>
                        <input type="text" name="name" required value={formState.name} onChange={handleChange} className="input" placeholder="Your name" />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
                        <input type="email" name="email" required value={formState.email} onChange={handleChange} className="input" placeholder="you@company.com" />
                      </div>
                    </div>
                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Company</label>
                        <input type="text" name="company" value={formState.company} onChange={handleChange} className="input" placeholder="Your company" />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                        <input type="tel" name="phone" value={formState.phone} onChange={handleChange} className="input" placeholder="+971 50 123 4567" />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Inquiry Type *</label>
                      <select name="inquiryType" required value={formState.inquiryType} onChange={handleChange} className="input">
                        {inquiryTypes.map((type) => (
                          <option key={type.value} value={type.value}>{type.label}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Message *</label>
                      <textarea name="message" required rows={5} value={formState.message} onChange={handleChange} className="input resize-none" placeholder="How can we help?" />
                    </div>
                    <div className="flex items-center justify-between">
                      <p className="text-sm text-gray-500">We'll respond within 24 hours</p>
                      <button type="submit" disabled={isSubmitting} className="btn-primary">
                        {isSubmitting ? 'Sending...' : <>Send Message <Send className="ml-2 h-4 w-4" /></>}
                      </button>
                    </div>
                  </form>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}
