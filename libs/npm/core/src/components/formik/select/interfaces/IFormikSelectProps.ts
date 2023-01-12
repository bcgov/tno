import { Props } from 'react-select';

import { IOption } from './IOption';

export interface IFormikSelectProps extends Props {
  label?: string;
  options: IOption[];
}
