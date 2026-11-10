import './index.scss';

import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { ThemeProvider } from 'styled-components';
import { SummonProvider } from 'tno-core';

import { store } from '@/subscriber/store';

import App from './App';
import css from './css/_variables.module.scss';

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
