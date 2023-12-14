import { Box } from 'components/box';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaGripVertical, FaTrash } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import { IReportForm } from '../../../interfaces';
import { getBlockName } from '../../../utils';
import { ReportSectionContent } from './ReportSectionContent';
import { ReportSectionSummary } from './ReportSectionSummary';
import { ReportSectionTableOfContents } from './ReportSectionTableOfContents';

export interface IReportSectionProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  index: number;
}

/**
 * Component provides a way to configure a report section settings.
 */
export const ReportSection = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();

    const section = values.sections[index];

    const removeSection = React.useCallback(
      (index: number) => {
        setFieldValue(
          'sections',
          values.sections
            .filter((s, i) => i !== index)
            .map((section, index) => ({
              ...section,
              sortOrder: index,
            })),
        );
      },
      [setFieldValue, values.sections],
    );

    return (
      <Box
        icon={<FaGripVertical className="draggable" />}
        title={
          <Row gap="1rem" flex="1" alignItems="center">
            <Col flex="1">
              <h2>{getBlockName(section)}</h2>
            </Col>
            <FormikCheckbox name={`sections.${index}.isEnabled`} label="Is Enabled" />
          </Row>
        }
        actions={
          <Button
            variant={ButtonVariant.link}
            className="danger"
            onClick={() => removeSection(index)}
          >
            <FaTrash />
          </Button>
        }
        canShrink={true}
        expand={section.open}
        onExpand={(expand) => {
          setFieldValue(`sections.${index}.expand`, expand);
          return expand;
        }}
        {...rest}
      >
        <Show visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}>
          <ReportSectionTableOfContents index={index} />
        </Show>
        <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
          <ReportSectionContent index={index} />
        </Show>
        <Show visible={section.settings.sectionType === ReportSectionTypeName.Summary}>
          <ReportSectionSummary index={index} />
        </Show>
      </Box>
    );
  },
);
