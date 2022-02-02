import { IOptionItem } from '.';

export class OptionItem implements IOptionItem {
  discriminator: 'IOption';
  label: string;
  value: string | number;
  selected: boolean;

  constructor(label: string, value: string | number, selected: boolean = false) {
    this.discriminator = 'IOption';
    this.label = label;
    this.value = value;
    this.selected = selected;
  }

  static create(label: string, value: string | number, selected: boolean = false): IOptionItem {
    return new OptionItem(label, value, selected);
  }
}
