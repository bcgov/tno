import { ReactKeycloakProvider } from '@react-keycloak/web';
import { NavMenu } from 'components';
import { Layout } from 'components/layout';
import { AppRouter } from 'components/router';
import { KeycloakInstance } from 'keycloak-js';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import ReactTooltip from 'react-tooltip';
import { LayoutAnonymous, Loading, useKeycloakEventHandler } from 'tno-core';
import { createKeycloakInstance } from 'utils';

function App() {
  const [keycloak, setKeycloak] = React.useState<KeycloakInstance>();
  const keycloakEventHandler = useKeycloakEventHandler();
  const name = 'TNO News Service';

  React.useEffect(() => {
    createKeycloakInstance().then((result) => {
      setKeycloak(result);
    });
  }, []);

  return keycloak ? (
    <ReactKeycloakProvider
      authClient={keycloak}
      LoadingComponent={
        <LayoutAnonymous name={name}>
          <Loading />
        </LayoutAnonymous>
      }
      onEvent={keycloakEventHandler(keycloak)}
    >
      <BrowserRouter>
        <Layout name={name}>{{ menu: <NavMenu />, router: <AppRouter /> }}</Layout>

        <ReactTooltip id="main-tooltip" effect="float" type="light" place="top" />
        <ReactTooltip id="main-tooltip-right" effect="solid" type="light" place="right" />
      </BrowserRouter>
    </ReactKeycloakProvider>
  ) : (
    <LayoutAnonymous name={name}>
      <Loading />
    </LayoutAnonymous>
  );
}

export default App;
