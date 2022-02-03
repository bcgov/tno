export interface IOptionItem {
  discriminator: 'IOption';
  label: string;
  value?: string | number;
}
