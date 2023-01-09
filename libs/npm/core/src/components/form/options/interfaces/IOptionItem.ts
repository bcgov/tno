export interface IOptionItem<T extends string | number | undefined = string | number | undefined> {
  discriminator: 'IOption';
  label: string;
  value?: T;
  isEnabled: boolean;
}
