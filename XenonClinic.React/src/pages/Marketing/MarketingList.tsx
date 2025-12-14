import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  MegaphoneIcon,
  UserGroupIcon,
  ChartBarIcon,
  CurrencyDollarIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  FunnelIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
} from '@heroicons/react/24/outline';
import { campaignApi, leadApi, marketingStatsApi } from '../../lib/api';
import { Modal } from '../../components/ui/Modal';
import { CampaignForm } from '../../components/marketing/CampaignForm';
import { LeadForm } from '../../components/marketing/LeadForm';
import type { Campaign, Lead, MarketingStatistics } from '../../types/marketing';
import { CampaignStatus, CampaignType, LeadStatus, LeadSource } from '../../types/marketing';
import { format } from 'date-fns';

const getCampaignTypeLabel = (type: CampaignType): string => {
  const labels: Record<string, string> = {
    [CampaignType.Email]: 'Email',
    [CampaignType.Social]: 'Social Media',
    [CampaignType.SMS]: 'SMS',
    [CampaignType.Event]: 'Event',
    [CampaignType.Referral]: 'Referral',
    [CampaignType.Recall]: 'Patient Recall',
    [CampaignType.Seasonal]: 'Seasonal',
    [CampaignType.Digital]: 'Digital Ads',
    [CampaignType.Print]: 'Print',
    [CampaignType.Other]: 'Other',
  };
  return labels[type] || type;
};

const getCampaignStatusBadge = (status: CampaignStatus) => {
  const config: Record<string, { className: string; label: string }> = {
    [CampaignStatus.Draft]: { className: 'bg-gray-100 text-gray-800', label: 'Draft' },
    [CampaignStatus.Scheduled]: { className: 'bg-blue-100 text-blue-800', label: 'Scheduled' },
    [CampaignStatus.Active]: { className: 'bg-green-100 text-green-800', label: 'Active' },
    [CampaignStatus.Paused]: { className: 'bg-yellow-100 text-yellow-800', label: 'Paused' },
    [CampaignStatus.Completed]: { className: 'bg-purple-100 text-purple-800', label: 'Completed' },
    [CampaignStatus.Cancelled]: { className: 'bg-red-100 text-red-800', label: 'Cancelled' },
  };
  const c = config[status] || { className: 'bg-gray-100 text-gray-800', label: status };
  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
      {c.label}
    </span>
  );
};

