import { SubscriberFooter } from 'components/footer';
import { CustomSidebar } from 'components/sidebar/CustomSidebar';
import { UnauthenticatedHome } from 'features/login';
import React from 'react';
import { useProSidebar } from 'react-pro-sidebar';
import { Outlet } from 'react-router-dom';
import { useToastError } from 'store/hooks';
import { Show, SummonContext, useKeycloakWrapper } from 'tno-core';

import { LayoutErrorBoundary } from '.';
import * as styled from './styled';

export interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
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
export const DefaultLayout: React.FC<ILayoutProps> = ({ children, ...rest }) => {
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
