import { Tab, Tabs } from 'components/tabs';
import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { CBRAReport, ContentLogs, UserList } from '.';
import * as styled from './styled';

export const AdminPage: React.FC = () => {
  return (
    <styled.AdminPage>
      <Tabs
        tabs={
          <>
            <Tab navigateTo="contents/log" label="Linked Snippet Log" />
            <Tab navigateTo="users" label="Users" />
            <Tab navigateTo="reports/cbra" label="CBRA Report" />
          </>
        }
      >
        <Routes>
          <Route index element={<Navigate to="reports/cbra" />} />
          <Route path="contents/log" element={<ContentLogs />} />
          <Route path="users" element={<UserList />} />
          <Route path="reports/cbra" element={<CBRAReport />} />
        </Routes>
      </Tabs>
    </styled.AdminPage>
  );
};
