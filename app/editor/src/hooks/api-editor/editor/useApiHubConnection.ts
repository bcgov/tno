import { HubConnectionBuilder } from '@microsoft/signalr';
import React from 'react';

export const useApiHubConnection = () => {
  const controller = React.useRef({
    getConnection: () => {
      const connection = new HubConnectionBuilder()
        .withUrl(process.env.REACT_APP_API_URL + '/workOrderHub', { withCredentials: false })
        .withAutomaticReconnect()
        .build();
      return connection;
    },
  }).current;

  return controller;
};
