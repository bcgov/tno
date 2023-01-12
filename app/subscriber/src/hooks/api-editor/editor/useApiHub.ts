import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import React from 'react';
import { useAppStore } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';

import { HubMethodName, Settings } from '../constants';

const url = Settings.ApiPath + '/hub';
let hub: HubConnection;

export interface IHubController {
  /** The state of the hub connection. */
  state: HubConnectionState;
  /** Start the connection. */
  start: () => Promise<void>;
  /** Add event listener. */
  on: (methodName: string, newMethod: (...args: any[]) => void) => void;
  /** Remove event listener. */
  off: (methodName: string, method: (...args: any[]) => void) => void;
  /** Stop the connection. */
  stop: () => Promise<void>;
  /** Add event listener and return function to remove listener. */
  listen: (methodName: HubMethodName, newMethod: (...args: any[]) => void) => () => void;
}

/**
 * useApiHub hook provides a SignalR implementation.
 * @param param0 Hook properties.
 * @returns Hook controller.
 */
export const useApiHub = (): IHubController => {
  const [state, setState] = React.useState(hub?.state ?? HubConnectionState.Disconnected);
  const [, app] = useAppStore();
  const auth = useKeycloakWrapper();

  React.useEffect(() => {
    if (hub === undefined) {
      hub = new HubConnectionBuilder()
        .withUrl(url, {
          withCredentials: true,
          accessTokenFactory: () => auth.instance.token ?? '',
        })
        .configureLogging(LogLevel.Error)
        .withAutomaticReconnect()
        .build();
      hub.onclose(() => {
        setState(hub.state);
      });
      hub.onreconnected(() => {
        setState(hub.state);
      });
      hub.onreconnecting(() => {
        setState(hub.state);
      });
      hub
        .start()
        .then(() => {
          setState(hub.state);
        })
        .catch((error) => {
          setState(hub.state);
          console.error('signalR', error);
          app.addError({
            statusText: error.errorType,
            message: error.message,
          });
        });
    }
  }, [app, auth]);

  return {
    state,
    start: () => hub.start(),
    on: (methodName: string, newMethod: (...args: any[]) => void) => hub.on(methodName, newMethod),
    off: (methodName: string, method: (...args: any[]) => void) => hub.off(methodName, method),
    stop: () => hub.stop(),
    listen: (methodName: HubMethodName, newMethod: (...args: any[]) => void) => {
      hub.on(methodName, newMethod);
      return () => {
        hub.off(methodName, newMethod);
      };
    },
  };
};
