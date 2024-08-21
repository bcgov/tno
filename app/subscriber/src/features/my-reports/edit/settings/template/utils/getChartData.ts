import {
  getDistinct,
  IReportInstanceContentModel,
  IReportSectionChartTemplateModel,
  IReportSectionModel,
} from 'tno-core';

import { calcDataPoint } from './calcDataPoint';
import { IGenerateChartDataOptions } from './generateChartData';
import { getSectionLabel } from './getSectionLabel';
import { IChartData } from './IChartData';

interface IGetChartDataOptions extends IGenerateChartDataOptions {
  // An array of groups to divide content.
  groups?: (string | number)[];
  // Predicate to identify groups from the content.
  groupOn: (content?: IReportInstanceContentModel) => string | number | undefined | null;
  // Predicate to determine if content is in group.
  isInGroup?: (content?: IReportInstanceContentModel, group?: string | number | null) => boolean;
  // predicate to extract a label for the group.  Defaults to using the 'groupOn' predicate.
  getLabel?: (content?: IReportInstanceContentModel) => string | undefined | null;
}

/**
 * Group the content into specific datasets.
 * @param chart Chart configuration settings.
 * @param datasets Dictionary containing data for each dataset.
 * @param sections Report sections.
 * @param options Configure options.
 * @returns Chart data.
 */
export const getChartData = (
  chart: IReportSectionChartTemplateModel,
  datasets: Record<string, IReportInstanceContentModel[]>,
  sections: IReportSectionModel[],
  options: IGetChartDataOptions,
): IChartData => {
  options.getLabel ??= (c) => {
    const value = options.groupOn(c);
    return value !== undefined ? `${value}` : undefined;
  };
  options.isInGroup ??= (c, g) =>
    (options.groupOn(c) ?? g ?? options?.labelValueWhenEmpty ?? '') === g;

  // Flatten all items into an array.
  const content = Object.entries(datasets)
    .map(([k, v]) => v)
    .flat();

  // Extract unique values to map to the axis labels.
  const groups =
    options.groups ??
    getDistinct(content, (d) => options.groupOn(d)).map((s) => options.groupOn(s));
  const labels = groups.map((group) => {
    // For each group get the label text.
    const label =
      options.getLabel?.(content.find((item) => options.isInGroup?.(item, group))) ??
      (group ? `${group}` : undefined) ??
      options?.labelValueWhenEmpty ??
      '';
    return getSectionLabel(label, chart.sectionSettings, sections);
  });

  // Create an array of datasets grouped by the specified label.
  const results = Object.entries(datasets).map(([k, v]) => {
    let data = groups.map((group) => {
      const items = v
        .filter((item) => options.isInGroup?.(item, group))
        .map((item) => item.content);
      return calcDataPoint(chart, items);
    });

    return {
      label: k,
      data: data,
    };
  });

  return {
    labels,
    datasets: results,
  };
};
