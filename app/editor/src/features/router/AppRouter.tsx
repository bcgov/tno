import { DefaultLayout } from 'components/layout';
import { AccessRequest } from 'features/access-request';
import { AdminRouter } from 'features/admin';
import { ContentForm, ContentListView } from 'features/content';
import { Login } from 'features/login';
import { StorageListView } from 'features/storage';
import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Claim, NotFound } from 'tno-core';

import { PrivateRoute } from '.';

let isInitialized = false;

export interface IAppRouter {
  name: string;
}

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter: React.FC<IAppRouter> = ({ name }) => {
  const [, { init, isUserReady }] = useApp();
  const isReady = isUserReady();

  React.useEffect(() => {
    if (!isInitialized && isReady) {
      isInitialized = true;
      init();
    }
  }, [init, isReady]);

  return (
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
          path="storage"
          element={
            <PrivateRoute claims={Claim.editor} element={<StorageListView />}></PrivateRoute>
          }
        />
        <Route
          path="contents/:id"
          element={<PrivateRoute claims={Claim.editor} element={<ContentForm />}></PrivateRoute>}
        />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
};
