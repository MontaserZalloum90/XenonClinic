// Marketing Types for campaigns, leads, and marketing activities

// ============================================
// CAMPAIGN TYPES
// ============================================

export const CampaignStatus = {
  Draft: 'draft',
  Scheduled: 'scheduled',
  Active: 'active',
  Paused: 'paused',
  Completed: 'completed',
  Cancelled: 'cancelled',
} as const;

export type CampaignStatus = typeof CampaignStatus[keyof typeof CampaignStatus];

export const CampaignType = {
  Email: 'email',
  Social: 'social',
  SMS: 'sms',
  Event: 'event',
  Referral: 'referral',
  Recall: 'recall',
  Seasonal: 'seasonal',
  Digital: 'digital',
  Print: 'print',
  Other: 'other',
} as const;

export type CampaignType = typeof CampaignType[keyof typeof CampaignType];

export interface Campaign {
  id: number;
  campaignCode: string;
  name: string;
  description?: string;
  type: CampaignType;
  status: CampaignStatus;

  // Campaign period
  startDate: string;
  endDate?: string;

  // Budget
  budget?: number;
  actualSpend?: number;

  // Target metrics
  targetLeads?: number;
  targetConversions?: number;

  // Actual metrics
  leadsGenerated: number;
  conversions: number;
  revenue?: number;

  // Target audience
  targetAudience?: string;
  tags?: string;

  // Content
  subject?: string;
  content?: string;
  callToAction?: string;

  // Branch
  branchId: number;

  // Assignment
  assignedToUserId?: string;
  assignedToUserName?: string;

  // Audit
  createdBy: string;
  createdAt: string;
  lastModifiedBy?: string;
  lastModifiedAt?: string;

  // Computed
  roi?: number;
  conversionRate?: number;
  costPerLead?: number;
  isActive?: boolean;

  // Related
  leads?: Lead[];
  activities?: MarketingActivity[];
}

export interface CreateCampaignRequest {
  name: string;
  description?: string;
  type: CampaignType;
  startDate: string;
  endDate?: string;
  budget?: number;
  targetLeads?: number;
  targetConversions?: number;
  targetAudience?: string;
  tags?: string;
  subject?: string;
  content?: string;
  callToAction?: string;
  assignedToUserId?: string;
}

export interface UpdateCampaignRequest extends CreateCampaignRequest {
  id: number;
  status?: CampaignStatus;
  actualSpend?: number;
  leadsGenerated?: number;
  conversions?: number;
  revenue?: number;
}

// ============================================
// LEAD TYPES
// ============================================

export const LeadStatus = {
  New: 'new',
  Contacted: 'contacted',
  Qualified: 'qualified',
  Negotiating: 'negotiating',
  Converted: 'converted',
  Lost: 'lost',
  Archived: 'archived',
} as const;

export type LeadStatus = typeof LeadStatus[keyof typeof LeadStatus];

export const LeadSource = {
  Website: 'website',
  Referral: 'referral',
  SocialMedia: 'social_media',
  Advertising: 'advertising',
  Event: 'event',
  WalkIn: 'walk_in',
  Phone: 'phone',
  Email: 'email',
  Partner: 'partner',
  Campaign: 'campaign',
  Other: 'other',
} as const;

export type LeadSource = typeof LeadSource[keyof typeof LeadSource];

export const LeadPriority = {
  Low: 'low',
  Medium: 'medium',
  High: 'high',
  Urgent: 'urgent',
} as const;

export type LeadPriority = typeof LeadPriority[keyof typeof LeadPriority];

export interface Lead {
  id: number;
  leadCode: string;

  // Contact Information
  firstName: string;
  lastName: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  mobileNumber?: string;

  // Company/Organization
  companyName?: string;
  jobTitle?: string;

  // Address
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;

  // Lead Details
  status: LeadStatus;
  source: LeadSource;
  sourceDetails?: string;

  // Interest and scoring
  interestedIn?: string;
  leadScore?: number;
  priority?: LeadPriority;

  // Campaign association
  campaignId?: number;
  campaignName?: string;

  // Branch
  branchId: number;

  // Assignment
  assignedToUserId?: string;
  assignedToUserName?: string;

  // Follow-up
  nextFollowUpDate?: string;
  nextFollowUpNotes?: string;
  lastContactedDate?: string;
  contactAttempts: number;

  // Conversion
  convertedDate?: string;
  convertedToPatientId?: number;
  estimatedValue?: number;
  actualValue?: number;

  // Lost lead info
  lostReason?: string;

  // Notes and tags
  notes?: string;
  tags?: string;

  // Audit
  createdBy: string;
  createdAt: string;
  lastModifiedBy?: string;
  lastModifiedAt?: string;

