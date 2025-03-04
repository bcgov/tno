import { configureStore } from '@reduxjs/toolkit';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';

// Create a mock store with initial state
const mockStore = configureStore({
  reducer: {
    app: (
      state = {
        userInfo: {
          id: 3,
          key: '5edb4014-8feb-47ca-af53-25ed94312088',
          username: 'admin',
          email: 'admin@local.com',
          displayName: 'Admin User',
          firstName: 'Admin',
          lastName: 'User',
          isEnabled: true,
          status: 'Activated',
          roles: ['editor', 'subscriber', 'administrator'],
        },
        requests: [],
        isReady: true,
      },
    ) => state,
    content: (
      state = {
        items: [],
        page: 1,
        total: 0,
        quantity: 10,
      },
    ) => state,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    }),
});

// Mock theme
const theme = {
  css: {
    primaryColor: '#1a5a96',
    secondaryColor: '#606060',
    accentColor: '#fcba19',
    backgroundColor: '#f2f2f2',
    textColor: '#313132',
    dangerColor: '#d8292f',
    warningColor: '#fcba19',
    successColor: '#2e8540',
    infoColor: '#1a5a96',
    cancelledColor: '#d8292f',
    disabledColor: '#959595',
    linkColor: '#1a5a96',
    linkHoverColor: '#0000ee',
    borderColor: '#d3d3d3',
    tableStripeColor: '#f2f2f2',
  },
};

interface Props {
  children: React.ReactNode;
}

export const TestWrapper: React.FC<Props> = ({ children }) => {
  return (
    <BrowserRouter>
      <Provider store={mockStore}>
        <ThemeProvider theme={theme}>{children}</ThemeProvider>
      </Provider>
    </BrowserRouter>
  );
};
