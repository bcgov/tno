import React from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { Claim, InternalServerError, NotFound } from 'tno-core';

import { DefaultLayout, LayoutAnonymous } from '@/subscriber/components/layout';
import { AccessRequest } from '@/subscriber/features/access-request';
import { AIAnalysisPage } from '@/subscriber/features/ai-analysis';
import { FilterMediaLanding } from '@/subscriber/features/filter-media';
import { Help } from '@/subscriber/features/help';
import { Impersonation } from '@/subscriber/features/impersonation';
import { Landing } from '@/subscriber/features/landing';
import { Login } from '@/subscriber/features/login';
import { MyColleaguesLanding } from '@/subscriber/features/my-colleagues';
import { FolderLanding } from '@/subscriber/features/my-folders';
import { MyProducts } from '@/subscriber/features/my-products';
import { MyReports, ReportView } from '@/subscriber/features/my-reports';
import { ReportEditPage } from '@/subscriber/features/my-reports/edit';
import { SearchPage } from '@/subscriber/features/search-page';
import { SettingsLanding } from '@/subscriber/features/settings';
import { TranscriptionRequest } from '@/subscriber/features/transcription-request';
import { useApp } from '@/subscriber/store/hooks';

import { PrivateRoute } from './PrivateRoute';

/**
 * AppRouter provides a SPA router to manage routes.
 * Renders router when the application has been initialized.
 * @returns AppRouter component.
 */
export const AppRouter: React.FC = () => {
  const [, { authenticated }] = useApp();
  const navigate = useNavigate();

  React.useEffect(() => {
    window.snowplow('trackPageView');
    // There is a race condition, when keycloak is ready state related to user claims will not be.
    // Additionally, when the user is not authenticated keycloak also is not initialized (which makes no sense).
    if (
      !authenticated &&
      !window.location.pathname.startsWith('/login') &&
      !window.location.pathname.startsWith('/report/instances') &&
      !window.location.pathname.startsWith('/transcribe')
    )
      navigate(`/login?redirectTo=${window.location.pathname}`);
  }, [authenticated, navigate]);

  if (!authenticated)
    return (
      <Routes>
        <Route
          path="/report/instances/:id/view"
          element={
            <LayoutAnonymous name={''}>
              <ReportView regenerate={false} />
            </LayoutAnonymous>
          }
        />
        <Route
          path="/transcribe/:id"
          element={
            <LayoutAnonymous name={''}>
              <TranscriptionRequest />
            </LayoutAnonymous>
          }
        />
        <Route path="/" element={<DefaultLayout />}>
          <Route path="/" element={<Navigate to="/landing/home" />} />
          <Route path="login" element={<Login />} />
          <Route path="help" element={<Help />} />
          <Route path="welcome" element={<AccessRequest />} />
          <Route path="access/request" element={<AccessRequest />} />
        </Route>
      </Routes>
    );

  return (
    <Routes>
      <Route path="/" element={<DefaultLayout />}>
        <Route path="/" element={<Navigate to="/landing/home" />} />
        <Route path="login" element={<Login />} />
        <Route path="help" element={<Help />} />
        <Route path="welcome" element={<AccessRequest />} />
        <Route path="access/request" element={<AccessRequest />} />
        <Route
          path="/landing/:id"
          element={<PrivateRoute claims={Claim.subscriber} element={<Landing />}></PrivateRoute>}
        />
        <Route
          path="/search"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced={false} showDate />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/:id"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced={false} showDate />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/advanced"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced showDate />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/advanced/:id"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced showDate />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/folders/:id?"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<FolderLanding />}></PrivateRoute>
          }
        />
        <Route
          path="/settings"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<SettingsLanding />}></PrivateRoute>
          }
        />
        <Route
          path="/folders/:action?/:id?"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<FolderLanding />}></PrivateRoute>
          }
        />
        <Route
          path="/colleagues"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<MyColleaguesLanding />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/view/:id/:popout?"
          element={<PrivateRoute claims={Claim.subscriber} element={<Landing />}></PrivateRoute>}
        />
        <Route
          path="/view/my-minister/:id"
          element={<PrivateRoute claims={Claim.subscriber} element={<Landing />}></PrivateRoute>}
        />
        <Route
          path="/products"
          element={<PrivateRoute claims={Claim.subscriber} element={<MyProducts />}></PrivateRoute>}
        />
        <Route
          path="/filter-media"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<FilterMediaLanding />}></PrivateRoute>
          }
        />
        <Route
          path="/reports"
          element={<PrivateRoute claims={Claim.subscriber} element={<MyReports />}></PrivateRoute>}
        />
        <Route
          path="/reports/:id/:path1"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<ReportEditPage />}></PrivateRoute>
          }
        />
        <Route
          path="/reports/:id/:path1/:path2"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<ReportEditPage />}></PrivateRoute>
          }
        />
        <Route
          path="/reports/:id"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<ReportEditPage />}></PrivateRoute>
          }
        />
        <Route path="report/instances/:id/view" element={<ReportView regenerate={false} />} />
        <Route
          path="/ai"
          element={
            <PrivateRoute claims={Claim.administrator} element={<AIAnalysisPage />}></PrivateRoute>
          }
        />
        <Route
          path="/impersonation"
          element={
            <PrivateRoute claims={Claim.administrator} element={<Impersonation />}></PrivateRoute>
          }
        />
        <Route path="/transcribe/:id" element={<TranscriptionRequest />} />
        <Route path="error" element={<InternalServerError />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
};
