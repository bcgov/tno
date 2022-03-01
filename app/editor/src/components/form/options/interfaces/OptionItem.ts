import { IOptionItem } from '.';

export class OptionItem<T extends string | number = string | number> implements IOptionItem<T> {
  discriminator: 'IOption';
  label: string;
  value: T;

  constructor(label: string, value: T) {
    this.discriminator = 'IOption';
    this.label = label;
    this.value = value;
  }

  static create<T extends string | number>(label: string, value: T): IOptionItem<T> {
    return new OptionItem(label, value);
  }

  toInterface(): IOptionItem<T> {
    return {
      discriminator: this.discriminator,
      label: this.label,
      value: this.value,
    };
  }
}
