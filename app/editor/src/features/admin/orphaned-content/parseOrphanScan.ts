import { IOrphanScan } from 'store/hooks/admin/useOrphanedContent';

export class OrphanScanResponseError extends Error {
  constructor() {
    super(
      'Scan status is incompatible with this page. Update the API to the background-scan version.',
    );
  }
}

/** Reject old scan-page responses before they become job state. */
export const parseOrphanScan = (value: unknown): IOrphanScan | null => {
  if (value === null || value === undefined || value === '') return null;
  if (typeof value !== 'object') throw new OrphanScanResponseError();
  const job = value as Partial<IOrphanScan>;
  const counts = [job.scanned, job.pending, job.found];
  if (
    typeof job.id !== 'string' ||
    !job.id ||
    !['postgres', 'elasticsearch'].includes(job.side ?? '') ||
    !['content', 'published'].includes(job.index ?? '') ||
    !['Scanning', 'Confirming', 'Completed', 'Cancelled', 'Failed'].includes(job.status ?? '') ||
    !counts.every(
      (count) => typeof count === 'number' && Number.isSafeInteger(count) && count >= 0,
    ) ||
    typeof job.createdOn !== 'string' ||
    !Number.isFinite(Date.parse(job.createdOn)) ||
    typeof job.cancelRequested !== 'boolean'
  ) {
    throw new OrphanScanResponseError();
  }
  return job as IOrphanScan;
};
