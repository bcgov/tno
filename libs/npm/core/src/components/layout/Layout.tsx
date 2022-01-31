import React from 'react';

import { useKeycloakWrapper } from '../../hooks';
import { Footer, Header, Loading, MenuProvider } from '..';
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

  return (
    <styled.Layout {...rest}>
      <MenuProvider>
        <Header name={name} />
        <div className="main-window">
          {showMenu && children.menu}
          <main>
            {children.router}
            {isLoading && <Loading />}
          </main>
        </div>
        <Footer />
      </MenuProvider>
    </styled.Layout>
  );
};
