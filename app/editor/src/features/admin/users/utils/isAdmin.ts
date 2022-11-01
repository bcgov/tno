import { IUserModel } from 'hooks';

/**
 * Check whether the user is an admin or not.
 * @param user the user to check
 * @returns  true if the user is an admin, false otherwise
 */
export const isAdmin = (user: IUserModel) => {
  return user.roles?.includes('administrator');
};
