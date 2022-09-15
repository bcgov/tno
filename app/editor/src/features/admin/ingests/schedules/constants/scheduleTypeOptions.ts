import { OptionItem } from 'components/form';
import { ScheduleTypeName } from 'hooks/api-editor';

export const scheduleTypeOptions = [
  new OptionItem('None', ScheduleTypeName.None),
  new OptionItem('Continuous', ScheduleTypeName.Continuous),
  new OptionItem('Daily', ScheduleTypeName.Daily),
  new OptionItem('Advanced', ScheduleTypeName.Advanced),
];
