import { Moment } from 'moment';
import { getDistinct, IReportInstanceContentModel } from 'tno-core';

import { IConvertToChartOptions } from './convertToChart';

interface ISeparateDatasetsOptions extends IConvertToChartOptions {
  // An array of groups to divide content.
  groups?: (string | number)[];
  // Predicate to identify groups from the content.
  groupOn: (
    content?: IReportInstanceContentModel,
  ) => string | number | Moment | Date | undefined | null;
  // Predicate to determine if content is in group.
  isInGroup?: (
    content?: IReportInstanceContentModel,
    group?: string | number | Moment | Date | null,
  ) => boolean;
  // predicate to extract a label for the group.  Defaults to using the 'groupOn' predicate.
  getLabel?: (content?: IReportInstanceContentModel) => string | undefined | null;
}

/**
 * Separates the data into datasets based on the provided predicate.
 * @param data An array of content in the report.
 * @param options Options to control grouping.
 * @returns Dictionary containing content for each dataset.
 */
export const separateDatasets = (
  data: IReportInstanceContentModel[],
  options: ISeparateDatasetsOptions,
): Record<string, IReportInstanceContentModel[]> => {
  options.getLabel ??= (c) => {
    const value = options.groupOn(c);
    return value !== undefined ? `${value}` : undefined;
  };
  options.isInGroup ??= (c, g) =>
    (options.groupOn(c) ?? g ?? options?.labelValueWhenEmpty ?? '') === g;

  const results: Record<string, IReportInstanceContentModel[]> = {};

  const groups =
    options.groups ??
    getDistinct(data, (item) => options.groupOn(item)).map((s) => options.groupOn(s));

  groups.forEach((group) => {
    const items = data.filter((item) => options.isInGroup?.(item, group));
    const key =
      (items.length ? options.getLabel!(items[0]) : undefined) ??
      `${group}` ??
      options?.labelValueWhenEmpty ??
      '';
    results[key] = items;
  });

  return results;
};
