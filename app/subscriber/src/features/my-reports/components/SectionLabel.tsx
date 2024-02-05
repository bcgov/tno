import { IReportSectionModel, ReportSectionTypeName, Row } from 'tno-core';

import { SectionIcon } from './SectionIcon';

export interface ISectionLabelProps {
  section: IReportSectionModel;
  showIcon?: boolean;
}

export const SectionLabel = ({ section, showIcon = true }: ISectionLabelProps) => {
  if (section.sectionType === ReportSectionTypeName.Content) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        <span>Stories</span>
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.TableOfContents) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        <span>ToC</span>
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Text) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        <span>Text</span>
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.MediaAnalytics) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        <span>Analysis</span>
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        <span>Gallery</span>
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>{section.settings.label}</span>
      </Row>
    );
  } else return <>Unknown</>;
};
