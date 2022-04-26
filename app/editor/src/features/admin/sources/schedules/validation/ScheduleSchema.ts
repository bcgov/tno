import { object, string } from 'yup';

export const ScheduleSchema = object().shape({
  name: string().required(),
  startAt: string().required(),
  stopAt: string().required(),
});
