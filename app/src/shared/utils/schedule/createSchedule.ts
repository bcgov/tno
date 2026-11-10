import { defaultSchedule } from './defaultSchedule';

export const createSchedule = (name: string, description: string) => {
  return { ...defaultSchedule, name, description };
};
