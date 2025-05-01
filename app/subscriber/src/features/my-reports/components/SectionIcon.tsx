import { FaAlignJustify, FaChartPie, FaImage, FaList, FaNewspaper } from 'react-icons/fa6';
import { ReportSectionTypeName } from 'tno-core';

export interface ISectionIconProps {
  type: ReportSectionTypeName;
}

export const SectionIcon = ({ type }: ISectionIconProps) => {
  if (type === ReportSectionTypeName.Content) {
    return <FaNewspaper />;
  } else if (type === ReportSectionTypeName.TableOfContents) {
    return <FaList />;
  } else if (type === ReportSectionTypeName.Text) {
    return <FaAlignJustify />;
  } else if (type === ReportSectionTypeName.MediaAnalytics) {
    return <FaChartPie />;
  } else if (type === ReportSectionTypeName.Gallery) {
    return <FaImage />;
  } else if (type === ReportSectionTypeName.Image) {
    return <FaImage />;
  } else return null;
};
