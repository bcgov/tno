import { Tab, Tabs } from 'components/tabs';
import { View } from 'components/view';
import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  CBRAReport,
  ContentLogs,
  DataSource,
  DataSourceList,
  ScheduleContinuos,
  ScheduleProgram,
  ScheduleSingle,
  UserList,
} from '.';
import * as styled from './styled';

export const AdminPage: React.FC = () => {
  return (
    <styled.AdminPage>
      <Tabs
        tabs={
          <>
            <Tab navigateTo="users" label="Users" />
            <Tab navigateTo="data/sources" label="Sources" />
            <Tab navigateTo="contents/log" label="Linked Snippet Log" />
            <Tab navigateTo="reports/cbra" label="CBRA Report" />
          </>
        }
      >
        <View>
          <Routes>
            <Route index element={<Navigate to="reports/cbra" />} />
            <Route path="users" element={<UserList />} />
            <Route path="data/sources" element={<DataSourceList />} />
            <Route path="data/sources/:id" element={<DataSource />}>
              <Route index element={<ScheduleContinuos />} />
              <Route path="schedules" element={<ScheduleContinuos />} />
              <Route path="schedules/daily" element={<ScheduleSingle />} />
              <Route path="schedules/advanced" element={<ScheduleProgram />} />
            </Route>
            <Route path="contents/log" element={<ContentLogs />} />
            <Route path="reports/cbra" element={<CBRAReport />} />
          </Routes>
        </View>
      </Tabs>
    </styled.AdminPage>
  );
};