const getLeadStatusBadge = (status: LeadStatus) => {
  const config: Record<string, { className: string; label: string }> = {
    [LeadStatus.New]: { className: 'bg-blue-100 text-blue-800', label: 'New' },
    [LeadStatus.Contacted]: { className: 'bg-purple-100 text-purple-800', label: 'Contacted' },
    [LeadStatus.Qualified]: { className: 'bg-green-100 text-green-800', label: 'Qualified' },
    [LeadStatus.Negotiating]: { className: 'bg-yellow-100 text-yellow-800', label: 'Negotiating' },
    [LeadStatus.Converted]: { className: 'bg-emerald-100 text-emerald-800', label: 'Converted' },
    [LeadStatus.Lost]: { className: 'bg-red-100 text-red-800', label: 'Lost' },
    [LeadStatus.Archived]: { className: 'bg-gray-100 text-gray-800', label: 'Archived' },
  };
  const c = config[status] || { className: 'bg-gray-100 text-gray-800', label: status };
  return (
    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${c.className}`}>
      {c.label}
    </span>
  );
};

const getLeadSourceLabel = (source: LeadSource): string => {
  const labels: Record<string, string> = {
    [LeadSource.Website]: 'Website',
    [LeadSource.Referral]: 'Referral',
    [LeadSource.SocialMedia]: 'Social Media',
    [LeadSource.Advertising]: 'Advertising',
    [LeadSource.Event]: 'Event',
    [LeadSource.WalkIn]: 'Walk-in',
    [LeadSource.Phone]: 'Phone',
    [LeadSource.Email]: 'Email',
    [LeadSource.Partner]: 'Partner',
    [LeadSource.Campaign]: 'Campaign',
    [LeadSource.Other]: 'Other',
  };
  return labels[source] || source;
};

type TabType = 'campaigns' | 'leads';

export const MarketingList = () => {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<TabType>('campaigns');
  const [showNewCampaign, setShowNewCampaign] = useState(false);
  const [showNewLead, setShowNewLead] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');

  // Fetch campaigns
  const { data: campaignsData, isLoading: campaignsLoading } = useQuery({
    queryKey: ['campaigns'],
    queryFn: () => campaignApi.getAll(),
  });

  // Fetch leads
  const { data: leadsData, isLoading: leadsLoading } = useQuery({
    queryKey: ['leads'],
    queryFn: () => leadApi.getAll(),
  });

  // Fetch statistics
  const { data: statsData } = useQuery({
    queryKey: ['marketing-stats'],
    queryFn: () => marketingStatsApi.getDashboard(),
  });

  const campaigns: Campaign[] = campaignsData?.data || [];
  const leads: Lead[] = leadsData?.data || [];
  const stats: MarketingStatistics = statsData?.data || {
    totalCampaigns: 0,
    activeCampaigns: 0,
    campaignsThisMonth: 0,
    totalLeads: 0,
    newLeadsThisMonth: 0,
    newLeadsThisWeek: 0,
    qualifiedLeads: 0,
    convertedLeadsThisMonth: 0,
    activitiesThisWeek: 0,
    activitiesThisMonth: 0,
    scheduledFollowUps: 0,
    overdueFollowUps: 0,
    leadsByStatus: {},
    leadsBySource: {},
    campaignsByType: {},
    activitiesByType: {},
  };

  // Filter campaigns
  const filteredCampaigns = campaigns.filter((campaign) => {
    const matchesSearch =
      !searchTerm ||
      campaign.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      campaign.campaignCode.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || campaign.status === statusFilter;
    const matchesType = !typeFilter || campaign.type === typeFilter;
    return matchesSearch && matchesStatus && matchesType;
  });

  // Filter leads
  const filteredLeads = leads.filter((lead) => {
    const matchesSearch =
      !searchTerm ||
      lead.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      lead.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      lead.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      lead.leadCode.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || lead.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const createCampaignMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => campaignApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['campaigns'] });
      queryClient.invalidateQueries({ queryKey: ['marketing-stats'] });
      setShowNewCampaign(false);
    },
  });

  const createLeadMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => leadApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['marketing-stats'] });
      setShowNewLead(false);
    },
  });

  const handleNewCampaign = (data: unknown) => {
    createCampaignMutation.mutate(data as Record<string, unknown>);
  };

  const handleNewLead = (data: unknown) => {
    createLeadMutation.mutate(data as Record<string, unknown>);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Marketing</h1>
          <p className="text-gray-600">Manage campaigns, leads, and marketing activities</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setShowNewLead(true)}
            className="inline-flex items-center px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-md hover:bg-gray-50"
          >
            <PlusIcon className="h-5 w-5 mr-2" />
            New Lead
          </button>
          <button
            onClick={() => setShowNewCampaign(true)}
            className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
          >
            <PlusIcon className="h-5 w-5 mr-2" />
            New Campaign
          </button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <MegaphoneIcon className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Active Campaigns</p>
              <p className="text-2xl font-bold text-gray-900">{stats.activeCampaigns}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg">
              <UserGroupIcon className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">New Leads This Month</p>
              <p className="text-2xl font-bold text-gray-900">{stats.newLeadsThisMonth}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-purple-100 rounded-lg">
              <ChartBarIcon className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Qualified Leads</p>
              <p className="text-2xl font-bold text-gray-900">{stats.qualifiedLeads}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-4">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg">
              <CurrencyDollarIcon className="h-6 w-6 text-yellow-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm text-gray-500">Conversions This Month</p>
              <p className="text-2xl font-bold text-gray-900">
                {stats.convertedLeadsThisMonth}
                {stats.overdueFollowUps > 0 && (
                  <span className="text-sm text-red-600 ml-2">
                    ({stats.overdueFollowUps} overdue)
                  </span>
                )}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <button
              onClick={() => {
                setActiveTab('campaigns');
                setStatusFilter('');
                setTypeFilter('');
              }}
              className={`px-6 py-3 text-sm font-medium border-b-2 ${
                activeTab === 'campaigns'
                  ? 'border-primary-500 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <MegaphoneIcon className="h-5 w-5 inline-block mr-2" />
              Campaigns ({campaigns.length})
            </button>
            <button
              onClick={() => {
                setActiveTab('leads');
                setStatusFilter('');
                setTypeFilter('');
              }}
              className={`px-6 py-3 text-sm font-medium border-b-2 ${
                activeTab === 'leads'
                  ? 'border-primary-500 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <UserGroupIcon className="h-5 w-5 inline-block mr-2" />
              Leads ({leads.length})
            </button>
          </nav>
        </div>

        {/* Filters */}
        <div className="p-4 border-b border-gray-200">
          <div className="flex flex-wrap gap-4">
            <div className="flex-1 min-w-[200px]">
              <div className="relative">
                <MagnifyingGlassIcon className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" />
                <input
                  type="text"
                  placeholder={activeTab === 'campaigns' ? 'Search campaigns...' : 'Search leads...'}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-primary-500 focus:border-primary-500"
                />
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <FunnelIcon className="h-5 w-5 text-gray-400" />
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="px-3 py-2 border border-gray-300 rounded-md"
              >
                <option value="">All Status</option>
                {activeTab === 'campaigns' ? (
                  <>
                    {Object.entries(CampaignStatus).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </>
                ) : (
                  <>
                    {Object.entries(LeadStatus).map(([key, value]) => (
                      <option key={value} value={value}>
                        {key}
                      </option>
                    ))}
                  </>
                )}
              </select>
              {activeTab === 'campaigns' && (
                <select
                  value={typeFilter}
                  onChange={(e) => setTypeFilter(e.target.value)}
                  className="px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value="">All Types</option>
                  {Object.entries(CampaignType).map(([key, value]) => (
                    <option key={value} value={value}>
                      {getCampaignTypeLabel(value)}
                    </option>
                  ))}
                </select>
              )}
            </div>
          </div>
        </div>

        {/* Campaigns Table */}
        {activeTab === 'campaigns' && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Campaign
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Period
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Leads
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Conversions
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    ROI
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {campaignsLoading ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                      Loading campaigns...
                    </td>
                  </tr>
                ) : filteredCampaigns.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                      No campaigns found
                    </td>
                  </tr>
                ) : (
                  filteredCampaigns.map((campaign) => (
                    <tr key={campaign.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="font-medium text-gray-900">{campaign.name}</div>
                        <div className="text-sm text-gray-500">{campaign.campaignCode}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {getCampaignTypeLabel(campaign.type)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        <div>{format(new Date(campaign.startDate), 'MMM d, yyyy')}</div>
                        {campaign.endDate && (
                          <div className="text-gray-500">
                            to {format(new Date(campaign.endDate), 'MMM d, yyyy')}
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getCampaignStatusBadge(campaign.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {campaign.leadsGenerated}
                        {campaign.targetLeads && (
                          <span className="text-gray-500">/{campaign.targetLeads}</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {campaign.conversions}
                        {campaign.conversionRate !== undefined && campaign.conversionRate > 0 && (
                          <span className="text-green-600 ml-1">
                            ({campaign.conversionRate.toFixed(1)}%)
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm">
                        {campaign.roi !== undefined && campaign.roi !== null ? (
                          <span
                            className={`flex items-center ${
                              campaign.roi >= 0 ? 'text-green-600' : 'text-red-600'
                            }`}
                          >
                            {campaign.roi >= 0 ? (
                              <ArrowTrendingUpIcon className="h-4 w-4 mr-1" />
                            ) : (
                              <ArrowTrendingDownIcon className="h-4 w-4 mr-1" />
                            )}
                            {campaign.roi.toFixed(1)}%
                          </span>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button className="text-primary-600 hover:text-primary-900 mr-3">
                          View
                        </button>
                        <button className="text-gray-600 hover:text-gray-900">Edit</button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Leads Table */}
        {activeTab === 'leads' && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Lead
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Contact
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Source
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Score
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Follow-Up
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Assigned To
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {leadsLoading ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                      Loading leads...
                    </td>
                  </tr>
                ) : filteredLeads.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-gray-500">
                      No leads found
                    </td>
                  </tr>
                ) : (
                  filteredLeads.map((lead) => (
                    <tr key={lead.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="font-medium text-gray-900">
                          {lead.firstName} {lead.lastName}
                        </div>
                        <div className="text-sm text-gray-500">{lead.leadCode}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">{lead.email || '-'}</div>
                        <div className="text-sm text-gray-500">{lead.phoneNumber || '-'}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {getLeadSourceLabel(lead.source)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getLeadStatusBadge(lead.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm">
                        {lead.leadScore !== undefined && lead.leadScore !== null ? (
                          <div className="flex items-center">
                            <div
                              className={`w-8 h-8 rounded-full flex items-center justify-center text-white font-medium ${
                                lead.leadScore >= 80
                                  ? 'bg-green-500'
                                  : lead.leadScore >= 50
                                  ? 'bg-yellow-500'
                                  : 'bg-gray-400'
                              }`}
                            >
                              {lead.leadScore}
                            </div>
                          </div>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm">
                        {lead.nextFollowUpDate ? (
                          <div
                            className={
                              new Date(lead.nextFollowUpDate) < new Date()
                                ? 'text-red-600'
                                : 'text-gray-900'
                            }
                          >
                            {format(new Date(lead.nextFollowUpDate), 'MMM d, yyyy')}
                          </div>
                        ) : (
                          <span className="text-gray-400">Not scheduled</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {lead.assignedToUserName || (
                          <span className="text-gray-400">Unassigned</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button className="text-primary-600 hover:text-primary-900 mr-3">
                          View
                        </button>
                        <button className="text-green-600 hover:text-green-900">Convert</button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* New Campaign Modal */}
      <Modal
        isOpen={showNewCampaign}
        onClose={() => setShowNewCampaign(false)}
        title="New Campaign"
        size="xl"
      >
        <CampaignForm
          onSubmit={handleNewCampaign}
          onCancel={() => setShowNewCampaign(false)}
        />
      </Modal>

      {/* New Lead Modal */}
      <Modal
        isOpen={showNewLead}
        onClose={() => setShowNewLead(false)}
        title="New Lead"
        size="xl"
      >
        <LeadForm
          onSubmit={handleNewLead}
          onCancel={() => setShowNewLead(false)}
        />
      </Modal>
    </div>
  );
};
