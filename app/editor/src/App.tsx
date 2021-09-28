import { ReactKeycloakProvider } from '@react-keycloak/web';
import { Layout } from 'components/layout';
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
          <p>Loading</p>
        </Layout>
      }
      onEvent={keycloakEventHandler(keycloak)}
    >
      <Layout>
        <AppRouter />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
      </Layout>
    </ReactKeycloakProvider>
  ) : (
    <Layout>
      <p>Loading</p>
    </Layout>
  );
}

export default App;
