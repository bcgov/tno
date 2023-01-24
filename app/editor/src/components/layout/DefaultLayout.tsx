import { UnauthenticatedHome } from 'features/home';
import { UserInfo } from 'features/login';
import { NavBar } from 'features/navbar';
import {
  HubMethodName,
  IWorkOrderModel,
  IWorkOrderToast,
  useApiHub,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'hooks';
import React from 'react';
import { Link, Outlet } from 'react-router-dom';
import { toast } from 'react-toastify';
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
  const { setToken } = React.useContext(SummonContext);
  var hub = useApiHub();
  useToastError();

  const [toastIds, setToastIds] = React.useState<IWorkOrderToast[]>([]);

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

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderModel) => {
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

  React.useEffect(() => {
    return hub.listen(HubMethodName.WorkOrder, onWorkOrder);
  }, [onWorkOrder, hub]);

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
            <UnauthenticatedHome />
          </main>
        </div>
      </Show>
    </styled.Layout>
  );
};
