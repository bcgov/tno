import { type IOptionItem } from 'tno-core';

/**
 * One select option. Reference data that has been disabled is passed with `isDisabled` so it
 * stays in the list — a profile that already references it still renders its name instead of an
 * empty picker — while react-select refuses to let it be chosen again.
 */
export const createOption = (
  label: string,
  value: string | number,
  isDisabled = false,
): IOptionItem => ({
  label,
  value,
  discriminator: 'IOption',
  isDisabled,
});
