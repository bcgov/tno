import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import React from 'react';

import { Settings } from '../constants';

export const useApiHubConnection = () => {
  const controller = React.useRef({
    getConnection: () => {
      const url = Settings.ApiPath + '/workOrderHub';
      const connection = new HubConnectionBuilder()
        .withUrl(url, { withCredentials: false })
        .configureLogging(LogLevel.Error)
        .withAutomaticReconnect()
        .build();
      return connection;
    },
  }).current;

  return controller;
};
