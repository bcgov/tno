export interface IOptionItem<T extends string | number | undefined = string | number | undefined> {
  discriminator: 'IOption';
  label: string | React.ReactElement;
  value?: T;
  isEnabled: boolean;
}
