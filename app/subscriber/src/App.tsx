import 'react-toastify/dist/ReactToastify.css';
import 'react-tooltip/dist/react-tooltip.css';

import isPropValid from '@emotion/is-prop-valid';
import { ReactKeycloakProvider } from '@react-keycloak/web';
import { ContentListProvider } from 'components/content-list/ContentListContext';
import { LayoutAnonymous } from 'components/layout';
import { AppRouter } from 'features/router';
import { SearchPageProvider } from 'features/search-page';
import { fixTooltipResizeObserver } from 'features/utils';
import Keycloak from 'keycloak-js';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { StyleSheetManager } from 'styled-components';
import { createKeycloakInstance, Loading, Show, useKeycloakEventHandler } from 'tno-core';

const appName = 'Media Monitoring Insights';
fixTooltipResizeObserver();

// This implements the default behavior from styled-components v5
function shouldForwardProp(propName: any, target: any) {
  if (typeof target === 'string') {
    // For HTML elements, forward the prop if it is a valid HTML attribute
    return isPropValid(propName);
  }
  // For other elements, forward all props
  return true;
}

function App() {
  const keycloakEventHandler = useKeycloakEventHandler();

  const [keycloak, setKeycloak] = React.useState<Keycloak>();

  React.useEffect(() => {
    createKeycloakInstance().then((result) => {
      setKeycloak(result);
    });
  }, []);

  return (
    <BrowserRouter>
      <StyleSheetManager shouldForwardProp={shouldForwardProp}>
        <Show visible={!!keycloak}>
          <ReactKeycloakProvider
            initOptions={{ pkceMethod: 'S256', checkLoginIframe: false }}
            authClient={keycloak!}
            LoadingComponent={
              <LayoutAnonymous name={appName}>
                <Loading />
              </LayoutAnonymous>
            }
            onEvent={keycloakEventHandler(keycloak!)}
          >
            <ContentListProvider>
              <SearchPageProvider>
                <AppRouter name={appName} />
              </SearchPageProvider>
            </ContentListProvider>
          </ReactKeycloakProvider>
        </Show>
        <Show visible={!keycloak}>
          <LayoutAnonymous name={appName}>
            <Loading />
          </LayoutAnonymous>
        </Show>
        <ToastContainer />
        <Tooltip
          opacity={1}
          style={{
            backgroundColor: '#FFFFCC',
            color: 'black',
            boxShadow: '0 0 8px #464545',
            zIndex: '999',
          }}
          id="main-tooltip"
          place="top"
        />
        <Tooltip variant="light" id="main-tooltip-right" place="right" />
      </StyleSheetManager>
    </BrowserRouter>
  );
}

export default App;
