import './index.scss';
import '@bcgov/bc-sans/css/BCSans.css';

import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { store } from 'store';
import { ThemeProvider } from 'styled-components';
import { SummonProvider } from 'tno-core';

import App from './App';
import css from './css/_variables.module.scss';
import * as serviceWorker from './serviceWorker';

const Index = () => {
  return (
    <React.StrictMode>
      <ThemeProvider theme={{ css }}>
        <Provider store={store}>
          <SummonProvider>
            <App />
          </SummonProvider>
        </Provider>
      </ThemeProvider>
    </React.StrictMode>
  );
};

ReactDOM.render(<Index />, document.getElementById('root'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
