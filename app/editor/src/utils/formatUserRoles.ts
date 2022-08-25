import { IRoleModel } from 'hooks';

export const formatUserRoles = (roles?: IRoleModel[]) => {
  let rolesNameArray: string[] = [];
  roles?.forEach((r) => rolesNameArray.push(r.name));
  return rolesNameArray.join(', ');
};
