import { IOptionItem, OptionItem } from 'components/form';

export const serviceTypes = (ingestType?: string) => {
  const values: IOptionItem<string>[] = [
    new OptionItem('Stream', 'stream'),
    new OptionItem('Clip', 'clip'),
    new OptionItem('Tuner', 'tuner'),
  ];

  if (ingestType === 'Video') values.push(new OptionItem('Raspberry Pi', 'RPi'));

  return values;
};
