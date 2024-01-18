import { IReportSectionModel, ReportSectionTypeName, Row } from 'tno-core';

import { SectionIcon } from './SectionIcon';

export interface ISectionLabelProps {
  section: IReportSectionModel;
  showIcon?: boolean;
}

export const SectionLabel = ({ section, showIcon = true }: ISectionLabelProps) => {
  if (section.sectionType === ReportSectionTypeName.Content) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.sectionType} />}
        Media Stories: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.TableOfContents) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.sectionType} />}
        Table of Contents
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Text) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.sectionType} />}
        Text: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.MediaAnalytics) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.sectionType} />}
        Media Analytics: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.sectionType} />}
        Front Page Images
      </Row>
    );
  } else return <>Unknown</>;
};
