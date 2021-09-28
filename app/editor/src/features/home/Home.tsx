import { useKeycloakWrapper } from 'hooks';

export const Home = () => {
  return (
    <div>
      <p>Home</p>
      <LoginLogout />
    </div>
  );
};

const LoginLogout = () => {
  const keycloak = useKeycloakWrapper();

  return (
    <>
      {keycloak.authenticated ? (
        <button onClick={() => keycloak.instance.logout()}>Logout</button>
      ) : (
        <button onClick={() => keycloak.instance.login()}>Login</button>
      )}
    </>
  );
};
