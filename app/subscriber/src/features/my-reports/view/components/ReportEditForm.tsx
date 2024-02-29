import { Action } from 'components/action';
import { Button } from 'components/button';
import { PageSection } from 'components/section';
import { Tabs } from 'components/tabs';
import { ITab } from 'components/tabs/interfaces';
import { toForm } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaRecycle } from 'react-icons/fa';
import { FaArrowLeft, FaCloud, FaFileCirclePlus, FaGear } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Modal, ReportStatusName, Row, useModal } from 'tno-core';

import { IReportForm, IReportInstanceContentForm } from '../../interfaces';
import { ReportPreviewForm, ReportSections, ReportSendForm } from '.';

export interface IReportEditFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Whether to show the add story row */
  showAdd?: boolean;
  /** The active row. */
  activeRow?: IReportInstanceContentForm;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content?: IReportInstanceContentForm, action?: 'previous' | 'next') => void;
}

/**
 * Provides a way to move stories around in a report, add new stories, and remove stories.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditForm: React.FC<IReportEditFormProps> = ({
  disabled,
  showAdd,
  activeRow,
  onContentClick,
}) => {
  const navigate = useNavigate();
  const { values, isSubmitting, submitForm, setValues } = useFormikContext<IReportForm>();
  const { path = 'content' } = useParams();
  const [, { storeReportOutput }] = useProfileStore();
  const { isShowing, toggle } = useModal();
  const [, { generateReport }] = useReports();
  const [{ exportReport }] = useReportInstances();

  const instance = values.instances.length ? values.instances[0] : undefined;

  const tabs: ITab[] = React.useMemo(
    () => [
      {
        key: 'id',
        type: 'other',
        label: values.name ? <label className="h2">{values.name}</label> : '[Report Name]',
        className: 'report-name',
      },
      {
        key: 'content',
        to: `/reports/${values.id}/edit/content`,
        label: <Action label="Stories" />,
      },
      {
        key: 'sections',
        to: `/reports/${values.id}/edit/sections`,
        label: <Action label="Sections" />,
      },
      {
        key: 'preview',
        to: `/reports/${values.id}/edit/preview`,
        label: (
          <Row gap="1rem">
            <Action label="Preview" />
            <Action icon={<FaRecycle />} onClick={() => storeReportOutput(undefined)} />
          </Row>
        ),
      },
      {
        key: 'send',
        to: `/reports/${values.id}/edit/send`,
        label: <Action label="Send" />,
      },
    ],
    [storeReportOutput, values.id, values.name],
  );

  const handleExport = React.useCallback(
    async (report: IReportForm) => {
      try {
        if (report?.id) {
          const instance = report.instances.length ? report.instances[0] : 0;
          if (instance) {
            const filename = report.name.replace(/[^a-zA-Z0-9 ]/g, '');
            await toast.promise(exportReport(instance.id, filename), {
              pending: 'Downloading file',
              success: 'Download complete',
              error: 'Download failed',
            });
          } else {
            toast.error(`The report has not been generated.`);
          }
        }
      } catch {}
    },
    [exportReport],
  );

  const handleRegenerate = React.useCallback(
    async (values: IReportForm, regenerate: boolean) => {
      try {
        const report = await generateReport(values.id, regenerate);
        setValues(toForm(report, true));
        if (regenerate) toast.success('Report has been regenerated');
        else {
          toast.success('Report has been generated');
          navigate(`/reports/${values.id}/edit/content`);
        }
      } catch {}
    },
    [generateReport, navigate, setValues],
  );

  return (
    <PageSection
      tabIndex={0}
      onKeyDown={(e) => {
        console.log(
          '***************************ReportEditForm onKeyDown**********************************',
        );
        if (e.code === 'Escape') onContentClick?.();
        else if (e.ctrlKey || e.metaKey) {
          console.log('metaKey detected');
          if (e.code === 'ArrowUp' || e.code === 'ArrowLeft') {
            onContentClick?.(undefined, 'previous');
            e.stopPropagation();
            e.preventDefault();
          } else if (e.code === 'ArrowDown' || e.code === 'ArrowRight') {
            onContentClick?.(undefined, 'next');
            e.stopPropagation();
            e.preventDefault();
          }
        }
        console.log(
          '***************************ReportEditForm onKeyDown**********************************',
        );
      }}
      header={
        <Row flex="1" alignItems="center" gap="1rem">
          <Col flex="1" gap="0.5rem">
            <Row>
              <Action
                icon={<FaArrowLeft />}
                label="Back to my reports"
                onClick={() => navigate('/reports')}
              />
            </Row>
            <Row alignItems="center">
              <label>Edit Report</label>
            </Row>
          </Col>
          <Col gap="0.5rem">
            <Row gap="1rem" justifyContent="flex-end">
              <Action
                disabled={isSubmitting}
                icon={<FaGear />}
                title="Edit report template"
                onClick={(e) => {
                  if (e.ctrlKey) toggle();
                  else navigate(`/reports/${values.id}`);
                }}
              />
              <Action
                disabled={isSubmitting}
                icon={
                  <img
                    className="excel-icon"
                    src={'/assets/excel-icon.svg'}
                    alt="Export to Excel"
                  />
                }
                title="Export to Excel"
                onClick={() => handleExport(values)}
              />
              {!disabled || instance?.status === ReportStatusName.Submitted ? (
                <Button onClick={() => submitForm()} disabled={isSubmitting || disabled}>
                  Save
                  <FaCloud />
                </Button>
              ) : (
                <Button
                  onClick={() => handleRegenerate(values, true)}
                  disabled={isSubmitting || disabled}
                >
                  Start next report
                  <FaFileCirclePlus />
                </Button>
              )}
            </Row>
          </Col>
        </Row>
      }
    >
      <Tabs
        tabs={tabs}
        activeTab={path}
        onChange={() => {
          // Close story editing window.
          onContentClick?.();
          return Promise.resolve(true);
        }}
      >
        {(tab) => {
          if (tab?.key === 'preview') return <ReportPreviewForm />;
          else if (tab?.key === 'send') return <ReportSendForm />;
          else if (tab?.key === 'sections')
            return (
              <ReportSections
                disabled={disabled}
                form={'sections'}
                activeRow={activeRow}
                onContentClick={onContentClick}
              />
            );
          return (
            <ReportSections
              disabled={disabled}
              showAdd={showAdd}
              form={path === 'content' ? 'stories' : 'sections'}
              activeRow={activeRow}
              onContentClick={onContentClick}
            />
          );
        }}
      </Tabs>
      <span></span>
      <Modal
        headerText="Regenerate Report"
        body="Regenerating a report will rerun all filters and update content in the report.  Do you want to proceed?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            await handleRegenerate(values, true);
          } finally {
            toggle();
          }
        }}
      />
    </PageSection>
  );
};
