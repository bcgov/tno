import { IOptionItem, OptionItem } from '../components';
import { IUserModel } from '../hooks';

export const sortUser = <T extends IUserModel>(a: T, b: T) => {
  if (a.username < b.username) return -1;
  if (a.username > b.username) return 1;
  if (a.displayName < b.displayName) return -1;
  if (a.displayName > b.displayName) return 1;
  return 0;
};

const displayName = (user: IUserModel) => {
  if (user.displayName.toLowerCase() === user.username.toLowerCase()) return user.displayName;
  return `${user.displayName} [${user.username}]`;
};

export const getUserOptions = <T extends IUserModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (u) => new OptionItem(displayName(u), u.id, u.isEnabled),
) => {
  return prepend.concat([...items].sort(sortUser).map(map));
};
