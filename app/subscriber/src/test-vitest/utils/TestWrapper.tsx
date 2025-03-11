import { configureStore } from '@reduxjs/toolkit';
import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';

import variables from '../../css/_variables.module.scss';

// Theme using exported CSS variables
const mockTheme = {
  css: {
    bkWhite: variables.bkWhite,
    btnGrayColor: variables.btnGrayColor,
    btnBkSuccess: variables.btnBkSuccess,
    btnBkWarn: variables.btnBkWarn,
    btnBkError: variables.btnBkError,
    btnBkPrimary: variables.btnBkPrimary,
    btnSuccessColor: variables.btnSuccessColor,
    btnWarnColor: variables.btnWarnColor,
    btnErrorColor: variables.btnErrorColor,
    btnPrimaryColor: variables.btnPrimaryColor,
    boxShadow: variables.boxShadow,
    linkPrimaryActiveColor: variables.linkPrimaryActiveColor,
  },
};

// create store
const mockStore = configureStore({
  reducer: {},
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: false,
    }),
});

interface Props {
  children: React.ReactNode;
}

export const TestWrapper: React.FC<Props> = ({ children }) => {
  return (
    <BrowserRouter>
      <Provider store={mockStore}>
        <ThemeProvider theme={mockTheme}>{children}</ThemeProvider>
      </Provider>
    </BrowserRouter>
  );
};
