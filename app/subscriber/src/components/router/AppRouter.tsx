import { Home } from 'features/home';
import { Route, Switch } from 'react-router-dom';
import { Claim, Login, NotFound, PrivateRoute } from 'tno-core';

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter = () => {
  return (
    <Switch>
      <Route path="/login" component={Login}></Route>
      <PrivateRoute path="/admin" claims={Claim.administrator}>
        <p>Administration</p>
      </PrivateRoute>
      <Route path="/" exact={true} component={Home}></Route>
      <Route path="*" exact={true} component={NotFound} />
    </Switch>
  );
};
