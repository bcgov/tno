import { type IOptionItem } from 'tno-core';
import { object, string } from 'yup';

import { createOption } from '../utils';
import { contentFieldOptions } from './defaultAutomationProfile';

export const AutomationSchema = object({
  name: string().required('Name is required'),
});

export const contentFieldOptionItems: IOptionItem[] = contentFieldOptions.map((option) =>
  createOption(option.label, option.value),
);
