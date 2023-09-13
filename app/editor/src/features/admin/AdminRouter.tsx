import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  ActionForm,
  ActionList,
  ChartTemplateForm,
  ConnectionForm,
  ConnectionList,
  ContentReferenceList,
  ContributorForm,
  ContributorList,
  DataLocationForm,
  DataLocationList,
  FilterForm,
  FilterList,
  IngestDetails,
  IngestForm,
  IngestList,
  IngestSchedule,
  IngestSettings,
  IngestTypeForm,
  IngestTypeList,
  LicenseForm,
  LicenseList,
  MinisterForm,
  MinisterList,
  NotificationForm,
  NotificationList,
  ProductForm,
  ProductList,
  ReachEarnedMedia,
  ReportAdmin,
  ReportForm,
  ReportTemplateForm,
  SeriesForm,
  SeriesList,
  SettingForm,
  SettingList,
  SourceDetails,
  SourceForm,
  SourceList,
  TagList,
  TagsForm,
  TopicList,
  TopicScoreRuleList,
  UserForm,
  UserList,
  WorkOrderForm,
  WorkOrderList,
} from '.';
import { AVOverview } from './av-overviews';
import { SystemMessageForm } from './system-message/SystemMessageForm';

export const AdminRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="users" />} />
      <Route path="users" element={<UserList />} />
      <Route path="users/:id" element={<UserForm />} />

      <Route path="topics" element={<TopicList />} />
      <Route path="topics/:id" element={<TopicList />} />

      <Route path="topic-scores" element={<TopicScoreRuleList />} />

      <Route path="tags" element={<TagList />} />
      <Route path="tags/:id" element={<TagsForm />} />

      <Route path="system-message" element={<SystemMessageForm />} />

      <Route path="programs" element={<SeriesList />} />
      <Route path="programs/:id" element={<SeriesForm />} />

      <Route path="contributors" element={<ContributorList />} />
      <Route path="contributors/:id" element={<ContributorForm />} />

      <Route path="products" element={<ProductList />} />
      <Route path="products/:id" element={<ProductForm />} />

      <Route path="actions" element={<ActionList />} />
      <Route path="actions/:id" element={<ActionForm />} />

      <Route path="licences" element={<LicenseList />} />
      <Route path="licences/:id" element={<LicenseForm />} />

      <Route path="sources" element={<SourceList />} />
      <Route path="sources/:id" element={<SourceForm />}>
        <Route index element={<SourceDetails />} />
        <Route path="details" element={<SourceDetails />} />
        <Route path="metrics" element={<ReachEarnedMedia />} />
      </Route>

      <Route path="connections" element={<ConnectionList />} />
      <Route path="connections/:id" element={<ConnectionForm />} />

      <Route path="ministers" element={<MinisterList />} />
      <Route path="ministers/:id" element={<MinisterForm />} />

      <Route path="data/locations" element={<DataLocationList />} />
      <Route path="data/locations/:id" element={<DataLocationForm />} />

      <Route path="ingest/types" element={<IngestTypeList />} />
      <Route path="ingest/types/:id" element={<IngestTypeForm />} />

      <Route path="ingests" element={<IngestList />} />
      <Route path="ingests/:id" element={<IngestForm />}>
        <Route index element={<IngestDetails />} />
        <Route path="details" element={<IngestDetails />} />
        <Route path="schedule" element={<IngestSchedule />} />
        <Route path="settings" element={<IngestSettings />} />
        <Route path="ingesting" element={<ContentReferenceList />} />
      </Route>

      <Route path="work/orders" element={<WorkOrderList />} />
      <Route path="work/orders/:id" element={<WorkOrderForm />} />

      <Route path="reports" element={<ReportAdmin />} />
      <Route path="reports/:id" element={<ReportForm />} />
      <Route path="report/templates/:id" element={<ReportTemplateForm />} />
      <Route path="report/:path" element={<ReportAdmin />} />
      <Route path="chart/templates" element={<ReportAdmin path="charts" />} />
      <Route path="chart/templates/:id" element={<ChartTemplateForm />} />

      <Route path="filters" element={<FilterList />} />
      <Route path="filters/:id" element={<FilterForm />} />

      <Route path="settings" element={<SettingList />} />
      <Route path="settings/:id" element={<SettingForm />} />

      <Route path="notifications" element={<NotificationList />} />
      <Route path="notifications/:id" element={<NotificationForm />} />

      <Route path="av/evening-overview" element={<AVOverview />} />
    </Routes>
  );
};
