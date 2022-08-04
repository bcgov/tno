import { IOptionItem, OptionItem } from 'components/form';

export const getEnumStringOptions = (
  enumerable: { [s: number]: string },
  prepend: IOptionItem[] = [],
) => {
  const items = Object.values(enumerable);
  return prepend.concat([...items].map((i) => new OptionItem(i, i.toLowerCase())));
};
