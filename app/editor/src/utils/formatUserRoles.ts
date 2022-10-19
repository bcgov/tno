import { IRoleModel } from 'hooks';

/**
 * Format the user roles array to a string for display purposes.
 * @param {IRoleModel[]} roles The array of user roles to format.
 * @returns A string of formatted user roles.
 */
export const formatUserRoles = (roles?: IRoleModel[]) => {
  let rolesNameArray: string[] = [];
  roles?.forEach((r) => rolesNameArray.push(r.name));
  return rolesNameArray.join(', ');
};
