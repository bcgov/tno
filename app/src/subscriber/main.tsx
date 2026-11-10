import './index.scss';
// Evaluate all shared code (and its third-party CSS) before the design's own modules,
// as when it was the tno-core package. styled-components orders rules by definition order.
import '@/shared';

import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { ThemeProvider } from 'styled-components';

import { SummonProvider } from '@/shared';
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
