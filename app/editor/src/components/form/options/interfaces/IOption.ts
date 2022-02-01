export interface IOption {
  discriminator: 'IOption';
  label: string;
  value?: string | number;
}

export class Option {
  static create(label: string, value: string | number): IOption {
    return {
      discriminator: 'IOption',
      label,
      value,
    };
  }
}
