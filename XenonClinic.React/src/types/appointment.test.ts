import { describe, it, expect } from 'vitest';
import { AppointmentStatus, AppointmentType } from './appointment';
import type { Appointment, CreateAppointmentRequest, UpdateAppointmentRequest, AppointmentStatistics, AvailabilitySlot } from './appointment';

describe('Appointment Types', () => {
  describe('AppointmentStatus', () => {
    it('has correct Booked value', () => {
      expect(AppointmentStatus.Booked).toBe(0);
    });

    it('has correct Confirmed value', () => {
      expect(AppointmentStatus.Confirmed).toBe(1);
    });

    it('has correct CheckedIn value', () => {
      expect(AppointmentStatus.CheckedIn).toBe(2);
    });

    it('has correct Completed value', () => {
      expect(AppointmentStatus.Completed).toBe(3);
    });

    it('has correct Cancelled value', () => {
      expect(AppointmentStatus.Cancelled).toBe(4);
    });

    it('has correct NoShow value', () => {
      expect(AppointmentStatus.NoShow).toBe(5);
    });

    it('has all 6 status types', () => {
      const statusKeys = Object.keys(AppointmentStatus);
      expect(statusKeys.length).toBe(6);
    });
  });

  describe('AppointmentType', () => {
    it('has correct Consultation value', () => {
      expect(AppointmentType.Consultation).toBe(0);
    });

    it('has correct FollowUp value', () => {
      expect(AppointmentType.FollowUp).toBe(1);
    });

    it('has correct Procedure value', () => {
      expect(AppointmentType.Procedure).toBe(2);
    });

    it('has correct Emergency value', () => {
      expect(AppointmentType.Emergency).toBe(3);
    });

    it('has all 4 appointment types', () => {
      const typeKeys = Object.keys(AppointmentType);
      expect(typeKeys.length).toBe(4);
    });
  });

  describe('Appointment Interface', () => {
    it('can create a valid appointment object', () => {
      const appointment: Appointment = {
        id: 1,
        patientId: 100,
        branchId: 1,
        startTime: '2024-06-15T09:00:00Z',
        endTime: '2024-06-15T09:30:00Z',
        type: AppointmentType.Consultation,
        status: AppointmentStatus.Booked,
      };

      expect(appointment.id).toBe(1);
      expect(appointment.patientId).toBe(100);
      expect(appointment.type).toBe(AppointmentType.Consultation);
      expect(appointment.status).toBe(AppointmentStatus.Booked);
    });

    it('can create appointment with optional fields', () => {
      const appointment: Appointment = {
        id: 2,
        patientId: 101,
        branchId: 1,
        providerId: 50,
        startTime: '2024-06-15T10:00:00Z',
        endTime: '2024-06-15T10:30:00Z',
        type: AppointmentType.FollowUp,
        status: AppointmentStatus.Confirmed,
        notes: 'Follow-up consultation',
        patient: {
          id: 101,
          fullNameEn: 'John Doe',
          phoneNumber: '+971501234567',
          email: 'john@example.com',
        },
        provider: {
          id: 50,
          fullName: 'Dr. Smith',
        },
        branch: {
          id: 1,
          name: 'Main Branch',
        },
      };

      expect(appointment.providerId).toBe(50);
      expect(appointment.notes).toBe('Follow-up consultation');
      expect(appointment.patient?.fullNameEn).toBe('John Doe');
      expect(appointment.provider?.fullName).toBe('Dr. Smith');
      expect(appointment.branch?.name).toBe('Main Branch');
    });
  });

  describe('CreateAppointmentRequest Interface', () => {
    it('can create a valid create request', () => {
      const request: CreateAppointmentRequest = {
        patientId: 100,
        startTime: '2024-06-15T09:00:00Z',
        endTime: '2024-06-15T09:30:00Z',
        type: AppointmentType.Consultation,
      };

      expect(request.patientId).toBe(100);
      expect(request.type).toBe(AppointmentType.Consultation);
    });

    it('can include optional fields in create request', () => {
      const request: CreateAppointmentRequest = {
        patientId: 100,
        providerId: 50,
        startTime: '2024-06-15T09:00:00Z',
        endTime: '2024-06-15T09:30:00Z',
        type: AppointmentType.Procedure,
        notes: 'Minor procedure',
      };

      expect(request.providerId).toBe(50);
      expect(request.notes).toBe('Minor procedure');
    });
  });

  describe('UpdateAppointmentRequest Interface', () => {
    it('can create a valid update request', () => {
      const request: UpdateAppointmentRequest = {
        id: 1,
        patientId: 100,
        startTime: '2024-06-15T09:00:00Z',
        endTime: '2024-06-15T09:30:00Z',
        type: AppointmentType.Consultation,
        status: AppointmentStatus.Confirmed,
      };

      expect(request.id).toBe(1);
      expect(request.status).toBe(AppointmentStatus.Confirmed);
    });
  });

  describe('AppointmentStatistics Interface', () => {
    it('can create valid statistics object', () => {
      const stats: AppointmentStatistics = {
        total: 100,
        today: 10,
        upcoming: 25,
        completed: 60,
        cancelled: 5,
        noShow: 5,
        statusDistribution: {
          [AppointmentStatus.Booked]: 10,
          [AppointmentStatus.Confirmed]: 15,
          [AppointmentStatus.CheckedIn]: 5,
          [AppointmentStatus.Completed]: 60,
          [AppointmentStatus.Cancelled]: 5,
          [AppointmentStatus.NoShow]: 5,
        },
        typeDistribution: {
          [AppointmentType.Consultation]: 50,
          [AppointmentType.FollowUp]: 30,
          [AppointmentType.Procedure]: 15,
          [AppointmentType.Emergency]: 5,
        },
        completionRate: 0.6,
      };

      expect(stats.total).toBe(100);
      expect(stats.completionRate).toBe(0.6);
      expect(stats.statusDistribution[AppointmentStatus.Completed]).toBe(60);
    });
  });

  describe('AvailabilitySlot Interface', () => {
    it('can create valid availability slot', () => {
      const slot: AvailabilitySlot = {
        startTime: '2024-06-15T09:00:00Z',
        endTime: '2024-06-15T09:30:00Z',
        isAvailable: true,
      };

      expect(slot.startTime).toBe('2024-06-15T09:00:00Z');
      expect(slot.isAvailable).toBe(true);
    });

    it('can create unavailable slot', () => {
      const slot: AvailabilitySlot = {
        startTime: '2024-06-15T10:00:00Z',
        endTime: '2024-06-15T10:30:00Z',
        isAvailable: false,
      };

      expect(slot.isAvailable).toBe(false);
    });
  });
});
