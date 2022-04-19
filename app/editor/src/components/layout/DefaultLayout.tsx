import { Header } from 'components/header';
import { Home } from 'features/home';
import { UserInfo } from 'features/login';
import { NavBar } from 'features/navbar';
import React from 'react';
import { Outlet } from 'react-router-dom';
import { useToastError } from 'store/hooks';
import { Footer, useKeycloakWrapper } from 'tno-core';

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

  return (
    <styled.Layout {...rest}>
      <UserInfo />
      {keycloak.authenticated ? (
        <>
          <Header name={name} />
          <div className="main-window">
            <NavBar />
            <main>
              <LayoutErrorBoundary>
                <Outlet />
              </LayoutErrorBoundary>
            </main>
          </div>
          <Footer />
        </>
      ) : (
        <div className="main-window">
          <main style={{ backgroundColor: '#f2f2f2' }}>
            <Home />
          </main>
        </div>
      )}
    </styled.Layout>
  );
};
