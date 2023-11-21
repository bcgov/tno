import { SubscriberFooter } from 'components/footer';
import { CustomSidebar } from 'components/sidebar/CustomSidebar';
import { UnauthenticatedHome, UserInfo } from 'features/login';
import React from 'react';
import { useProSidebar } from 'react-pro-sidebar';
import { Outlet } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useToastError } from 'store/hooks';
import { useApiHub } from 'store/hooks/signalr';
import {
  IReportMessageModel,
  MessageTargetName,
  Show,
  SummonContext,
  useKeycloakWrapper,
  useWindowSize,
} from 'tno-core';

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
  const hub = useApiHub();

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

  hub.useHubEffect(MessageTargetName.ReportStatus, async (message: IReportMessageModel) => {
    // Report has been updated, go fetch latest.
    try {
      toast.info(`Report "${message.subject}" has been sent out by email.`);
    } catch {}
  });

  const { collapsed, collapseSidebar } = useProSidebar();
  const { width } = useWindowSize();

  // check if width is mobile and if so collapse the sidebar or when window is being resized
  React.useEffect(() => {
    if (width && width < 768 && !collapsed) {
      collapseSidebar();
    }
  }, [width, collapseSidebar, collapsed]);

  return (
    <styled.Layout collapsed={collapsed} {...rest}>
      <UserInfo />
      <Show visible={keycloak.authenticated}>
        <Show visible={keycloak.hasClaim()}>
          <div className="grid-container">
            <div className="nav-bar">
              <CustomSidebar />
            </div>
            <div className="main-contents">
              <LayoutErrorBoundary>
                <main>
                  <Outlet />
                </main>
                <SubscriberFooter />
              </LayoutErrorBoundary>
            </div>
          </div>
        </Show>
        <Show visible={!keycloak.hasClaim()}>
          <div className="main-contents">
            <LayoutErrorBoundary>
              <main>
                <Outlet />
              </main>
              <SubscriberFooter />
            </LayoutErrorBoundary>
          </div>
        </Show>
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
