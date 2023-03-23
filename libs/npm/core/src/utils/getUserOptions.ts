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

/**
 * Sorts provided items into options.
 * @param items An array of items to return as sorted options.
 * @param prepend An array of options to prepend to array.
 * @param map How to map the items to options.
 * @param sort How to sort items.
 * @returns An array of options.
 */
export const getUserOptions = <T extends IUserModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (u) => new OptionItem(displayName(u), u.id, u.isEnabled),
  sort: (a: T, b: T) => number = sortUser,
) => {
  return prepend.concat([...items].sort(sort).map(map));
};
