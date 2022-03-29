import { View } from 'components/view';
import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  CBRAReport,
  ContentLogs,
  DataSource,
  DataSourceDetails,
  DataSourceList,
  ReachEarnedMedia,
  Schedule,
  ScheduleContinuous,
  ScheduleProgram,
  ScheduleSingle,
  UserList,
} from '.';
import {
  advancedSchedule,
  continuousSchedule,
  singleSchedule,
} from './sources/schedules/constants';
import * as styled from './styled';

export const AdminPage: React.FC = () => {
  return (
    <styled.AdminPage>
      <View>
        <Routes>
          <Route index element={<Navigate to="reports/cbra" />} />
          <Route path="users" element={<UserList />} />
          <Route path="data/sources" element={<DataSourceList />} />
          <Route path="data/sources/:id" element={<DataSource />}>
            <Route index element={<DataSourceDetails />} />
            <Route path="details" element={<DataSourceDetails />} />
            <Route path="schedules" element={<Schedule />}>
              <Route
                index
                element={<ScheduleContinuous index={0} message={continuousSchedule} />}
              />
              <Route
                path="continuous"
                element={<ScheduleContinuous index={0} message={continuousSchedule} />}
              />
              <Route path="daily" element={<ScheduleSingle index={0} message={singleSchedule} />} />
              <Route path="advanced" element={<ScheduleProgram message={advancedSchedule} />} />
            </Route>
            <Route path="metrics" element={<ReachEarnedMedia />} />
          </Route>
          <Route path="contents/log" element={<ContentLogs />} />
          <Route path="reports/cbra" element={<CBRAReport />} />
        </Routes>
      </View>
    </styled.AdminPage>
  );
};
