import { AdminRouter } from 'features/admin';
import React, { lazy, Suspense } from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Claim, ContentTypeName, InternalServerError, NotFound } from 'tno-core';

const DefaultLayout = lazy(() => import('components/layout/DefaultLayout'));
const AccessRequest = lazy(() => import('features/access-request/AccessRequest'));
const RequestClip = lazy(() => import('features/clips/RequestClip'));
const ContentForm = lazy(() => import('features/content/form/ContentForm'));
const ContentListView = lazy(() => import('features/content/list-view/ContentListView'));
const Papers = lazy(() => import('features/content/papers/Papers'));
const DemoPage = lazy(() => import('features/demo/DemoPage'));
const Login = lazy(() => import('features/login/Login'));
const AVOverview = lazy(() => import('features/reports/av-overview/AVOverview'));
const AVOverviewPreview = lazy(() => import('features/reports/av-overview/AVOverviewPreview'));
const ReportInstancePreview = lazy(() => import('features/reports/ReportInstancePreview'));
const ReportPreview = lazy(() => import('features/reports/ReportPreview'));
const ReportsRouter = lazy(() => import('features/reports/ReportsRouter'));
const StorageListView = lazy(() => import('features/storage/StorageListView'));
const PrivateRoute = lazy(() => import('features/router/PrivateRoute'));
const TranscriptionList = lazy(
  () => import('features/work-orders/transcription/TranscriptionList'),
);
const EventOfTheDayList = lazy(() => import('features/admin/event-of-the-day/EventOfTheDayList'));

export interface IAppRouter {
  name: string;
}

/**
 * AppRouter provides a SPA router to manage routes.
 * Renders router when the application has been initialized.
 * @returns AppRouter component.
 */
export const AppRouter: React.FC<IAppRouter> = ({ name }) => {
  const [, { authenticated }] = useApp();
  const navigate = useNavigate();

  React.useEffect(() => {
    // There is a race condition, when keycloak is ready state related to user claims will not be.
    // Additionally, when the user is not authenticated keycloak also is not initialized (which makes no sense).
    if (!authenticated && !window.location.pathname.startsWith('/login'))
      navigate(`/login?redirectTo=${window.location.pathname}`);
  }, [authenticated, navigate]);

  return (
    <Suspense fallback={<div>Loading...</div>}>
      <Routes>
        <Route path="/" element={<DefaultLayout name={name} />}>
          <Route path="/" element={<Navigate to="/contents" />} />
          <Route path="login" element={<Login />} />
          <Route path="welcome" element={<AccessRequest />} />
          <Route path="access/request" element={<AccessRequest />} />
          <Route
            path="admin/*"
            element={<PrivateRoute claims={Claim.administrator} element={<AdminRouter />} />}
          />
          <Route
            path="contents"
            element={
              <PrivateRoute claims={Claim.editor} element={<ContentListView />}></PrivateRoute>
            }
          />
          <Route
            path="contents/:id"
            element={<PrivateRoute claims={Claim.editor} element={<ContentForm />}></PrivateRoute>}
          />
          <Route
            path="/contents/combined/:id"
            element={
              <PrivateRoute claims={Claim.editor} element={<ContentListView />}></PrivateRoute>
            }
          />
          <Route
            path="snippets/:id"
            element={
              <PrivateRoute
                claims={Claim.editor}
                element={<ContentForm contentType={ContentTypeName.AudioVideo} />}
              ></PrivateRoute>
            }
          />
          <Route
            path="papers/:id"
            element={
              <PrivateRoute
                claims={Claim.editor}
                element={<ContentForm contentType={ContentTypeName.PrintContent} />}
              ></PrivateRoute>
            }
          />
          <Route
            path="images/:id"
            element={
              <PrivateRoute
                claims={Claim.editor}
                element={<ContentForm contentType={ContentTypeName.Image} />}
              ></PrivateRoute>
            }
          />
          <Route
            path="stories/:id"
            element={
              <PrivateRoute
                claims={Claim.editor}
                element={<ContentForm contentType={ContentTypeName.Internet} />}
              ></PrivateRoute>
            }
          />
          <Route
            path="papers"
            element={<PrivateRoute claims={Claim.editor} element={<Papers />}></PrivateRoute>}
          />
          <Route
            path="/papers/combined/:id"
            element={<PrivateRoute claims={Claim.editor} element={<Papers />}></PrivateRoute>}
          />
          <Route
            path="storage/locations/:id"
            element={
              <PrivateRoute claims={Claim.editor} element={<StorageListView />}></PrivateRoute>
            }
          />
          <Route path="clips" element={<RequestClip />} />
          <Route path="transcriptions" element={<TranscriptionList />} />

          <Route
            path="report/instances/:id/view"
            element={
              <PrivateRoute
                claims={Claim.editor}
                element={<ReportInstancePreview />}
              ></PrivateRoute>
            }
          />

          <Route path="reports/event-of-the-day" element={<EventOfTheDayList />} />
          <Route
            path="reports/av/evening-overview"
            element={<PrivateRoute claims={Claim.editor} element={<AVOverview />}></PrivateRoute>}
          />
          <Route
            path="reports/av/evening-overview/:id"
            element={
              <PrivateRoute claims={Claim.editor} element={<AVOverviewPreview />}></PrivateRoute>
            }
          />
          <Route
            path="reports/:id/preview"
            element={
              <PrivateRoute claims={Claim.editor} element={<ReportPreview />}></PrivateRoute>
            }
          />
          <Route
            path="reports/*"
            element={<PrivateRoute claims={Claim.administrator} element={<ReportsRouter />} />}
          />
          <Route path="demo" element={<DemoPage />} />
          <Route path="error" element={<InternalServerError />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Suspense>
  );
};
