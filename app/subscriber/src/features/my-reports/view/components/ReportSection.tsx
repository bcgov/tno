import { Box } from 'components/box';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaListOl, FaNewspaper, FaRegFolder, FaTasks } from 'react-icons/fa';
import { FaA, FaChartSimple, FaFilter } from 'react-icons/fa6';
import { ReportSectionTypeName, Row, Show } from 'tno-core';

import { IReportForm } from '../../interfaces';
import { ReportSectionContent } from './ReportSectionContent';
import { ReportSectionSummary } from './ReportSectionSummary';
import { ReportSectionTableOfContents } from './ReportSectionTableOfContents';

export interface IReportSectionProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  index: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Enable toggling the form values */
  showForm?: boolean;
}

/**
 * Component provides a way to configure a report section settings.
 */
export const ReportSection = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, showForm, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();

    const section = values.sections[index];

    const [show, setShow] = React.useState(showForm ?? (!section.filterId && !section.folderId));

    return (
      <Box
        icon={
          <>
            <Show visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}>
              <FaListOl />
            </Show>
            <Show
              visible={
                section.settings.sectionType === ReportSectionTypeName.Content &&
                !section.filterId &&
                !section.folderId
              }
            >
              <FaNewspaper />
            </Show>
            <Show
              visible={
                section.settings.sectionType === ReportSectionTypeName.Content && !!section.filterId
              }
            >
              <FaFilter />
            </Show>
            <Show
              visible={
                section.settings.sectionType === ReportSectionTypeName.Content && !!section.folderId
              }
            >
              <FaRegFolder />
            </Show>
            <Show
              visible={
                section.settings.sectionType === ReportSectionTypeName.Summary &&
                !section.chartTemplates.length
              }
            >
              <FaA />
            </Show>
            <Show
              visible={
                section.settings.sectionType === ReportSectionTypeName.Summary &&
                !!section.chartTemplates.length
              }
            >
              <FaChartSimple />
            </Show>
          </>
        }
        title={
          <Row className="header">
            <h2 className="ellipsis">{section.settings.label}</h2>
          </Row>
        }
        canShrink={true}
        expand={section.expand}
        onExpand={(expand) => {
          setFieldValue(`sections.${index}.expand`, expand);
          return expand;
        }}
        actions={
          <Show visible={!!section.filterId || !!section.folderId}>
            <Row gap="0.25rem">
              <FaTasks className="btn btn-link" title="Edit" onClick={() => setShow(!show)} />
            </Row>
          </Show>
        }
        {...rest}
      >
        <Show visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}>
          <ReportSectionTableOfContents index={index} showForm={true} />
        </Show>
        <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
          <ReportSectionContent index={index} showForm={show} />
        </Show>
        <Show visible={section.settings.sectionType === ReportSectionTypeName.Summary}>
          <ReportSectionSummary index={index} showForm={show} />
        </Show>
      </Box>
    );
  },
);
