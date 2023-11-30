import { Box } from 'components/box';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaListOl, FaNewspaper, FaPlus, FaRegFolder, FaTasks } from 'react-icons/fa';
import { FaA, FaChartSimple, FaFilter } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useLookup } from 'store/hooks';
import { ReportSectionTypeName, Row, Settings, Show } from 'tno-core';

import { IReportForm } from '../../../interfaces';
import { createReportInstanceContent, sortContent } from '../../../utils';
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
    const [{ userInfo }] = useApp();
    const [{ isReady, settings }] = useLookup();

    const [show, setShow] = React.useState(showForm);
    const [defaultLicenseId, setDefaultLicenseId] = React.useState(0);
    const [defaultMediaTypeId, setDefaultMediaTypeId] = React.useState(0);

    const section = values.sections[index];
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

    const addStory = React.useCallback(
      (instanceId: number, sectionName: string) => {
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
      },
      [defaultLicenseId, defaultMediaTypeId, setFieldValue, userId, values.instances],
    );

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
          <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
            <Row gap="1rem">
              <FaPlus
                className="btn btn-link"
                title="Add story"
                onClick={() => addStory(values.instances[0].id, section.name)}
              />
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
          <ReportSectionSummary index={index} showForm={true} />
        </Show>
      </Box>
    );
  },
);
