import { IOptionItem } from '.';

export class OptionItem<T extends string | number | undefined = string | number | undefined>
  implements IOptionItem<T>
{
  discriminator: 'IOption';
  label: string | React.ReactElement;
  value: T;
  isDisabled: boolean;

  constructor(label: string | React.ReactElement, value: T, isDisabled: boolean = false) {
    this.discriminator = 'IOption';
    this.label = label;
    this.value = value;
    this.isDisabled = isDisabled;
  }

  static create<T extends string | number>(
    label: string | React.ReactElement,
    value: T,
    isDisabled: boolean = false,
  ): IOptionItem<T> {
    return new OptionItem(label, value, isDisabled);
  }

  toInterface(): IOptionItem<T> {
    return this as IOptionItem<T>;
  }
}

OptionItem.prototype.toString = function () {
  return `${this.value}`;
};
