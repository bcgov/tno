import { OptionItem } from 'components/form';
import { DataSourceScheduleTypeName } from 'hooks/api-editor';

export const scheduleTypeOptions = [
  new OptionItem('None', DataSourceScheduleTypeName.None),
  new OptionItem('Continuous', DataSourceScheduleTypeName.Continuous),
  new OptionItem('Daily', DataSourceScheduleTypeName.Daily),
  new OptionItem('Advanced', DataSourceScheduleTypeName.Advanced),
];
