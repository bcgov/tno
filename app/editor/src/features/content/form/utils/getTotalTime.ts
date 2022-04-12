import { ITimeTrackingModel } from 'hooks/api-editor';

export const getTotalTime = (timeTracking: ITimeTrackingModel[]) => {
  let count = 0;
  if (timeTracking.length === 0) {
    return 0;
  }
  timeTracking.forEach((t: ITimeTrackingModel) => (count += Number(t.effort)));
  return count;
};
