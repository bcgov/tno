import { OptionItem } from 'components';
import { LogicalOperator } from 'hooks';

export const logicalOperators = [
  new OptionItem('Contains', LogicalOperator.Contains, true),
  new OptionItem('Equals', LogicalOperator.Equals),
  new OptionItem('Not Equal', LogicalOperator.NotEqual),
];
