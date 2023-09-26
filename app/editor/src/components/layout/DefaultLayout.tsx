import { UnauthenticatedHome } from 'features/home';
import { UserInfo } from 'features/login';
import { NavBar } from 'features/navbar';
import React from 'react';
import { Link, Outlet, useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { HubMethodName, IWorkOrderToast, useApiHub, useToastError } from 'store/hooks';
import {
  Button,
  Header,
  IWorkOrderMessageModel,
  Show,
  SummonContext,
  useKeycloakWrapper,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { LayoutErrorBoundary } from '.';
import * as styled from './styled';

interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Site name to display in header.
   */
  name: string;
  /** Whether the navbar is visible. */
  showNav?: boolean;
}

/**
 * DefaultLayout provides a div structure to organize the page for users once Keycloak has initialized.
 * @param param0 Component properties.
 * @returns DefaultLayout component.
 */
const DefaultLayout: React.FC<ILayoutProps> = ({
  name,
  showNav: initShowNav,
  children,
  ...rest
}) => {
  const keycloak = useKeycloakWrapper();
  const { setToken } = React.useContext(SummonContext);
  var hub = useApiHub();
  useToastError();
  const [searchParams] = useSearchParams({ showNav: 'true' });

  const [toastIds, setToastIds] = React.useState<IWorkOrderToast[]>([]);
  const showNav = initShowNav ?? searchParams.get('showNav') === 'true';

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

  const onWorkOrderMessage = React.useCallback(
    (workOrder: IWorkOrderMessageModel) => {
      if (workOrder.workType === WorkOrderTypeName.FileRequest) {
        const toastId = toastIds.find((t) => t.workOrderId === workOrder.id);
        if (toastId) {
          toast.dismiss(toastId.toastId);
          setToastIds((state) => state.filter((t) => t.toastId !== toastId?.toastId));
        }

        if (workOrder.status === WorkOrderStatusName.InProgress) {
          const id = toast.loading(
            `File copy request in progress. File: ${workOrder.configuration.path}`,
            {
              closeButton: true,
            },
          );
          setToastIds((state) => [
            ...state,
            {
              toastId: id,
              workOrderId: workOrder.id,
            },
          ]);
        } else if (workOrder.status === WorkOrderStatusName.Completed) {
          toast.success(() => (
            <div>
              File copy request complete: File:{' '}
              <Link
                to={`/storage/locations/1?path=/_tmp/${workOrder.configuration.path}`} // TODO: shouldn't hardcode location.
              >
                {workOrder.configuration.path}
              </Link>
            </div>
          ));
        } else {
          toast.error(`File copy request failed: File: ${workOrder.configuration.path}`);
        }
      }
    },
    [toastIds],
  );

  hub.useHubEffect(HubMethodName.WorkOrder, onWorkOrderMessage);

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
          <LayoutErrorBoundary>
            <Show visible={showNav}>
              <NavBar />
            </Show>
            <main>
              <Outlet />
            </main>
          </LayoutErrorBoundary>
        </div>
      </Show>
      <Show visible={!keycloak.authenticated}>
        <div className="main-window">
          <main style={{ backgroundColor: '#f2f2f2' }}>
            <UnauthenticatedHome />
          </main>
        </div>
      </Show>
    </styled.Layout>
  );
};

export default DefaultLayout;
