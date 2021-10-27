import { ReactKeycloakProvider } from '@react-keycloak/web';
import { Layout, Loading } from 'components';
import { AppRouter } from 'components/router';
import { KeycloakInstance } from 'keycloak-js';
import React from 'react';
import { createKeycloakInstance, keycloakEventHandler } from 'utils';

function App() {
  const [keycloak, setKeycloak] = React.useState<KeycloakInstance>();

  React.useEffect(() => {
    createKeycloakInstance().then((result) => {
      setKeycloak(result);
    });
  }, []);

  return keycloak ? (
    <ReactKeycloakProvider
      authClient={keycloak}
      LoadingComponent={
        <Layout>
          <Loading />
        </Layout>
      }
      onEvent={keycloakEventHandler(keycloak)}
    >
      <Layout authReady={true}>
        <AppRouter />
      </Layout>
    </ReactKeycloakProvider>
  ) : (
    <Layout>
      <Loading />
    </Layout>
  );
}

export default App;
