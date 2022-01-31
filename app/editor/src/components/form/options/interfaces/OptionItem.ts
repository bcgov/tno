import { IOptionItem } from '.';

export class OptionItem implements IOptionItem {
  discriminator: 'IOption';
  label: string;
  value: string | number;

  constructor(label: string, value: string | number) {
    this.discriminator = 'IOption';
    this.label = label;
    this.value = value;
  }

  static create(label: string, value: string | number): IOptionItem {
    return new OptionItem(label, value);
  }

  toInterface(): IOptionItem {
    return {
      discriminator: this.discriminator,
      label: this.label,
      value: this.value,
    };
  }
}
