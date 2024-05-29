import { IOptionItem, OptionItem } from '../components';

interface IEnumStringOptions {
  // Prepend these items to the options.
  prepend?: IOptionItem[];
  // Append these items to the options.
  append?: IOptionItem[];
  // Split enum values before each capital letter (i.e. TestThisValue = Test This Value).
  splitOnCapital?: boolean;
}

/**
 * Convert enum object into an array of OptionItem.
 * @param enumerable Object that is enumerable.
 * @param prependOrOptions Prepend any specified options.
 * @returns An array of OptionItem.
 */
export const getEnumStringOptions = (
  enumerable: { [s: number]: string | number },
  prependOrOptions?: IOptionItem[] | IEnumStringOptions,
) => {
  const prepend: IOptionItem[] = Array.isArray(prependOrOptions) ? prependOrOptions : [];
  const options: IEnumStringOptions = Array.isArray(prependOrOptions)
    ? { splitOnCapital: true }
    : prependOrOptions ?? { splitOnCapital: true };
  const append: IOptionItem[] = options.append ? options.append : [];

  const items = Object.values(enumerable);
  return prepend
    .concat(
      [...items].map(
        (i) =>
          new OptionItem(
            typeof i === 'string'
              ? options.splitOnCapital
                ? i.replace(/([A-Z])/g, ' $1')
                : i
              : i.toString(),
            i,
          ),
      ),
    )
    .concat(append);
};
