import { IOptionItem, OptionItem } from 'components/form';

export const getEnumStringOptions = (enumerable: any, prepend: IOptionItem[] = []) => {
  var items: string[] = Object.values(enumerable);
  return prepend.concat([...items].map((i) => new OptionItem(i, i.toLowerCase())));
};
