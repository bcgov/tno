import 'react-toastify/dist/ReactToastify.css';

import { ReactKeycloakProvider } from '@react-keycloak/web';
import { LayoutAnonymous } from 'components/layout';
import { UploadContextWrapper } from 'components/upload';
import { AppRouter } from 'features/router';
import Keycloak from 'keycloak-js';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import ReactTooltip from 'react-tooltip';
import { createKeycloakInstance, Loading, useKeycloakEventHandler } from 'tno-core';

const appName = 'Media Monitoring Insights & Analysis';

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
      {keycloak ? (
        <ReactKeycloakProvider
          authClient={keycloak}
          LoadingComponent={
            <LayoutAnonymous name={appName}>
              <Loading />
            </LayoutAnonymous>
          }
          onEvent={keycloakEventHandler(keycloak)}
        >
          <UploadContextWrapper>
            <AppRouter name={appName} />
          </UploadContextWrapper>
        </ReactKeycloakProvider>
      ) : (
        <LayoutAnonymous name={appName}>
          <Loading />
        </LayoutAnonymous>
      )}
      <ToastContainer />
      <ReactTooltip id="main-tooltip" effect="float" type="light" place="top" />
      <ReactTooltip id="main-tooltip-right" effect="solid" type="light" place="right" />
    </BrowserRouter>
  );
}

export default App;
