import { Action } from 'components/action';
import { Section } from 'components/section';
import { SectionIcon, SectionLabel } from 'features/my-reports/admin/components';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { moveContent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import { ReportSectionTypeName, Row, Show } from 'tno-core';

import {
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
} from '.';

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
        setFieldValue(`instances.0.content`, newItems);
      }
    },
    [instance, setFieldValue],
  );

  return (
    <div className="sections">
      <Row justifyContent="flex-end">
        {values.sections.some((s) => s.open) ? (
          <Action
            icon={<FaMinus />}
            label="Close all sections"
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
            label="Open all sections"
            onClick={() => {
              setFieldValue(
                'sections',
                values.sections.map((s) => ({
                  ...s,
                  open: true,
                })),
              );
            }}
          />
        )}
      </Row>
      <DragDropContext onDragEnd={handleDrop}>
        {values.sections.map((section, index) => {
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
              key={section.id}
              icon={<SectionIcon type={section.sectionType} />}
              open={section.open}
              label={
                <Row>
                  <SectionLabel section={section} showIcon={false} />
                </Row>
              }
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
                />
              </Show>
              <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                <ReportSectionGallery
                  sectionIndex={index}
                  showForm={form === 'sections'}
                  disabled={disabled}
                />
              </Show>
            </Section>
          );
        })}
      </DragDropContext>
    </div>
  );
};
