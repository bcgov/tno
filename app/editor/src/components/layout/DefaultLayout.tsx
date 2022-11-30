import { Home } from 'features/home';
import { UserInfo } from 'features/login';
import { NavBar } from 'features/navbar';
import React from 'react';
import { Outlet } from 'react-router-dom';
import { useToastError } from 'store/hooks';
import { Button, Header, Show, SummonContext, useKeycloakWrapper } from 'tno-core';

import { LayoutErrorBoundary } from '.';
import * as styled from './styled';

interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Site name to display in header.
   */
  name: string;
}

/**
 * DefaultLayout provides a div structure to organize the page for users once Keycloak has initialized.
 * @param param0 Component properties.
 * @returns DefaultLayout component.
 */
export const DefaultLayout: React.FC<ILayoutProps> = ({ name, children, ...rest }) => {
  const keycloak = useKeycloakWrapper();
  useToastError();

  const state = React.useContext(SummonContext);
  keycloak.instance.onTokenExpired = () => {
    keycloak.instance
      .updateToken(86400)
      .then(function (refreshed: boolean) {
        if (refreshed) {
          state.setToken(keycloak.instance.token);
        }
      })
      .catch(function () {
        keycloak.instance.logout();
      });
  };

  return (
    <styled.Layout {...rest}>
      <UserInfo />
      <Show visible={keycloak.authenticated}>
        <Header name={name}>
          <Button onClick={() => keycloak.instance.logout()} name="signOut">
            Sign Out
          </Button>
        </Header>
        <div className="main-window">
          <NavBar />
          <main>
            <LayoutErrorBoundary>
              <Outlet />
            </LayoutErrorBoundary>
          </main>
        </div>
      </Show>
      <Show visible={!keycloak.authenticated}>
        <div className="main-window">
          <main style={{ backgroundColor: '#f2f2f2' }}>
            <Home />
          </main>
        </div>
      </Show>
    </styled.Layout>
  );
};
