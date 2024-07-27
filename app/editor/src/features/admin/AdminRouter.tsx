import React, { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

const Dashboard = lazy(() => import('features/admin/dashboard/Dashboard'));
const ActionForm = lazy(() => import('features/admin/actions/ActionForm'));
const ActionList = lazy(() => import('features/admin/actions/ActionList'));
const AVOverview = lazy(() => import('features/admin/av-overviews/AVOverview'));
const ChartTemplateForm = lazy(() => import('features/admin/charts/ChartTemplateForm'));
const ConnectionForm = lazy(() => import('features/admin/connections/ConnectionForm'));
const ConnectionList = lazy(() => import('features/admin/connections/ConnectionList'));
const ContentReferenceList = lazy(() => import('features/admin/ingests/ContentReferenceList'));
const ContributorForm = lazy(() => import('features/admin/contributors/ContributorForm'));
const ContributorList = lazy(() => import('features/admin/contributors/ContributorList'));
const DataLocationForm = lazy(() => import('features/admin/data-locations/DataLocationForm'));
const DataLocationList = lazy(() => import('features/admin/data-locations/DataLocationList'));
const FilterForm = lazy(() => import('features/admin/filters/FilterForm'));
const FilterList = lazy(() => import('features/admin/filters/FilterList'));
const FolderForm = lazy(() => import('features/admin/folders/FolderForm'));
const FolderList = lazy(() => import('features/admin/folders/FolderList'));
const IngestDetails = lazy(() => import('features/admin/ingests/IngestDetails'));
const IngestForm = lazy(() => import('features/admin/ingests/IngestForm'));
const IngestList = lazy(() => import('features/admin/ingests/IngestList'));
const IngestSchedule = lazy(() => import('features/admin/ingests/schedules/IngestSchedule'));
const IngestSettings = lazy(() => import('features/admin/ingests/IngestSettings'));
const IngestTypeForm = lazy(() => import('features/admin/ingest-types/IngestTypeForm'));
const IngestTypeList = lazy(() => import('features/admin/ingest-types/IngestTypeList'));
const LicenseForm = lazy(() => import('features/admin/licenses/LicenseForm'));
const LicenseList = lazy(() => import('features/admin/licenses/LicenseList'));
const MediaTypeForm = lazy(() => import('features/admin/media-types/MediaTypeForm'));
const MediaTypeList = lazy(() => import('features/admin/media-types/MediaTypeList'));
const MinisterForm = lazy(() => import('features/admin/ministers/MinisterForm'));
const MinisterList = lazy(() => import('features/admin/ministers/MinisterList'));
const NotificationForm = lazy(() => import('features/admin/notifications/NotificationForm'));
const NotificationList = lazy(() => import('features/admin/notifications/NotificationList'));
const ProductForm = lazy(() => import('features/admin/products/ProductForm'));
const ProductList = lazy(() => import('features/admin/products/ProductList'));
const ReachEarnedMedia = lazy(() => import('features/admin/sources/ReachEarnedMedia'));
const ReportAdmin = lazy(() => import('features/admin/reports/ReportAdmin'));
const ReportForm = lazy(() => import('features/admin/reports/ReportForm'));
const ReportTemplateForm = lazy(() => import('features/admin/report-templates/ReportTemplateForm'));
const SeriesDetails = lazy(() => import('features/admin/series/SeriesDetails'));
const SeriesForm = lazy(() => import('features/admin/series/SeriesForm'));
const SeriesList = lazy(() => import('features/admin/series/SeriesList'));
const SeriesMerge = lazy(() => import('features/admin/series/SeriesMerge'));
const SettingForm = lazy(() => import('features/admin/settings/SettingForm'));
const SettingList = lazy(() => import('features/admin/settings/SettingList'));
const SourceDetails = lazy(() => import('features/admin/sources/SourceDetails'));
const SourceForm = lazy(() => import('features/admin/sources/SourceForm'));
const SourceList = lazy(() => import('features/admin/sources/SourceList'));
const SystemMessageForm = lazy(() => import('features/admin/system-message/SystemMessageForm'));
const TagList = lazy(() => import('features/admin/tags/TagList'));
const TagsForm = lazy(() => import('features/admin/tags/TagsForm'));
const TopicList = lazy(() => import('features/admin/topics/TopicList'));
const TopicScoreRuleList = lazy(
  () => import('features/admin/topic-score-rules/TopicScoreRuleList'),
);
const UserForm = lazy(() => import('features/admin/users/UserForm'));
const UserList = lazy(() => import('features/admin/users/UserList'));
const WorkOrderForm = lazy(() => import('features/admin/work-orders/WorkOrderForm'));
const WorkOrderList = lazy(() => import('features/admin/work-orders/WorkOrderList'));

export const AdminRouter: React.FC = () => {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Routes>
        <Route index element={<Navigate to="users" />} />
        <Route path="dashboard" element={<Dashboard />} />

        <Route path="users" element={<UserList />} />
        <Route path="users/:id" element={<UserForm />} />
        <Route path="users/:id/:type" element={<UserForm />} />

        <Route path="topics" element={<TopicList />} />
        <Route path="topics/:id" element={<TopicList />} />

        <Route path="topic-scores" element={<TopicScoreRuleList />} />

        <Route path="tags" element={<TagList />} />
        <Route path="tags/:id" element={<TagsForm />} />

        <Route path="system-message" element={<SystemMessageForm />} />

        {/* <Route path="programs" element={<SeriesList />} />
        <Route path="programs/:id" element={<SeriesForm />} /> */}

        <Route path="programs" element={<SeriesList />} />
        <Route path="programs/:id" element={<SeriesForm />}>
          <Route index element={<SeriesDetails />} />
          <Route path="details" element={<SeriesDetails />} />
          <Route path="merge" element={<SeriesMerge />} />
        </Route>

        <Route path="contributors" element={<ContributorList />} />
        <Route path="contributors/:id" element={<ContributorForm />} />

        <Route path="media-types" element={<MediaTypeList />} />
        <Route path="media-types/:id" element={<MediaTypeForm />} />

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

        <Route path="products" element={<ProductList />} />
        <Route path="products/:id" element={<ProductForm />} />

        <Route path="reports" element={<ReportAdmin />} />
        <Route path="reports/:id" element={<ReportForm />} />
        <Route path="report/templates/:id" element={<ReportTemplateForm />} />
        <Route path="report/:path" element={<ReportAdmin />} />
        <Route path="chart/templates" element={<ReportAdmin path="charts" />} />
        <Route path="chart/templates/:id" element={<ChartTemplateForm />} />

        <Route path="filters" element={<FilterList />} />
        <Route path="filters/:id" element={<FilterForm />} />

        <Route path="folders" element={<FolderList />} />
        <Route path="folders/:id" element={<FolderForm />} />

        <Route path="settings" element={<SettingList />} />
        <Route path="settings/:id" element={<SettingForm />} />

        <Route path="notifications" element={<NotificationList />} />
        <Route path="notifications/:id" element={<NotificationForm />} />

        <Route path="av/evening-overview" element={<AVOverview />} />

        <Route path="work/orders" element={<WorkOrderList />} />
        <Route path="work/orders/:id" element={<WorkOrderForm />} />
      </Routes>
    </Suspense>
  );
};
