import React from 'react';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { cleanup, render, screen, waitFor } from '@testing-library/react';
import MockAdapter from 'axios-mock-adapter';
import api from '@/lib/axios';
import { AuthProvider } from '@/context/AuthContext';
import { OrganizationProvider } from '@/context/OrganizationContext';
import type { VerifyResponse } from '@/services/common/AuthService';
import ProjectsPage from '@/app/admin/projects/page';
import ModulesPage from '@/app/admin/modules/page';

describe('Admin lookup authorization', () => {
 let mock: MockAdapter;
 beforeEach(() => {
  mock = new MockAdapter(api);
  mock.onGet(/get-all$/).reply(200, {items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10});
  mock.onGet(/available-/).reply(200, []);
 });
 afterEach(() => { cleanup(); mock.restore(); });

 const cases = [
  {Page: ProjectsPage, resource: 'Projects', loading: 'Projeler Getiriliyor...', lookups: ['/api/admin/projects/available-users', '/api/admin/projects/available-modules'], management: ['ManageMembers', 'ManageModules']},
  {Page: ModulesPage, resource: 'Modules', loading: 'Modüller Getiriliyor...', lookups: ['/api/admin/modules/available-experts'], management: ['ManageExperts']},
 ];

 for (const {Page, resource, loading, lookups, management} of cases) {
  function renderPage(actions: string[]) {
   const userInfo: VerifyResponse = {authenticated: true, role: 'Viewer', username: 'viewer', userId: 2, isPasswordChangeRequired: false, permissions: {modules: [{resourceKey: resource, actions}]}};
   return render(<AuthProvider userInfo={userInfo} permissions={userInfo.permissions}><OrganizationProvider initialOrgInfo={{organizationName:'Test'}}><Page /></OrganizationProvider></AuthProvider>);
  }
  it(`${resource}: read/update permissions do not request management lookups`, async () => {
   renderPage(['Read', 'Update']);
   await waitFor(() => expect(screen.queryByText(loading)).not.toBeInTheDocument());
   expect(mock.history.get.some(r => r.url?.endsWith('/get-all'))).toBe(true);
   for (const url of lookups) expect(mock.history.get.some(r => r.url === url)).toBe(false);
  });
  it(`${resource}: requests permitted management lookups`, async () => {
   renderPage(['Read', ...management]);
   await waitFor(() => expect(screen.queryByText(loading)).not.toBeInTheDocument());
   for (const url of lookups) expect(mock.history.get.some(r => r.url === url)).toBe(true);
  });
 }
});
