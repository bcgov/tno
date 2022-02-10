import { Header } from 'components';
import { Home } from 'features';
import React from 'react';
import { Footer, Loading, MenuProvider, useKeycloakWrapper } from 'tno-core';

import * as styled from './LayoutStyled';

interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Site name to display in header.
   */
  name: string;
  children: {
    menu?: React.ReactNode;
    router: React.ReactNode;
  };
}

/**
 * Layout provides a div structure to organize the page.
 * @param param0 Div element attributes.
 * @returns Layout component.
 */
export const Layout: React.FC<ILayoutProps> = ({ name, children, ...rest }) => {
  const keycloak = useKeycloakWrapper();
  const [isLoading] = React.useState(false);
  const showMenu = !!keycloak.authenticated && !!children.menu;

  return keycloak.authenticated ? (
    <styled.Layout {...rest}>
      <MenuProvider>
        <Header name={name} />
        <div className="main-window">
          {showMenu && children.menu}
          <main style={{ backgroundColor: '#f2f2f2', margin: '0px' }}>
            {children.router}
            {isLoading && <Loading />}
          </main>
        </div>
        <Footer />
      </MenuProvider>
    </styled.Layout>
  ) : (
    <styled.Layout {...rest}>
      <MenuProvider>
        <div className="main-window">
          <main style={{ backgroundColor: '#f2f2f2' }}>
            <Home />
            {isLoading && <Loading />}
          </main>
        </div>
      </MenuProvider>
    </styled.Layout>
  );
};
