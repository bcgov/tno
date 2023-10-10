import { defaultSchedule } from '../constants';

export const createSchedule = (name: string, description: string) => {
  return { ...defaultSchedule, name, description };
};
