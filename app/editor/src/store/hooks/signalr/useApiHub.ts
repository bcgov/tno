import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import React from 'react';
import { useAppStore } from 'store/slices';
import { MessageTargetKey, Settings, useKeycloakWrapper } from 'tno-core';

const url = Settings.ApiPath + '/hub';
let hub: HubConnection | null = null;

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
  listen: (target: MessageTargetKey, newMethod: (...args: any[]) => void) => () => void;
  /** Add event listener and return function to remove listener. */
  useHubEffect: (target: MessageTargetKey, newMethod: (...args: any[]) => void) => void;
  /** Send a message to the hub */
  send: (target: string, args: any[]) => Promise<void>;
  /** Invoke a message to the hub */
  invoke: (target: string, args: any[]) => Promise<any>;
}

/**
 * useApiHub hook provides a SignalR implementation.
 * @param param0 Hook properties.
 * @returns Hook controller.
 */
export const useApiHub = (): IHubController => {
  const [{ hubState }, { addError, changeHubState }] = useAppStore();
  const auth = useKeycloakWrapper();

  React.useEffect(() => {
    if (!hub && auth.instance.token) {
      hub = new HubConnectionBuilder()
        .withUrl(url, {
          withCredentials: true,
          accessTokenFactory: () => auth.instance.token ?? '',
        })
        .configureLogging(LogLevel.Error)
        .withAutomaticReconnect()
        .build();
      hub.onclose(() => {
        changeHubState(hub!.state);
      });
      hub.onreconnected(() => {
        changeHubState(hub!.state);
      });
      hub.onreconnecting(() => {
        changeHubState(hub!.state);
      });
      changeHubState(HubConnectionState.Connecting);
      hub
        .start()
        .then(() => {
          changeHubState(hub!.state);
        })
        .catch((error) => {
          changeHubState(hub!.state);
          console.error('signalR', error);
          addError({
            statusText: error.errorType,
            message: error.message,
          });
        });
    }
  }, [addError, auth.instance.token, changeHubState]);

  return {
    state: hubState,
    send: async (target: string, args: any[]) => {
      if (hub?.state === HubConnectionState.Connected) await hub.send(target, ...args);
    },
    invoke: async (target: string, args: any[]) => {
      if (hub?.state === HubConnectionState.Connected) await hub.invoke(target, ...args);
    },
    start: () => hub?.start() ?? Promise.resolve(),
    on: (target: string, callback: (...args: any[]) => void) => hub?.on(target, callback),
    off: (target: string, callback: (...args: any[]) => void) => hub?.off(target, callback),
    stop: () => hub?.stop() ?? Promise.resolve(),
    listen: (target: MessageTargetKey, callback: (...args: any[]) => void) => {
      hub?.on(target, callback);
      return () => {
        hub?.off(target, callback);
      };
    },
    useHubEffect: (target: MessageTargetKey, callback: (...args: any[]) => void) => {
      React.useEffect(() => {
        hub?.on(target, callback);
        return () => {
          hub?.off(target, callback);
        };
      }, [target, callback]);
    },
  };
};
