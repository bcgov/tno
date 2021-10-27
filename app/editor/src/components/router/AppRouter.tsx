import { Login } from 'components/login';
import { Home } from 'features/home';
import { BrowserRouter, Route, Switch } from 'react-router-dom';

import { PrivateRoute } from '.';

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter = () => {
  return (
    <BrowserRouter basename={process.env.PUBLIC_URL}>
      <Switch>
        <Route path="/login">
          <Login />
        </Route>
        <PrivateRoute path="/secure" component={Secure} />
        <Route path="/">
          <Home />
        </Route>
      </Switch>
    </BrowserRouter>
  );
};

const Secure = () => {
  return <p>Secure</p>;
};
