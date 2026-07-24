import { type IOptionItem } from 'tno-core';
import { object, string } from 'yup';

import { createOption } from '../utils';
import { actionTypeOptions, contentFieldOptions } from './defaultAutomationProfile';

export const AutomationSchema = object({
  name: string().required('Name is required'),
});

export const stepTargetOptions: IOptionItem[] = [
  createOption('Content', 'content'),
  createOption('Run once at start', 'start'),
  createOption('Run once at end', 'end'),
];

/** The only valid target when the profile does not include a filter. */
export const noneTargetOptions: IOptionItem[] = [createOption('None', 'none')];

export const actionTypeOptionItems: IOptionItem[] = actionTypeOptions.map((option) =>
  createOption(option.label, option.value),
);

export const contentFieldOptionItems: IOptionItem[] = contentFieldOptions.map((option) =>
  createOption(option.label, option.value),
);
