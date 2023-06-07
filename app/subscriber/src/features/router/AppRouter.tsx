import { DefaultLayout } from 'components/layout';
import { AccessRequest } from 'features/access-request';
import { Landing } from 'features/landing';
import { Login } from 'features/login';
import React from 'react';
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { InternalServerError, NotFound } from 'tno-core';

export interface IAppRouter {
  name: string;
}

/**
 * AppRouter provides a SPA router to manage routes.
 * Renders router when the application has been initialized.
 * @returns AppRouter component.
 */
export const AppRouter: React.FC<IAppRouter> = () => {
  const [, { authenticated }] = useApp();
  const navigate = useNavigate();

  React.useEffect(() => {
    // There is a race condition, when keycloak is ready state related to user claims will not be.
    // Additionally, when the user is not authenticated keycloak also is not initialized (which makes no sense).
    if (!authenticated && !window.location.pathname.startsWith('/login'))
      navigate(`/login?redirectTo=${window.location.pathname}`);
  }, [authenticated, navigate]);

  return (
    <Routes>
      <Route path="/" element={<DefaultLayout />}>
        <Route path="/" element={<Navigate to="/landing/home" />} />
        <Route path="login" element={<Login />} />
        <Route path="welcome" element={<AccessRequest />} />
        <Route path="access/request" element={<AccessRequest />} />
        <Route path="/landing/:id" element={<Landing />} />
        <Route path="/view/:id" element={<Landing />} />
        <Route path="/view/my-minister/:id" element={<Landing />} />
        <Route path="error" element={<InternalServerError />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
};
