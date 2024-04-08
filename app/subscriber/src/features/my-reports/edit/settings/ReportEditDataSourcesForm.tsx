import { Action } from 'components/action';
import { Section } from 'components/section';
import { SectionLabel } from 'features/my-reports/components';
import React from 'react';
import { FaAngleDown, FaGripVertical, FaMinus } from 'react-icons/fa6';
import { useFilters, useFolders, useReports } from 'store/hooks';
import { Col, ReportSectionTypeName, Row, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import {
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
} from './data-sources';
import * as styled from './styled';

export const ReportEditDataSourcesForm = () => {
  const { values, setFieldValue } = useReportEditContext();
  const [{ myFolders }, { findMyFolders }] = useFolders();
  const [{ myFilters }, { findMyFilters }] = useFilters();
  const [{ myReports }, { findMyReports }] = useReports();

  React.useEffect(() => {
    if (!myFolders.length) {
      findMyFolders().catch(() => {});
    }
    if (!myFilters.length) {
      findMyFilters().catch(() => {});
    }
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.ReportEditDataSourcesForm>
      <Row className="template-action-bar">
        {!!values.sections.length &&
          (values.sections.some((s) => s.open) ? (
            <Action
              icon={<FaMinus />}
              label="Close All"
              onClick={() => {
                setFieldValue(
                  `sections`,
                  values.sections.map((s) => ({ ...s, open: false })),
                );
              }}
            />
          ) : (
            <Action
              icon={<FaAngleDown />}
              label="Open All"
              onClick={() => {
                setFieldValue(
                  'sections',
                  values.sections.map((s) => ({ ...s, open: true })),
                );
              }}
            />
          ))}
      </Row>
      <Col className="report-sections">
        {values.sections.map((section, index) => {
          if (
            ![
              ReportSectionTypeName.Content,
              ReportSectionTypeName.Gallery,
              ReportSectionTypeName.MediaAnalytics,
            ].includes(section.sectionType)
          )
            return null;
          return (
            <Section
              key={`${section.id}-${index}`}
              icon={<FaGripVertical className="grip-bar" />}
              label={<SectionLabel section={section} />}
              open={section.open}
              className="template-section"
              onChange={(open) => setFieldValue(`sections.${index}.open`, open)}
            >
              {/* CONTENT */}
              <Show visible={section.sectionType === ReportSectionTypeName.Content}>
                <ReportSectionContent index={index} />
              </Show>
              {/* MEDIA ANALYTICS */}
              <Show visible={section.sectionType === ReportSectionTypeName.MediaAnalytics}>
                <ReportSectionMediaAnalytics index={index} />
              </Show>
              {/* FRONT PAGE IMAGES */}
              <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                <ReportSectionGallery index={index} />
              </Show>
            </Section>
          );
        })}
      </Col>
    </styled.ReportEditDataSourcesForm>
  );
};
