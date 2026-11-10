import { IReportModel } from 'tno-core';

/**
 * Generates a unique name for the schedule.
 * @param name The schedule name.
 * @param report The report
 * @returns A unique name for the schedule
 */
export const getScheduleName = (name: string, report?: IReportModel) =>
  `${!!name ? `Report-${name}` : !!report?.name ? report.name : 'Report'}`;
