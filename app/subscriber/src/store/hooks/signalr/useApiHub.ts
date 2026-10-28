import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  IRetryPolicy,
  LogLevel,
} from '@microsoft/signalr';
import React from 'react';
import { useAppStore } from 'store/slices';
import { MessageTargetKey, Settings, useKeycloakWrapper } from 'tno-core';

const url = Settings.ApiPath + '/hub';
let hub: HubConnection | null = null;

/** Delay before each connection attempt; the last delay repeats so the hub never stops trying. */
const retryDelays = [0, 2000, 5000, 10000, 30000];
const retryDelay = (attempt: number) => retryDelays[Math.min(attempt, retryDelays.length - 1)];
const retryPolicy: IRetryPolicy = {
  nextRetryDelayInMilliseconds: (context) => retryDelay(context.previousRetryCount),
};

/**
 * Rooms this client has joined. Groups belong to a connection, so a reconnect (which may land on
 * another API pod) starts with none and they must be joined again.
 */
const rooms = new Set<string>();

const trackRoom = (target: string, args: any[]) => {
  const method = target.toLowerCase();
  if (method === 'joinroom') rooms.add(args[0]);
  else if (method === 'leaveroom') rooms.delete(args[0]);
};

const joinRooms = () => {
  rooms.forEach((room) =>
    hub?.send('JoinRoom', room).catch((error) => console.error('signalR', error)),
  );
};

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
        .withAutomaticReconnect(retryPolicy)
        .build();

      // Automatic reconnect only covers a connection that was established, so retry a failed start here.
      const start = (attempt: number) => {
        changeHubState(HubConnectionState.Connecting);
        hub!
          .start()
          .then(() => {
            changeHubState(hub!.state);
            joinRooms();
          })
          .catch((error) => {
            changeHubState(hub!.state);
            console.error('signalR', error);
            if (attempt === 0)
              addError({
                statusText: error.errorType,
                message: error.message,
              });
            setTimeout(() => start(attempt + 1), retryDelay(attempt + 1));
          });
      };

      hub.onclose((error) => {
        changeHubState(hub!.state);
        // Reconnecting never gives up, so an error here means the server closed the connection.
        if (error) start(1);
      });
      hub.onreconnected(() => {
        changeHubState(hub!.state);
        joinRooms();
      });
      hub.onreconnecting(() => {
        changeHubState(hub!.state);
      });
      start(0);
    }
  }, [addError, auth.instance.token, changeHubState]);

  return {
    state: hubState,
    send: async (target: string, args: any[]) => {
      trackRoom(target, args);
      if (hub?.state === HubConnectionState.Connected) await hub.send(target, ...args);
    },
    invoke: async (target: string, args: any[]) => {
      trackRoom(target, args);
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
