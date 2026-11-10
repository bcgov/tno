import { IReportModel } from '@/shared';

/** Shown when a collection-only report is asked to be sent; the API refuses with the same text. */
export const collectionOnlySendMessage =
  "This report is for content collection only and cannot be sent. Turn off 'Collection only' in the report settings to send it.";

/**
 * Whether the report only collects content for other reports and is never sent.
 * tno-core does not publish this setting yet.
 * @param report The report.
 */
export const isCollectionOnly = (report: IReportModel) =>
  !!(report.settings as { collectionOnly?: boolean } | undefined)?.collectionOnly;
