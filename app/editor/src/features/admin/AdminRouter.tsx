import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  ActionForm,
  ActionList,
  CategoryForm,
  CategoryList,
  ConnectionForm,
  ConnectionList,
  ContentReferenceList,
  DataLocationForm,
  DataLocationList,
  IngestDetails,
  IngestForm,
  IngestList,
  IngestSchedule,
  IngestSettings,
  IngestTypeForm,
  IngestTypeList,
  LicenseForm,
  LicenseList,
  ProductForm,
  ProductList,
  ReachEarnedMedia,
  SeriesForm,
  SeriesList,
  SourceDetails,
  SourceForm,
  SourceList,
  TagList,
  TagsForm,
  UserForm,
  UserList,
  WorkOrderForm,
  WorkOrderList,
} from '.';

export const AdminRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="users" />} />
      <Route path="users" element={<UserList />} />
      <Route path="users/:id" element={<UserForm />} />

      <Route path="categories" element={<CategoryList />} />
      <Route path="categories/:id" element={<CategoryForm />} />

      <Route path="tags" element={<TagList />} />
      <Route path="tags/:id" element={<TagsForm />} />

      <Route path="series" element={<SeriesList />} />
      <Route path="series/:id" element={<SeriesForm />} />

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
    </Routes>
  );
};
