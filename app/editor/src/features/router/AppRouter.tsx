import { AdminPage } from 'features/admin';
import { ContentForm, ContentListView } from 'features/content';
import { Login } from 'features/login';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Claim, NotFound } from 'tno-core';

import { PrivateRoute } from '.';

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/contents" />} />
      <Route path="/login" element={<Login />} />
      <Route
        path="/admin/*"
        element={
          <PrivateRoute redirectTo="/login" claims={Claim.administrator} element={<AdminPage />} />
        }
      />
      <Route
        path="/contents"
        element={
          <PrivateRoute
            redirectTo="/login"
            claims={Claim.editor}
            element={<ContentListView />}
          ></PrivateRoute>
        }
      />
      <Route
        path="/contents/:id"
        element={
          <PrivateRoute
            redirectTo="/login"
            claims={Claim.editor}
            element={<ContentForm />}
          ></PrivateRoute>
        }
      />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
};
