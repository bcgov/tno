import { ReactKeycloakProvider } from '@react-keycloak/web';
import { NavMenu } from 'components';
import { AppRouter } from 'components/router';
import { KeycloakInstance } from 'keycloak-js';
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import ReactTooltip from 'react-tooltip';
import {
  Layout,
  LayoutAnonymous,
  Loading,
  SummonProvider,
  useKeycloakEventHandler,
} from 'tno-core';
import { createKeycloakInstance } from 'utils';

function App() {
  const [keycloak, setKeycloak] = React.useState<KeycloakInstance>();
  const keycloakEventHandler = useKeycloakEventHandler();
  const name = "Today's News Online (Editor)";

  React.useEffect(() => {
    createKeycloakInstance().then((result) => {
      setKeycloak(result);
    });
  }, []);

  return keycloak ? (
    <SummonProvider>
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
        </BrowserRouter>

        <ReactTooltip id="main-tooltip" effect="float" type="light" place="top" />
        <ReactTooltip id="main-tooltip-right" effect="solid" type="light" place="right" />
      </ReactKeycloakProvider>
    </SummonProvider>
  ) : (
    <LayoutAnonymous name={name}>
      <Loading />
    </LayoutAnonymous>
  );
}

export default App;
