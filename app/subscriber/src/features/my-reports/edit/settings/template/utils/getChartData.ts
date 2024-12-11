import { Moment } from 'moment';
import {
  getDistinct,
  IReportInstanceContentModel,
  IReportSectionChartTemplateModel,
  IReportSectionModel,
} from 'tno-core';

import { calcDataPoint } from './calcDataPoint';
import { IConvertToChartOptions } from './convertToChart';
import { getSectionLabel } from './getSectionLabel';
import { IChartData } from './IChartData';

interface IGetChartDataOptions extends IConvertToChartOptions {
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
  const groupBy = chart.sectionSettings.groupBy;
  const groupByOrder = chart.sectionSettings.groupByOrder;
  const datasetOrder = chart.sectionSettings.datasetOrder;

  options.getLabel ??= (c) => {
    const value = options.groupOn(c);
    return value !== undefined ? `${value}` : undefined;
  };

  // Flatten all report instance content into an array.
  const content = Object.entries(datasets)
    .map(([k, v]) => v)
    .flat();

  // Extract unique values to map to the axis labels.
  let groups =
    options.groups?.map((g) => ({ key: g, label: `${g}` })) ??
    getDistinct(content, (content) => options.groupOn(content))
      .map((content) => {
        const label = options.getLabel?.(content);
        return {
          key: options.groupOn(content),
          label:
            getSectionLabel(label, chart.sectionSettings, sections) ??
            options.labelValueWhenEmpty ??
            'None',
        };
      })
      .filter(
        (g) => !chart.sectionSettings.excludeEmptyValues || (g.key !== undefined && g.key !== null),
      );

  if (!['reportSection', ''].includes(groupBy)) {
    const groupBySort = groupByOrder === 'asc' ? 1 : -1;
    groups = groups.sort((a, b) => {
      if (a.key === undefined || a.key === null) return -1 * groupBySort;
      if (b.key === undefined || b.key === null) return 1 * groupBySort;
      if (a.key < b.key) return -1 * groupBySort;
      if (a.key > b.key) return 1 * groupBySort;
      return 0;
    });
  }

  // Extract the labels and ensure they have a value.
  const labels = groups.map((g) => g.label);
  const datasetSort = datasetOrder === 'asc' ? 1 : -1;

  // Create an array of datasets grouped by the specified label.
  const results = Object.entries(datasets)
    .map(([label, content]) => {
      let values = groups.map((group) => {
        const items = content
          .filter((item) =>
            options?.isInGroup
              ? options.isInGroup(item, group.key)
              : options.groupOn(item) === group.key,
          )
          .map((item) => item.content);
        return calcDataPoint(chart, items);
      });

      // Resort the data values based on the label order.
      const data = labels.map((label) => {
        const oldIndex = groups.findIndex((g) => g.label === label);
        return values[oldIndex];
      });

      return {
        label,
        data,
      };
    })
    .sort((a, b) => {
      if (a.label < b.label) return -1 * datasetSort;
      if (a.label > b.label) return 1 * datasetSort;
      return 0;
    });

  return {
    labels,
    datasets: results,
  };
};
