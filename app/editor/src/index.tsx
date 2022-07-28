import './index.scss';

import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { store } from 'store';
import { ThemeProvider } from 'styled-components';
import { SummonProvider } from 'tno-core';

import App from './App';
import css from './css/_variables.module.scss';
import * as serviceWorker from './serviceWorker';

/** TODO: Enable React.StrictMode when keycloak fix is released. */
const Index = () => {
  return (
    <ThemeProvider theme={{ css }}>
      <Provider store={store}>
        <SummonProvider>
          <App />
        </SummonProvider>
      </Provider>
    </ThemeProvider>
  );
};

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(<Index />);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
