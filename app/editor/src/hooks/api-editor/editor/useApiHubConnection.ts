import signalR, { HubConnectionBuilder } from '@microsoft/signalr';
import React from 'react';

import { Settings } from '../constants';

export const useApiHubConnection = () => {
  const controller = React.useRef({
    getConnection: () => {
      const url = Settings.ApiPath + '/workOrderHub';
      console.log(`Settings.ApiPath: ${Settings.ApiPath}`);
      const connection = new HubConnectionBuilder()
        .withUrl(url, { withCredentials: false })
        .configureLogging(signalR.LogLevel.Debug)
        .withAutomaticReconnect()
        .build();
      return connection;
    },
  }).current;

  return controller;
};
