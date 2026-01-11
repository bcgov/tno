import { IReportSectionModel, ReportSectionTypeName, Row } from 'tno-core';

import { SectionIcon } from './SectionIcon';

export interface ISectionLabelProps {
  section: IReportSectionModel;
}

export const SectionLabel = ({ section }: ISectionLabelProps) => {
  if (section.sectionType === ReportSectionTypeName.Content) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Media Stories: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.TableOfContents) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Table of Contents
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Text) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Text: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.MediaAnalytics) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Media Analytics: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Gallery: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Image) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Image: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Data) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        Data: {section.settings.label}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.AI) {
    return (
      <Row gap="0.25rem" alignItems="center">
        <SectionIcon type={section.sectionType} />
        AI: {section.settings.label}
      </Row>
    );
  } else return <>Unknown</>;
};
