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
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Stories:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.TableOfContents) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>ToC:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Text) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Text:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.MediaAnalytics) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Analysis:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Gallery:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else return <>Unknown</>;
};
