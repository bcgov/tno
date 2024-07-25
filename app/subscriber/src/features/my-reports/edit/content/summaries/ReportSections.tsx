import { Section } from 'components/section';
import { SectionLabel } from 'features/my-reports/components';
import { IReportForm } from 'features/my-reports/interfaces';
import { moveContent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { ReportSectionTypeName, Show } from 'tno-core';

import {
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
} from '.';
import * as styled from './styled';

export interface IReportSectionsProps {
  disabled?: boolean;
}

/**
 * Provides component that displays the report sections.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportSections: React.FC<IReportSectionsProps> = ({ disabled }) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();

  const instance = values.instances.length ? values.instances[0] : null;

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    (result: DropResult, provided: ResponderProvided) => {
      if (instance) {
        const newItems = moveContent(result, instance.content);
        if (newItems) setFieldValue(`instances.0.content`, newItems);
      }
    },
    [instance, setFieldValue],
  );
  const reportContent: { label: string; url: string; section: string }[] = values.instances.length
    ? values.instances[0].content.map((c) => {
        return {
          label: c.content?.headline || '', // Provide a default value in case headline is undefined
          url: `/view/${c.content?.id}` || '', // Provide a default value in case id is undefined
          section: values.sections.find((s) => s.name === c.sectionName)?.settings.label || '',
        };
      })
    : [];

  return (
    <styled.ReportSections className="sections">
      <DragDropContext onDragEnd={handleDrop}>
        {values.sections.map((section, index) => {
          return (
            <Section
              key={`${section.id}-${index}`}
              open={section.open}
              label={<SectionLabel section={section} />}
            >
              <Show visible={section.sectionType === ReportSectionTypeName.TableOfContents}>
                <ReportSectionTableOfContents sectionIndex={index} disabled={disabled} />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Text}>
                <ReportSectionText sectionIndex={index} disabled={disabled} />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Content}>
                <ReportSectionContent
                  reportContent={reportContent}
                  sectionIndex={index}
                  disabled={disabled}
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.MediaAnalytics}>
                <ReportSectionMediaAnalytics sectionIndex={index} disabled={disabled} />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                <ReportSectionGallery sectionIndex={index} disabled={disabled} />
              </Show>
            </Section>
          );
        })}
      </DragDropContext>
    </styled.ReportSections>
  );
};
