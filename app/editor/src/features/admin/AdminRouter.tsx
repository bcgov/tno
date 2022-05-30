import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  CBRAReport,
  ContentLogs,
  ContentReferenceList,
  DataSourceDetails,
  DataSourceForm,
  DataSourceList,
  MediaTypeForm,
  MediaTypeList,
  ReachEarnedMedia,
  ServiceIngestSettings,
  ServiceSchedule,
  UserForm,
  UserList,
} from '.';

export const AdminRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="users" />} />
      <Route path="users" element={<UserList />} />
      <Route path="users/:id" element={<UserForm />} />
      <Route path="media/types" element={<MediaTypeList />} />
      <Route path="media/types/:id" element={<MediaTypeForm />} />
      <Route path="data/sources" element={<DataSourceList />} />
      <Route path="data/sources/:id" element={<DataSourceForm />}>
        <Route index element={<DataSourceDetails />} />
        <Route path="details" element={<DataSourceDetails />} />
        <Route path="metrics" element={<ReachEarnedMedia />} />
        <Route path="ingest/schedule" element={<ServiceSchedule />} />
        <Route path="ingest/settings" element={<ServiceIngestSettings />} />
        <Route path="ingesting" element={<ContentReferenceList />} />
      </Route>
      <Route path="contents/log" element={<ContentLogs />} />
      <Route path="reports/cbra" element={<CBRAReport />} />
    </Routes>
  );
};
