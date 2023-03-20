import { ITimeTrackingModel } from 'tno-core';

/**
 * Function that sums the total time of the time tracking records.
 * @param {ITimeTrackingModel[]} timeTracking - An array of time tracking records.
 * @returns The sum of all time tracking records
 */
export const getTotalTime = (timeTracking: ITimeTrackingModel[]) => {
  let count = 0;
  if (timeTracking.length === 0) {
    return 0;
  }
  timeTracking.forEach((t: ITimeTrackingModel) => (count += Number(t.effort)));
  return count;
};
