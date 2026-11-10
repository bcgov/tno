import { IReportSectionModel, ReportSectionTypeName, Row } from 'tno-core';

import { useReportEditContext } from '../edit/ReportEditContext';
import { SectionIcon } from './SectionIcon';

export interface ISectionLabelProps {
  section: IReportSectionModel;
  showIcon?: boolean;
  showTotal?: boolean;
}

export const SectionLabel = ({ section, showIcon = true, showTotal }: ISectionLabelProps) => {
  const { values } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const sectionContent = instance?.content.filter((c) => c.sectionName === section.name) ?? [];
  const contentCount = sectionContent.length;

  if (section.sectionType === ReportSectionTypeName.Content) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Stories:</span>
        <span>{section.settings.label}</span>
        {showTotal && <span className="section-total">stories: {contentCount}</span>}
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
        {showTotal && <span className="section-total">stories: {contentCount}</span>}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Gallery) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Gallery:</span>
        <span>{section.settings.label}</span>
        {showTotal && <span className="section-total">stories: {contentCount}</span>}
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Image) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Image:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.Data) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>Data:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else if (section.sectionType === ReportSectionTypeName.AI) {
    return (
      <Row gap="0.25rem" alignItems="center" className="section-header-label">
        {showIcon && <SectionIcon type={section.sectionType} />}
        <span>AI:</span>
        <span>{section.settings.label}</span>
      </Row>
    );
  } else return <>Unknown</>;
};
