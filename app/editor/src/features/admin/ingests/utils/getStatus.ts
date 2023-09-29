import moment from 'moment';
import { IIngestModel } from 'tno-core';

export const getStatus = (data: IIngestModel) => {
  if (data.failedAttempts >= data.retryLimit) return 'Failed';
  else if (!data.isEnabled) return 'Disabled';
  else if (!data.lastRanOn) return 'Never Run';

  const lastDelay = moment();
  const lastRanOn = moment(data.lastRanOn).add(5, 'minutes');
  return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Waiting';
};
