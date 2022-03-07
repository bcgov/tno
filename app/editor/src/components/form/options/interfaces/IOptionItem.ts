export interface IOptionItem<T extends string | number = string | number> {
  discriminator: 'IOption';
  label: string;
  value?: T;
  isEnabled: boolean;
}
