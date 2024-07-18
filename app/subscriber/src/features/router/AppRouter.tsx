import { DefaultLayout, LayoutAnonymous } from 'components/layout';
import { AccessRequest } from 'features/access-request';
import { FilterMediaLanding } from 'features/filter-media';
import { Help } from 'features/help';
import { Impersonation } from 'features/impersonation';
import { Landing } from 'features/landing';
import { Login } from 'features/login';
import { MyColleagues } from 'features/my-colleagues';
import { ColleagueEdit } from 'features/my-colleagues/ColleagueEdit';
import { FolderLanding } from 'features/my-folders';
import { MyProducts } from 'features/my-products';
import { MyReports } from 'features/my-reports';
import { ReportView } from 'features/my-reports';
import { ReportEditPage } from 'features/my-reports/edit';
import { SearchPage } from 'features/search-page';
import { SettingsLanding } from 'features/settings';
import React from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Claim, InternalServerError, NotFound } from 'tno-core';

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
    // There is a race condition, when keycloak is ready state related to user claims will not be.
    // Additionally, when the user is not authenticated keycloak also is not initialized (which makes no sense).
    if (
      !authenticated &&
      !window.location.pathname.startsWith('/login') &&
      !window.location.pathname.startsWith('/report/instances')
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
              element={<SearchPage showAdvanced={false} />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/:id"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced={false} />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/advanced"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced={true} />}
            ></PrivateRoute>
          }
        />
        <Route
          path="/search/advanced/:id"
          element={
            <PrivateRoute
              claims={Claim.subscriber}
              element={<SearchPage showAdvanced={true} />}
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
            <PrivateRoute claims={Claim.subscriber} element={<MyColleagues />}></PrivateRoute>
          }
        />
        <Route
          path="/colleagues/add"
          element={
            <PrivateRoute claims={Claim.subscriber} element={<ColleagueEdit />}></PrivateRoute>
          }
        />
        <Route
          path="/view/:id"
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
          path="/impersonation"
          element={
            <PrivateRoute claims={Claim.administrator} element={<Impersonation />}></PrivateRoute>
          }
        />
        <Route path="error" element={<InternalServerError />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
};
