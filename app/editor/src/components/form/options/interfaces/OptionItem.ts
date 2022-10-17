import { IOptionItem } from '.';

export class OptionItem<T extends string | number | undefined = string | number | undefined>
  implements IOptionItem<T>
{
  discriminator: 'IOption';
  label: string;
  value: T;
  isEnabled: boolean;

  constructor(label: string, value: T, isEnabled: boolean = true) {
    this.discriminator = 'IOption';
    this.label = label;
    this.value = value;
    this.isEnabled = isEnabled;
  }

  static create<T extends string | number>(
    label: string,
    value: T,
    isEnabled: boolean = true,
  ): IOptionItem<T> {
    return new OptionItem(label, value, isEnabled);
  }

  toInterface(): IOptionItem<T> {
    return {
      discriminator: this.discriminator,
      label: this.label,
      value: this.value,
      isEnabled: this.isEnabled,
    };
  }
}
