import { IReportSectionModel, ReportSectionTypeName, Row } from 'tno-core';

import { SectionIcon } from './SectionIcon';

export interface ISectionLabelProps {
  section: IReportSectionModel;
  showIcon?: boolean;
}

export const SectionLabel = ({ section, showIcon = true }: ISectionLabelProps) => {
  if (section.settings.sectionType === ReportSectionTypeName.Content) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.settings.sectionType} />}
        Media Stories: {section.settings.label}
      </Row>
    );
  } else if (section.settings.sectionType === ReportSectionTypeName.TableOfContents) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.settings.sectionType} />}
        Table of Contents
      </Row>
    );
  } else if (section.settings.sectionType === ReportSectionTypeName.Text) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.settings.sectionType} />}
        Text: {section.settings.label}
      </Row>
    );
  } else if (section.settings.sectionType === ReportSectionTypeName.MediaAnalytics) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.settings.sectionType} />}
        Media Analytics: {section.settings.label}
      </Row>
    );
  } else if (section.settings.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center">
        {showIcon && <SectionIcon type={section.settings.sectionType} />}
        Front Page Images
      </Row>
    );
  } else return <>Unknown</>;
};
