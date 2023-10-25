import moment from 'moment';
import { IIngestModel } from 'tno-core';

export const isRunning = (ingest: IIngestModel, defaultDelayMS: number = 30000) => {
  const now = moment(Date.now());
  const lastRanOn = ingest.lastRanOn ? moment(ingest.lastRanOn) : undefined;
  var delayMS = Math.max(...ingest.schedules.map((s) => s.delayMS));
  if (!delayMS || delayMS === -Infinity) delayMS = defaultDelayMS;
  if (lastRanOn?.add(delayMS, 'millisecond').isSameOrAfter(now)) return true;
  return false;
};
