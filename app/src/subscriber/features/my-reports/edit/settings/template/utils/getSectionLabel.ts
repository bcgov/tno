import { IChartSectionSettingsModel, IReportSectionModel } from '@/shared';

export const getSectionLabel = (
  name: string | undefined | null,
  chartSectionSettings: IChartSectionSettingsModel,
  sections: IReportSectionModel[],
) => {
  if (
    chartSectionSettings.dataset === 'reportSection' ||
    chartSectionSettings.groupBy === 'reportSection'
  ) {
    const section = sections.find((s) => s.name === name);
    return section?.settings.label ?? name;
  }

  return name;
};
