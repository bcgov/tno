import { HubConnectionBuilder } from '@microsoft/signalr';
import React from 'react';

export const useApiHubConnection = () => {
  const controller = React.useRef({
    getConnection: () => {
      const url = process.env.REACT_APP_API_URL + '/api/workOrderHub';
      console.log(`REACT_APP_API_URL: ${process.env.REACT_APP_API_URL}`);
      const connection = new HubConnectionBuilder()
        .withUrl(url, { withCredentials: false })
        .withAutomaticReconnect()
        .build();
      return connection;
    },
  }).current;

  return controller;
};
