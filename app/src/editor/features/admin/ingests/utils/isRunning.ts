import { type IIngestModel } from '@/shared';
import { dayjs } from '@/shared/utils/date';

export const isRunning = (ingest: IIngestModel, defaultDelayMS: number = 300000) => {
  const now = dayjs(Date.now());
  const lastRanOn = ingest.lastRanOn ? dayjs(ingest.lastRanOn) : undefined;
  let delayMS = Math.max(...ingest.schedules.map((s) => s.delayMS));
  if (!delayMS || delayMS === -Infinity) delayMS = defaultDelayMS;
  if (lastRanOn?.add(delayMS, 'millisecond').isSameOrAfter(now)) return true;
  return false;
};
