import { IOptionItem, OptionItem } from '../components';

/**
 * Convert enum object into an array of OptionItem.
 * @param enumerable Object that is enumerable.
 * @param prepend Prepend any specified options.
 * @returns An array of OptionItem.
 */
export const getEnumStringOptions = (
  enumerable: { [s: number]: string | number },
  prepend: IOptionItem[] = [],
) => {
  const items = Object.values(enumerable);
  return prepend.concat(
    [...items].map(
      (i) => new OptionItem(typeof i === 'string' ? i.replace(/([A-Z])/g, ' $1') : i.toString(), i),
    ),
  );
};
