import React, { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

const Dashboard = lazy(async () => await import('features/admin/dashboard/Dashboard'));
const ActionForm = lazy(async () => await import('features/admin/actions/ActionForm'));
const ActionList = lazy(async () => await import('features/admin/actions/ActionList'));
const AVOverview = lazy(async () => await import('features/admin/av-overviews/AVOverview'));
const ChartTemplateForm = lazy(async () => await import('features/admin/charts/ChartTemplateForm'));
const ConnectionForm = lazy(async () => await import('features/admin/connections/ConnectionForm'));
const ConnectionList = lazy(async () => await import('features/admin/connections/ConnectionList'));
const ContentReferenceList = lazy(
  async () => await import('features/admin/ingests/ContentReferenceList'),
);
const ContributorForm = lazy(
  async () => await import('features/admin/contributors/ContributorForm'),
);
const ContributorList = lazy(
  async () => await import('features/admin/contributors/ContributorList'),
);
const DataLocationForm = lazy(
  async () => await import('features/admin/data-locations/DataLocationForm'),
);
const DataLocationList = lazy(
  async () => await import('features/admin/data-locations/DataLocationList'),
);
const FilterForm = lazy(async () => await import('features/admin/filters/FilterForm'));
const FilterList = lazy(async () => await import('features/admin/filters/FilterList'));
const FolderForm = lazy(async () => await import('features/admin/folders/FolderForm'));
const FolderList = lazy(async () => await import('features/admin/folders/FolderList'));
const IngestDetails = lazy(async () => await import('features/admin/ingests/IngestDetails'));
const IngestForm = lazy(async () => await import('features/admin/ingests/IngestForm'));
const IngestList = lazy(async () => await import('features/admin/ingests/IngestList'));
const IngestSchedule = lazy(
  async () => await import('features/admin/ingests/schedules/IngestSchedule'),
);
const IngestSettings = lazy(async () => await import('features/admin/ingests/IngestSettings'));
const IngestTypeForm = lazy(async () => await import('features/admin/ingest-types/IngestTypeForm'));
const IngestTypeList = lazy(async () => await import('features/admin/ingest-types/IngestTypeList'));
const LicenseForm = lazy(async () => await import('features/admin/licenses/LicenseForm'));
const LicenseList = lazy(async () => await import('features/admin/licenses/LicenseList'));
const MediaTypeForm = lazy(async () => await import('features/admin/media-types/MediaTypeForm'));
const MediaTypeList = lazy(async () => await import('features/admin/media-types/MediaTypeList'));
const MinisterForm = lazy(async () => await import('features/admin/ministers/MinisterForm'));
const MinisterList = lazy(async () => await import('features/admin/ministers/MinisterList'));
const NotificationForm = lazy(
  async () => await import('features/admin/notifications/NotificationForm'),
);
const NotificationList = lazy(
  async () => await import('features/admin/notifications/NotificationList'),
);
const ProductForm = lazy(async () => await import('features/admin/products/ProductForm'));
const ProductList = lazy(async () => await import('features/admin/products/ProductList'));
const ReachEarnedMedia = lazy(async () => await import('features/admin/sources/ReachEarnedMedia'));
const ReportAdmin = lazy(async () => await import('features/admin/reports/ReportAdmin'));
const ReportForm = lazy(async () => await import('features/admin/reports/ReportForm'));
const ReportDashboard = lazy(
  async () => await import('features/admin/reports/dashboard/ReportDashboard'),
);
const NotificationsDashboard = lazy(
  async () => await import('features/admin/notifications/dashboard/NotificationsDashboard'),
);
const ReportTemplateForm = lazy(
  async () => await import('features/admin/report-templates/ReportTemplateForm'),
);
const SeriesDetails = lazy(async () => await import('features/admin/series/SeriesDetails'));
const SeriesForm = lazy(async () => await import('features/admin/series/SeriesForm'));
const SeriesList = lazy(async () => await import('features/admin/series/SeriesList'));
const SeriesMerge = lazy(async () => await import('features/admin/series/SeriesMerge'));
const SettingForm = lazy(async () => await import('features/admin/settings/SettingForm'));
const SettingList = lazy(async () => await import('features/admin/settings/SettingList'));
const SourceDetails = lazy(async () => await import('features/admin/sources/SourceDetails'));
const SourceForm = lazy(async () => await import('features/admin/sources/SourceForm'));
const SourceList = lazy(async () => await import('features/admin/sources/SourceList'));
const SystemMessageList = lazy(
  async () => await import('features/admin/system-message/SystemMessageList'),
);
const SystemMessageForm = lazy(
  async () => await import('features/admin/system-message/SystemMessageForm'),
);
const TagList = lazy(async () => await import('features/admin/tags/TagList'));
const TagsForm = lazy(async () => await import('features/admin/tags/TagsForm'));
const TopicList = lazy(async () => await import('features/admin/topics/TopicList'));
const TopicScoreRuleList = lazy(
  async () => await import('features/admin/topic-score-rules/TopicScoreRuleList'),
);
const UserForm = lazy(async () => await import('features/admin/users/UserForm'));
const UserList = lazy(async () => await import('features/admin/users/UserList'));
const WorkOrderForm = lazy(async () => await import('features/admin/work-orders/WorkOrderForm'));
const WorkOrderList = lazy(async () => await import('features/admin/work-orders/WorkOrderList'));

export const AdminRouter: React.FC = () => {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Routes>
        <Route index element={<Navigate to="users" />} />
        <Route path="dashboard" element={<Dashboard />} />

        <Route path="users" element={<UserList />} />
        <Route path="users/:id" element={<UserForm />} />
        <Route path="users/:id/:type" element={<UserForm />} />
        <Route path="users/:id/:type/:tab" element={<UserForm />} />

        <Route path="topics" element={<TopicList />} />
        <Route path="topics/:id" element={<TopicList />} />

        <Route path="topic-scores" element={<TopicScoreRuleList />} />

        <Route path="tags" element={<TagList />} />
        <Route path="tags/:id" element={<TagsForm />} />

        <Route path="system-messages" element={<SystemMessageList />} />
        <Route path="system-messages/:id" element={<SystemMessageForm />} />

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
        <Route path="reports/dashboard" element={<ReportDashboard />} />
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
        <Route path="notifications/dashboard" element={<NotificationsDashboard />} />

        <Route path="av/evening-overview" element={<AVOverview />} />

        <Route path="work/orders" element={<WorkOrderList />} />
        <Route path="work/orders/:id" element={<WorkOrderForm />} />
      </Routes>
    </Suspense>
  );
};
