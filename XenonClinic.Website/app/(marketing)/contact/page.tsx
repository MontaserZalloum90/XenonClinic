'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Mail,
  Phone,
  MapPin,
  Clock,
  MessageSquare,
  Send,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';
import { submitContactInquiry } from '@/lib/platform-api';

const contactInfo = [
  {
    icon: Mail,
    title: 'Email',
    value: 'hello@xenon.ae',
    link: 'mailto:hello@xenon.ae',
  },
  {
    icon: Phone,
    title: 'Phone',
    value: '+971 4 234 5678',
    link: 'tel:+97142345678',
  },
  {
    icon: MapPin,
    title: 'Address',
    value: 'Dubai Internet City, Building 12, Dubai, UAE',
    link: null,
  },
  {
    icon: Clock,
    title: 'Business Hours',
    value: 'Sun-Thu: 9am-6pm GST',
    link: null,
  },
];

const inquiryTypes = [
  { value: 'sales', label: 'Sales Inquiry' },
  { value: 'support', label: 'Technical Support' },
  { value: 'partnership', label: 'Partnership' },
  { value: 'press', label: 'Press & Media' },
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
    setFormState((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
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
      setError('Network error. Please check your connection and try again.');
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
            <h1 className="heading-1 text-gray-900 mb-6">
              Get in touch
            </h1>
            <p className="text-lg md:text-xl text-gray-600">
              Have questions about XENON? We'd love to hear from you.
              Our team is ready to help.
            </p>
          </div>
        </div>
      </section>

      {/* Contact Info + Form */}
      <section className="section-padding bg-white">
        <div className="container-marketing">
          <div className="grid lg:grid-cols-3 gap-12">
            {/* Contact Information */}
            <div>
              <h2 className="text-xl font-bold text-gray-900 mb-6">
                Contact Information
              </h2>
              <div className="space-y-6">
                {contactInfo.map((info) => (
                  <div key={info.title} className="flex gap-4">
                    <div className="h-10 w-10 rounded-lg bg-primary-100 text-primary-600 flex items-center justify-center flex-shrink-0">
                      <info.icon className="h-5 w-5" />
                    </div>
                    <div>
                      <div className="text-sm text-gray-500">{info.title}</div>
                      {info.link ? (
                        <a
                          href={info.link}
                          className="font-medium text-gray-900 hover:text-primary-600 transition-colors"
                        >
                          {info.value}
                        </a>
                      ) : (
                        <div className="font-medium text-gray-900">{info.value}</div>
                      )}
                    </div>
                  </div>
                ))}
              </div>

              {/* Quick Links */}
              <div className="mt-10">
                <h3 className="font-semibold text-gray-900 mb-4">Quick Links</h3>
                <ul className="space-y-3">
                  <li>
                    <Link href="/demo" className="link text-sm">
                      Start a free trial
                    </Link>
                  </li>
                  <li>
                    <Link href="/docs" className="link text-sm">
                      View documentation
                    </Link>
                  </li>
                  <li>
                    <Link href="/pricing" className="link text-sm">
                      See pricing plans
                    </Link>
                  </li>
                  <li>
                    <Link href="/compliance" className="link text-sm">
                      Security & compliance
                    </Link>
                  </li>
                </ul>
              </div>
            </div>

            {/* Contact Form */}
            <div className="lg:col-span-2">
              <div className="card">
                {isSubmitted ? (
                  <div className="text-center py-12">
                    <div className="h-16 w-16 rounded-full bg-green-100 text-green-600 flex items-center justify-center mx-auto mb-6">
                      <CheckCircle className="h-8 w-8" />
                    </div>
                    <h3 className="text-xl font-bold text-gray-900 mb-2">
                      Message Sent!
                    </h3>
                    <p className="text-gray-600 mb-6">
                      Thank you for reaching out. Our team will get back to you within 24 hours.
                    </p>
                    <button
                      onClick={() => {
                        setIsSubmitted(false);
                        setFormState({
                          name: '',
                          email: '',
                          company: '',
                          phone: '',
                          inquiryType: 'sales',
                          message: '',
                        });
                      }}
                      className="btn-secondary"
                    >
                      Send Another Message
                    </button>
                  </div>
                ) : (
                  <form onSubmit={handleSubmit} className="space-y-6">
                    {/* Error message */}
                    {error && (
                      <div className="p-3 rounded-lg bg-red-50 border border-red-200 flex items-center gap-2 text-sm text-red-700">
                        <AlertCircle className="h-4 w-4 flex-shrink-0" />
                        {error}
                      </div>
                    )}

                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                          Full Name *
                        </label>
                        <input
                          type="text"
                          id="name"
                          name="name"
                          required
                          value={formState.name}
                          onChange={handleChange}
                          className="input"
                          placeholder="Your name"
                        />
                      </div>
                      <div>
                        <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                          Email Address *
                        </label>
                        <input
                          type="email"
                          id="email"
                          name="email"
                          required
                          value={formState.email}
                          onChange={handleChange}
                          className="input"
                          placeholder="you@company.com"
                        />
                      </div>
                    </div>

                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label htmlFor="company" className="block text-sm font-medium text-gray-700 mb-1">
                          Company Name
                        </label>
                        <input
                          type="text"
                          id="company"
                          name="company"
                          value={formState.company}
                          onChange={handleChange}
                          className="input"
                          placeholder="Your company"
                        />
                      </div>
                      <div>
                        <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-1">
                          Phone Number
                        </label>
                        <input
                          type="tel"
                          id="phone"
                          name="phone"
                          value={formState.phone}
                          onChange={handleChange}
                          className="input"
                          placeholder="+971 50 123 4567"
                        />
                      </div>
                    </div>

                    <div>
                      <label htmlFor="inquiryType" className="block text-sm font-medium text-gray-700 mb-1">
                        Inquiry Type *
                      </label>
                      <select
                        id="inquiryType"
                        name="inquiryType"
                        required
                        value={formState.inquiryType}
                        onChange={handleChange}
                        className="input"
                      >
                        {inquiryTypes.map((type) => (
                          <option key={type.value} value={type.value}>
                            {type.label}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div>
                      <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">
                        Message *
                      </label>
                      <textarea
                        id="message"
                        name="message"
                        required
                        rows={5}
                        value={formState.message}
                        onChange={handleChange}
                        className="input resize-none"
                        placeholder="How can we help you?"
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <p className="text-sm text-gray-500">
                        We'll respond within 24 hours
                      </p>
                      <button
                        type="submit"
                        disabled={isSubmitting}
                        className="btn-primary"
                      >
                        {isSubmitting ? (
                          <>
                            <span className="animate-spin mr-2">
                              <svg className="h-4 w-4" viewBox="0 0 24 24">
                                <circle
                                  className="opacity-25"
                                  cx="12"
                                  cy="12"
                                  r="10"
                                  stroke="currentColor"
                                  strokeWidth="4"
                                  fill="none"
                                />
                                <path
                                  className="opacity-75"
                                  fill="currentColor"
                                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                                />
                              </svg>
                            </span>
                            Sending...
                          </>
                        ) : (
                          <>
                            Send Message
                            <Send className="ml-2 h-4 w-4" />
                          </>
                        )}
                      </button>
                    </div>
                  </form>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Live Chat CTA */}
      <section className="section-padding bg-gray-50">
        <div className="container-marketing text-center">
          <div className="h-16 w-16 rounded-2xl bg-primary-100 text-primary-600 flex items-center justify-center mx-auto mb-6">
            <MessageSquare className="h-8 w-8" />
          </div>
          <h2 className="heading-3 text-gray-900 mb-4">
            Need immediate help?
          </h2>
          <p className="text-gray-600 mb-6 max-w-xl mx-auto">
            Our support team is available via live chat during business hours.
            Average response time: under 5 minutes.
          </p>
          <button className="btn-primary btn-lg">
            Start Live Chat
          </button>
        </div>
      </section>
    </>
  );
}
