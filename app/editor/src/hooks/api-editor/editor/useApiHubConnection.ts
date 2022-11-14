import { HubConnectionBuilder } from '@microsoft/signalr';
import React from 'react';

export const useApiHubConnection = () => {
  const controller = React.useRef({
    getConnection: () => {
      // TODO: here just for testing only for dev environment, and will be updated later on
      const url = 'https://tno-dev.apps.silver.devops.gov.bc.ca/api/workOrderHub';
      const connection = new HubConnectionBuilder()
        .withUrl(url, { withCredentials: false }) // process.env.REACT_APP_API_URL + '/workOrderHub'
        .withAutomaticReconnect()
        .build();
      return connection;
    },
  }).current;

  return controller;
};
