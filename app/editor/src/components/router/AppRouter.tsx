import { ListView, Login } from 'features';
import { Home } from 'features/home';
import { Route, Routes } from 'react-router-dom';
import { Claim, NotFound } from 'tno-core';

import { PrivateRoute } from '.';

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<Login />} />
      <Route
        path="/admin"
        element={
          <PrivateRoute redirectTo="/login" claims={Claim.administrator}>
            <p>Administration</p>
          </PrivateRoute>
        }
      />
      <Route
        path="/content"
        element={
          <PrivateRoute
            redirectTo="/login"
            claims={Claim.editor}
            element={<ListView />}
          ></PrivateRoute>
        }
      />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
};
