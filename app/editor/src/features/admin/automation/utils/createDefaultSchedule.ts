import { type IAutomationScheduleModel } from '../interfaces';

export const createDefaultSchedule = (): IAutomationScheduleModel => ({
  id: 0,
  name: '',
  isEnabled: true,
  startAt: null,
  runOnWeekDays: [],
});
