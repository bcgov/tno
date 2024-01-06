import { Action } from 'components/action';
import { Section } from 'components/section';
import { SectionIcon, SectionLabel } from 'features/my-reports/admin/components';
import { IReportForm } from 'features/my-reports/interfaces';
import { createReportInstanceContent, moveContent, sortContent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { FaCheck, FaPen, FaPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useLookup } from 'store/hooks';
import { ReportSectionTypeName, Row, Settings, Show } from 'tno-core';

import {
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
} from '.';

export interface IReportSectionsProps {
  disabled?: boolean;
}

export const ReportSections: React.FC<IReportSectionsProps> = ({ disabled }) => {
  const [{ userInfo }] = useApp();
  const { values, setFieldValue, isSubmitting } = useFormikContext<IReportForm>();
  const [{ isReady, settings }] = useLookup();

  const [defaultLicenseId, setDefaultLicenseId] = React.useState(0);
  const [defaultMediaTypeId, setDefaultMediaTypeId] = React.useState(0);
  const [showForm, setShowForm] = React.useState<number>();

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
          return (
            <Section
              key={section.id}
              icon={<SectionIcon type={section.settings.sectionType} />}
              open={true}
              label={
                <Row>
                  <SectionLabel section={section} showIcon={false} />
                </Row>
              }
              actions={({ open }) => {
                return (
                  <Row gap="1rem">
                    {/* <Action disabled={isSubmitting} icon={<FaFileExcel />} title="Export to Excel" /> */}
                    {[
                      ReportSectionTypeName.Content,
                      ReportSectionTypeName.Gallery,
                      ReportSectionTypeName.MediaAnalytics,
                    ].includes(section.settings.sectionType) &&
                      open &&
                      !disabled && (
                        <Action
                          disabled={isSubmitting || disabled}
                          icon={showForm !== index ? <FaPen /> : <FaCheck />}
                          title="edit"
                          onClick={() => setShowForm(index === showForm ? undefined : index)}
                        />
                      )}
                    {section.settings.sectionType === ReportSectionTypeName.Content &&
                      !disabled &&
                      open && (
                        <Action
                          disabled={isSubmitting || disabled}
                          icon={<FaPlus />}
                          title="Add custom content"
                          onClick={() => addStory(instance?.id, section.name)}
                        />
                      )}
                  </Row>
                );
              }}
            >
              <Show
                visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}
              >
                <ReportSectionTableOfContents index={index} showForm={true} disabled={disabled} />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Text}>
                <ReportSectionText index={index} showForm={true} disabled={disabled} />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
                <ReportSectionContent
                  index={index}
                  showForm={index === showForm}
                  disabled={disabled}
                />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.MediaAnalytics}>
                <ReportSectionMediaAnalytics
                  index={index}
                  showForm={index === showForm}
                  disabled={disabled}
                />
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Gallery}>
                <ReportSectionGallery
                  index={index}
                  showForm={index === showForm}
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
