import { IIngestModel } from 'tno-core';

import { isRunning } from './isRunning';

export const getStatus = (ingest: IIngestModel, defaultDelayMS: number = 300000) => {
  if (ingest.failedAttempts >= ingest.retryLimit) return 'Failed';
  else if (!ingest.isEnabled) return 'Disabled';
  else if (!ingest.lastRanOn) return 'Never Run';

  return isRunning(ingest, defaultDelayMS) ? 'Running' : 'Not Running';
};
