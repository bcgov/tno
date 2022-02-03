import { NavBarItem } from 'components/navbar/NavBarItemStyled';
import { NavBar } from 'components/navbar/NavBarStyled';
import { Home } from 'features';
import { useNavState } from 'hooks';
import React from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { Footer, Header, Loading, MenuProvider, useKeycloakWrapper } from 'tno-core';

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
  const [active, setActive] = React.useState('snippets');
  const navigate = useNavigate();

  return keycloak.authenticated ? (
    <styled.Layout {...rest}>
      <MenuProvider>
        <Header name={name} />
        <div className="main-window">
          {showMenu && (
            <NavBar>
              <div style={{ display: 'flex', flexDirection: 'row', marginLeft: '3.5%' }}>
                <NavBarItem
                  active={active === 'snippets'}
                  onClick={() => {
                    setActive('snippets');
                    navigate('/');
                  }}
                  className="item"
                >
                  Snippets
                </NavBarItem>
                <NavBarItem
                  className="item"
                  active={active === 'admin'}
                  onClick={() => {
                    setActive('admin');
                    navigate('/admin');
                  }}
                >
                  Admin
                </NavBarItem>
              </div>
            </NavBar>
          )}
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
