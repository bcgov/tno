import { type IOptionItem } from 'tno-core';

export const createOption = (label: string, value: string | number): IOptionItem => ({
  label,
  value,
  discriminator: 'IOption',
  isDisabled: false,
});
