import 'react-toastify/dist/ReactToastify.css';
import 'react-tooltip/dist/react-tooltip.css';
import 'prismjs';

import { ReactKeycloakProvider } from '@react-keycloak/web';
import { LayoutAnonymous } from 'components/layout';
import { UploadContextWrapper } from 'features/content';
import { AppRouter } from 'features/router';
import Keycloak from 'keycloak-js';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { createKeycloakInstance, Loading, Show, useKeycloakEventHandler } from 'tno-core';

const appName = 'Media Monitoring Insights';

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
          <UploadContextWrapper>
            <AppRouter name={appName} />
          </UploadContextWrapper>
        </ReactKeycloakProvider>
      </Show>
      <Show visible={!keycloak}>
        <LayoutAnonymous name={appName}>
          <Loading />
        </LayoutAnonymous>
      </Show>
      <ToastContainer />
      <Tooltip variant="info" id="main-tooltip" place="top" float />
      <Tooltip variant="info" id="main-tooltip-right" place="right" float />
    </BrowserRouter>
  );
}

export default App;
