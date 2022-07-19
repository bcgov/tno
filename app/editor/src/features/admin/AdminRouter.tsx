import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  ActionForm,
  ActionList,
  CategoryForm,
  CategoryList,
  CBRAReport,
  ContentLogs,
  ContentReferenceList,
  DataSourceDetails,
  DataSourceForm,
  DataSourceList,
  MediaTypeForm,
  MediaTypeList,
  ReachEarnedMedia,
  SeriesForm,
  SeriesList,
  ServiceIngestSettings,
  ServiceSchedule,
  TagList,
  TagsForm,
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
      <Route path="categories" element={<CategoryList />} />
      <Route path="tags" element={<TagList />} />
      <Route path="actions" element={<ActionList />} />
      <Route path="series" element={<SeriesList />} />
      <Route path="media/types/:id" element={<MediaTypeForm />} />
      <Route path="series/:id" element={<SeriesForm />} />
      <Route path="tags/:id" element={<TagsForm />} />
      <Route path="actions/:id" element={<ActionForm />} />
      <Route path="categories/:id" element={<CategoryForm />} />

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
