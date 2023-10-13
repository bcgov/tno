import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import React from 'react';
import { useAppStore } from 'store/slices';
import { MessageTargetName, Settings, useKeycloakWrapper } from 'tno-core';

const url = Settings.ApiPath + '/hub';
let hub: HubConnection;

export interface IHubController {
  /** The state of the hub connection. */
  state: HubConnectionState;
  /** Start the connection. */
  start: () => Promise<void>;
  /** Add event listener. */
  on: (target: string, newMethod: (...args: any[]) => void) => void;
  /** Remove event listener. */
  off: (target: string, method: (...args: any[]) => void) => void;
  /** Stop the connection. */
  stop: () => Promise<void>;
  /** Add event listener and return function to remove listener. */
  listen: (target: MessageTargetName, newMethod: (...args: any[]) => void) => () => void;
  /** Add event listener and return function to remove listener. */
  useHubEffect: (target: MessageTargetName, newMethod: (...args: any[]) => void) => void;
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
    on: (target: string, callback: (...args: any[]) => void) => hub.on(target, callback),
    off: (target: string, callback: (...args: any[]) => void) => hub.off(target, callback),
    stop: () => hub.stop(),
    listen: (target: MessageTargetName, callback: (...args: any[]) => void) => {
      hub.on(target, callback);
      return () => {
        hub.off(target, callback);
      };
    },
    useHubEffect: (target: MessageTargetName, callback: (...args: any[]) => void) => {
      React.useEffect(() => {
        hub.on(target, callback);
        return () => {
          hub.off(target, callback);
        };
      }, [target, callback]);
    },
  };
};
