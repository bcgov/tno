import { OptionItem } from 'components/form';

export const reportDurations = [
  new OptionItem<number>('Last 7 days', 0),
  new OptionItem<number>('Last 14 days', 1),
  new OptionItem<number>('Current month', 2),
  new OptionItem<number>('Previous month', 3),
  new OptionItem<number>('Custom', 4),
];
