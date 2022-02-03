import { OptionItem } from 'components';
import { LogicalOperator } from 'hooks';

export const logicalOperators = [
  new OptionItem('Contains', LogicalOperator.Contains),
  new OptionItem('Equals', LogicalOperator.Equals),
  new OptionItem('Not Equal', LogicalOperator.NotEqual),
  new OptionItem('Greater Than', LogicalOperator.GreaterThan),
  new OptionItem('Greater Than Or Equal', LogicalOperator.GreaterThanOrEqual),
  new OptionItem('Less Than', LogicalOperator.LessThan),
  new OptionItem('Less Than Or Equal', LogicalOperator.LessThanOrEqual),
];
