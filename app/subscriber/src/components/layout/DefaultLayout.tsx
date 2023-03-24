import { CustomSidebar } from 'components/sidebar/CustomSidebar';
import { SelectableMenuItems } from 'components/sidebar/SelectableMenuItems';
import { UnauthenticatedHome } from 'features/home';
import { SubscriberFooter } from 'features/home/SubscriberFooter';
import React from 'react';
import { FaAngleDoubleLeft, FaAngleDoubleRight } from 'react-icons/fa';
import { Menu, Sidebar, useProSidebar } from 'react-pro-sidebar';
import { Outlet } from 'react-router-dom';
import { useToastError } from 'store/hooks';
import { Row, Show, SummonContext, useKeycloakWrapper } from 'tno-core';

import { LayoutErrorBoundary } from '.';
import { MenuItemNames } from './constants/MenuItemNames';
import * as styled from './styled';

export interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Site name to display in header.
   */
  name: string;
  /**
   * Whether or not the sidebar is collapsed.
   */
  collapsed?: boolean;
}

/**
 * DefaultLayout provides a div structure to organize the page for users once Keycloak has initialized.
 * @param param0 Component properties.
 * @returns DefaultLayout component.
 */
export const DefaultLayout: React.FC<ILayoutProps> = ({ name, children, ...rest }) => {
  const keycloak = useKeycloakWrapper();
  const { setToken } = React.useContext(SummonContext);
  useToastError();

  React.useEffect(() => {
    keycloak.instance.onTokenExpired = () => {
      keycloak.instance
        .updateToken(240)
        .then(function (refreshed: boolean) {
          if (refreshed) {
            setToken(keycloak.instance.token);
          }
        })
        .catch(function () {
          keycloak.instance.logout();
        });
    };

    return () => {
      keycloak.instance.onTokenExpired = undefined;
    };
  }, [keycloak, setToken]);

  const { collapsed } = useProSidebar();

  return (
    <styled.Layout collapsed={collapsed} {...rest}>
      <Show visible={keycloak.authenticated}>
        <div className="grid-container">
          <div>
            <CustomSidebar />
          </div>
          <div className="main-contents">
            <main>
              <LayoutErrorBoundary>
                <Outlet />
              </LayoutErrorBoundary>
            </main>
          </div>
        </div>
      </Show>
      <Show visible={!keycloak.authenticated}>
        <div className="main-window">
          <main>
            <UnauthenticatedHome />
            <SubscriberFooter />
          </main>
        </div>
      </Show>
    </styled.Layout>
  );
};
