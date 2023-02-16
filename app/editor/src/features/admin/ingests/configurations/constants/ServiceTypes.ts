import { IOptionItem, OptionItem } from 'tno-core';

export const serviceTypes = (ingestType?: string) => {
  const values: IOptionItem<string>[] = [
    new OptionItem('Stream', 'stream'),
    new OptionItem('Clip', 'clip'),
    new OptionItem('Tuner', 'tuner'),
  ];

  if (ingestType === 'Video') {
    values.push(new OptionItem('Raspberry Pi', 'RPi'));
    values.push(new OptionItem('HDMI', 'HDMI'));
  }
  return values;
};
