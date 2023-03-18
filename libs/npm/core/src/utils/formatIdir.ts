/**
 * Function that removes '@idir' tag from username coming in from keycloak
 * @param {string} username - The username to remove '@idir' from
 * returns The formatted username string
 * */
export const formatIdirUsername = (userName?: string) => {
  if (userName?.includes('@idir')) return userName.split('@').shift();
  return userName;
};
