import { FaAlignJustify, FaChartPie, FaList, FaNewspaper } from 'react-icons/fa6';
import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

export const getSectionIcon = (section: IReportSectionModel) => {
  if (section.settings.sectionType === ReportSectionTypeName.Content) {
    if (section.settings.showCharts) return <FaChartPie />;
    return <FaNewspaper />;
  } else if (section.settings.sectionType === ReportSectionTypeName.TableOfContents) {
    return <FaList />;
  } else if (section.settings.sectionType === ReportSectionTypeName.Summary) {
    if (section.settings.showCharts) return <FaChartPie />;
    return <FaAlignJustify />;
  } else return 'Unknown';
};
