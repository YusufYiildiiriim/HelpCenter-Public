import { describe, it, expect } from 'vitest';
import { hasPermission } from '@/lib/permissions';

describe('hasPermission', () => {
  const permissions = {
    modules: [
      { resourceKey: 'Users', actions: ['Read', 'Update'] },
      { resourceKey: 'Roles', actions: ['Read'] },
    ],
  };

  it('grants when action present', () => {
    expect(hasPermission(permissions, 'Users', 'Read')).toBe(true);
  });

  it('denies unknown action', () => {
    expect(hasPermission(permissions, 'Users', 'Delete')).toBe(false);
  });

  it('denies unknown resource', () => {
    expect(hasPermission(permissions, 'Projects', 'Read')).toBe(false);
  });
});
