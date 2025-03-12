import { configureStore } from '@reduxjs/toolkit';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';

import variables from '../../css/_variables.module.scss';

// Create a mock store with initial state
const mockStore = configureStore({
  reducer: {},
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    }),
});

// Theme using exported CSS variables
const theme = {
  css: {
    primaryColor: variables.primaryColor,
    secondaryColor: variables.secondaryVariantColor,
    accentColor: variables.accentColor,
    backgroundColor: variables.backgroundColor,
    textColor: variables.textColor,
    dangerColor: variables.dangerColor,
    warningColor: variables.warning,
    successColor: variables.completedColor,
    infoColor: variables.info,
    cancelledColor: variables.cancelledColor,
    disabledColor: variables.secondary,
    linkColor: variables.linkPrimaryColor,
    linkHoverColor: variables.linkPrimaryHoverColor,
    borderColor: variables.borderColor,
    tableStripeColor: variables.tableEvenRow,
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