  // Computed
  isConverted?: boolean;
  needsFollowUp?: boolean;
  daysSinceLastContact?: number;

  // Related
  activities?: MarketingActivity[];
}

export interface CreateLeadRequest {
  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber?: string;
  mobileNumber?: string;
  companyName?: string;
  jobTitle?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  postalCode?: string;
  source: LeadSource;
  sourceDetails?: string;
  interestedIn?: string;
  priority?: LeadPriority;
  campaignId?: number;
  assignedToUserId?: string;
  nextFollowUpDate?: string;
  nextFollowUpNotes?: string;
  estimatedValue?: number;
  notes?: string;
  tags?: string;
}

export interface UpdateLeadRequest extends CreateLeadRequest {
  id: number;
  status?: LeadStatus;
  leadScore?: number;
  lastContactedDate?: string;
  contactAttempts?: number;
  convertedDate?: string;
  convertedToPatientId?: number;
  actualValue?: number;
  lostReason?: string;
}

// ============================================
// MARKETING ACTIVITY TYPES
// ============================================

export const ActivityType = {
  Call: 'call',
  Email: 'email',
  Meeting: 'meeting',
  Demo: 'demo',
  FollowUp: 'follow_up',
  Presentation: 'presentation',
  Proposal: 'proposal',
  Negotiation: 'negotiation',
  Visit: 'visit',
  Task: 'task',
  Note: 'note',
  Other: 'other',
} as const;

export type ActivityType = typeof ActivityType[keyof typeof ActivityType];

export const ActivityStatus = {
  Scheduled: 'scheduled',
  InProgress: 'in_progress',
  Completed: 'completed',
  Cancelled: 'cancelled',
} as const;

export type ActivityStatus = typeof ActivityStatus[keyof typeof ActivityStatus];

export interface MarketingActivity {
  id: number;
  activityType: ActivityType;
  subject: string;
  description?: string;

  // Activity timing
  activityDate: string;
  durationMinutes?: number;

  // Status
  status: ActivityStatus;
  outcome?: string;

  // Associated entities
  leadId?: number;
  leadName?: string;
  campaignId?: number;
  campaignName?: string;
  patientId?: number;
  patientName?: string;

  // Branch
  branchId: number;

  // User who performed the activity
  performedByUserId?: string;
  performedByUserName?: string;

  // Contact details
  contactMethod?: string;
  contactedPerson?: string;
  phoneNumber?: string;
  email?: string;

  // Next steps
  nextSteps?: string;
  followUpDate?: string;

  // Notes
  notes?: string;
  internalNotes?: string;

  // Audit
  createdBy: string;
  createdAt: string;
  lastModifiedBy?: string;
  lastModifiedAt?: string;
}

export interface CreateActivityRequest {
  activityType: ActivityType;
  subject: string;
  description?: string;
  activityDate: string;
  durationMinutes?: number;
  leadId?: number;
  campaignId?: number;
  patientId?: number;
  contactMethod?: string;
  contactedPerson?: string;
  phoneNumber?: string;
  email?: string;
  nextSteps?: string;
  followUpDate?: string;
  notes?: string;
  internalNotes?: string;
}

export interface UpdateActivityRequest extends CreateActivityRequest {
  id: number;
  status?: ActivityStatus;
  outcome?: string;
}

// ============================================
// STATISTICS
// ============================================

export interface MarketingStatistics {
  // Campaign stats
  totalCampaigns: number;
  activeCampaigns: number;
  campaignsThisMonth: number;

  // Lead stats
  totalLeads: number;
  newLeadsThisMonth: number;
  newLeadsThisWeek: number;
  qualifiedLeads: number;
  convertedLeadsThisMonth: number;

  // Activity stats
  activitiesThisWeek: number;
  activitiesThisMonth: number;
  scheduledFollowUps: number;
  overdueFollowUps: number;

  // Financial
  campaignBudgetTotal?: number;
  campaignSpendTotal?: number;
  revenueGenerated?: number;

  // Rates
  overallConversionRate?: number;
  averageLeadScore?: number;

  // Distributions
  leadsByStatus: Record<string, number>;
  leadsBySource: Record<string, number>;
  campaignsByType: Record<string, number>;
  activitiesByType: Record<string, number>;
}

// ============================================
// DASHBOARD TYPES
// ============================================

export interface CampaignPerformance {
  campaignId: number;
  campaignName: string;
  type: CampaignType;
  leadsGenerated: number;
  conversions: number;
  conversionRate: number;
  spend: number;
  revenue: number;
  roi: number;
}

export interface LeadFunnel {
  stage: LeadStatus;
  count: number;
  value: number;
}

export interface LeadTrend {
  date: string;
  newLeads: number;
  conversions: number;
}
