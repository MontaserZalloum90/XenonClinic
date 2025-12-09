import { describe, it, expect } from 'vitest';
import { hasRole, Roles } from './ProtectedRoute';

describe('hasRole helper function', () => {
  describe('when no roles are required', () => {
    it('returns true with empty required roles', () => {
      expect(hasRole(['Admin'], [])).toBe(true);
    });

    it('returns true with empty user roles and empty required roles', () => {
      expect(hasRole([], [])).toBe(true);
    });
  });

  describe('when checking for any role (default behavior)', () => {
    it('returns true if user has one of the required roles', () => {
      expect(hasRole(['Admin'], ['Admin', 'Doctor'])).toBe(true);
    });

    it('returns true if user has multiple required roles', () => {
      expect(hasRole(['Admin', 'Doctor'], ['Admin', 'Nurse'])).toBe(true);
    });

    it('returns false if user has none of the required roles', () => {
      expect(hasRole(['Nurse'], ['Admin', 'Doctor'])).toBe(false);
    });

    it('returns false if user has no roles', () => {
      expect(hasRole([], ['Admin'])).toBe(false);
    });
  });

  describe('when requiring all roles', () => {
    it('returns true if user has all required roles', () => {
      expect(hasRole(['Admin', 'Doctor'], ['Admin', 'Doctor'], true)).toBe(true);
    });

    it('returns true if user has more than required roles', () => {
      expect(hasRole(['Admin', 'Doctor', 'Nurse'], ['Admin', 'Doctor'], true)).toBe(true);
    });

    it('returns false if user is missing one required role', () => {
      expect(hasRole(['Admin'], ['Admin', 'Doctor'], true)).toBe(false);
    });

    it('returns false if user has no roles but roles are required', () => {
      expect(hasRole([], ['Admin'], true)).toBe(false);
    });
  });
});

describe('Roles constants', () => {
  it('has ADMIN role', () => {
    expect(Roles.ADMIN).toBe('Admin');
  });

  it('has DOCTOR role', () => {
    expect(Roles.DOCTOR).toBe('Doctor');
  });

  it('has NURSE role', () => {
    expect(Roles.NURSE).toBe('Nurse');
  });

  it('has RECEPTIONIST role', () => {
    expect(Roles.RECEPTIONIST).toBe('Receptionist');
  });

  it('has LAB_TECHNICIAN role', () => {
    expect(Roles.LAB_TECHNICIAN).toBe('LabTechnician');
  });

  it('has PHARMACIST role', () => {
    expect(Roles.PHARMACIST).toBe('Pharmacist');
  });

  it('has RADIOLOGIST role', () => {
    expect(Roles.RADIOLOGIST).toBe('Radiologist');
  });

  it('has HR_MANAGER role', () => {
    expect(Roles.HR_MANAGER).toBe('HRManager');
  });

  it('has ACCOUNTANT role', () => {
    expect(Roles.ACCOUNTANT).toBe('Accountant');
  });

  it('has 9 total roles defined', () => {
    expect(Object.keys(Roles).length).toBe(9);
  });
});
