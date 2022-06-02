/** function that removes '@idir' tag from username coming in from keycloak*/
export const formatIdirUsername = (userName?: string) => {
  if (userName?.includes('@idir')) return userName.split('@').shift();
  return userName;
};
