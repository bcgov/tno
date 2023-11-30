import { Action } from 'components/action';
import { Section } from 'components/section';
import { IReportForm } from 'features/my-reports/interfaces';
import {
  createReportInstanceContent,
  getBlockName,
  getSectionIcon,
  moveContent,
  sortContent,
} from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { FaCheck, FaPen, FaPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useLookup } from 'store/hooks';
import { ReportSectionTypeName, Row, Settings, Show } from 'tno-core';

import { ReportSectionSummary } from '../old/components/ReportSectionSummary';
import { ReportSectionTableOfContents } from '../old/components/ReportSectionTableOfContents';
import { ReportContentSection } from './ReportContentSection';

export const ReportContentForm: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { values, setFieldValue, isSubmitting } = useFormikContext<IReportForm>();
  const [{ isReady, settings }] = useLookup();

  const [defaultLicenseId, setDefaultLicenseId] = React.useState(0);
  const [defaultMediaTypeId, setDefaultMediaTypeId] = React.useState(0);
  const [showForm, setShowForm] = React.useState(false);

  const instance = values.instances.length ? values.instances[0] : null;
  const userId = userInfo?.id ?? 0;

  React.useEffect(() => {
    if (isReady) {
      const defaultLicenseId = settings.find(
        (s) => s.name === Settings.DefaultSubscriberContentLicense,
      )?.value;
      if (defaultLicenseId) setDefaultLicenseId(+defaultLicenseId);
      else
        toast.error(
          `Configuration settings '${Settings.DefaultSubscriberContentLicense}' is required.`,
        );
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    if (isReady) {
      const defaultMediaTypeId = settings.find(
        (s) => s.name === Settings.DefaultSubscriberContentMediaType,
      )?.value;
      if (defaultMediaTypeId) setDefaultMediaTypeId(+defaultMediaTypeId);
      else
        toast.error(
          `Configuration settings '${Settings.DefaultSubscriberContentMediaType}' is required.`,
        );
    }
  }, [isReady, settings]);

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

  const addStory = React.useCallback(
    (instanceId: number | undefined, sectionName: string) => {
      if (instanceId) {
        const content = sortContent([
          createReportInstanceContent(
            instanceId,
            sectionName,
            userId,
            defaultLicenseId,
            defaultMediaTypeId,
          ),
          ...values.instances[0].content,
        ]);
        setFieldValue(`instances.0.content`, content);
      }
    },
    [defaultLicenseId, defaultMediaTypeId, setFieldValue, userId, values.instances],
  );

  return (
    <div className="sections">
      <DragDropContext onDragEnd={handleDrop}>
        {values.sections.map((section, index) => {
          const label = getBlockName(section);
          return (
            <Section
              key={section.id}
              icon={getSectionIcon(section)}
              open={true}
              label={
                <Row>
                  {label}
                  {section.settings.label !== label ? `: ${section.settings.label}` : ''}
                </Row>
              }
              actions={
                <Row>
                  {section.settings.sectionType === ReportSectionTypeName.Content && (
                    <Row gap="1rem">
                      {/* <Action disabled={isSubmitting} icon={<FaFileExcel />} title="Export to Excel" /> */}
                      <Action
                        icon={!showForm ? <FaPen /> : <FaCheck />}
                        title="edit"
                        onClick={() => setShowForm(!showForm)}
                      />
                      <Action
                        disabled={isSubmitting}
                        icon={<FaPlus />}
                        title="Add custom content"
                        onClick={() => addStory(instance?.id, section.name)}
                      />
                    </Row>
                  )}
                </Row>
              }
            >
              <Show
                visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}
              >
                <ReportSectionTableOfContents index={index} showForm={true} />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
                <ReportContentSection index={index} showForm={showForm} />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Summary}>
                <ReportSectionSummary index={index} showForm={true} />
              </Show>
            </Section>
          );
        })}
      </DragDropContext>
    </div>
  );
};
