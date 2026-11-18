import { IOptionItem } from '../components';

/**
 * Used to only display lookup options that are enabled within dropdowns.
 * @param options An array of options.
 * @param currentSelected The value currently selected.
 * @returns An array of enabled options and the currently select option.
 */
export const filterEnabledOptions = (
  options: IOptionItem[],
  currentSelected: string | number | undefined = undefined,
) => {
  return options.filter((item) => !item.isDisabled || item.value === currentSelected);
};
