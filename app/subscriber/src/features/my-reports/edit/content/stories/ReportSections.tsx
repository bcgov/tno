import { Section } from 'components/section';
import { SectionLabel } from 'features/my-reports/components';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
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
  /** Whether to show the add story row */
  showAdd?: boolean;
  /** Which type of form to display */
  form?: 'stories' | 'sections';
  /** The active row. */
  activeRow?: IReportInstanceContentForm;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content: IReportInstanceContentForm) => void;
}

/**
 * Provides component that displays the report sections.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportSections: React.FC<IReportSectionsProps> = ({
  disabled,
  showAdd,
  form = 'stories',
  activeRow,
  onContentClick,
}) => {
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

  return (
    <styled.ReportSections className="sections">
      <DragDropContext onDragEnd={handleDrop}>
        {values.sections.map((section, index) => {
          // Only display content and gallery sections if it's the stories tab.
          if (
            form === 'stories' &&
            ![ReportSectionTypeName.Content, ReportSectionTypeName.Gallery].includes(
              section.sectionType,
            )
          ) {
            return <React.Fragment key={section.id}></React.Fragment>;
          }

          return (
            <Section
              key={`${section.id}-${index}`}
              open={section.open}
              label={<SectionLabel section={section} />}
            >
              <Show visible={section.sectionType === ReportSectionTypeName.TableOfContents}>
                <ReportSectionTableOfContents
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  disabled={disabled}
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Text}>
                <ReportSectionText
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  disabled={disabled}
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Content}>
                <ReportSectionContent
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  showContent={form === 'stories'}
                  showAdd={showAdd}
                  disabled={disabled}
                  activeRow={activeRow}
                  onContentClick={onContentClick}
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.MediaAnalytics}>
                <ReportSectionMediaAnalytics
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  disabled={disabled}
                  onContentClick={onContentClick}
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                <ReportSectionGallery
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  disabled={disabled}
                  onContentClick={onContentClick}
                />
              </Show>
            </Section>
          );
        })}
      </DragDropContext>
    </styled.ReportSections>
  );
};
