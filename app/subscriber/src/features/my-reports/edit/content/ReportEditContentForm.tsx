import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { StartNextReportInfo } from 'features/my-reports/components';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { toForm } from 'features/my-reports/utils';
import React from 'react';
import { FaArrowsRotate, FaToggleOff, FaToggleOn } from 'react-icons/fa6';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useReports, useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Checkbox, Show, ToggleButton, useModal } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import { ReportSections } from './stories';
import * as styled from './styled';

export interface IReportEditContentFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Whether to show the add story row */
  showAdd?: boolean;
  /** The active row. */
  activeRow?: IReportInstanceContentForm;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content?: IReportInstanceContentForm, action?: 'previous' | 'next') => void;
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

/**
 * Provides component to manage the content in a report.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditContentForm = React.forwardRef<
  HTMLDivElement | null,
  IReportEditContentFormProps
>(({ disabled, showAdd, activeRow, onContentClick, updateForm }, ref) => {
  const { values, isSubmitting, setFieldValue, onExport, onGenerate, setSubmitting } =
    useReportEditContext();
  const { path1 } = useParams();
  const { isShowing, toggle } = useModal();
  const [, { updateReport }] = useReports();
  const [{ profile }] = useProfileStore();
  const { updateUser } = useUsers();

  const [excludeContentInUnsentReport, setExcludeContentInUnsentReport] = React.useState(
    values.settings.content.excludeContentInUnsentReport,
  );

  const showFolders = !!profile?.preferences?.showReportFolderSections;

  return (
    <styled.ReportEditContentForm className="report-edit-section" ref={ref}>
      <StartNextReportInfo />
      <div className="section-actions">
        <div>
          <ToggleButton
            on={<FaToggleOn />}
            off={<FaToggleOff />}
            onClick={async (e) => {
              try {
                if (profile)
                  await updateUser({
                    ...profile,
                    preferences: {
                      ...profile?.preferences,
                      showReportFolderSections: !showFolders,
                    },
                  });
              } catch {}
            }}
            value={showFolders}
            width="25px"
            height="25px"
            label="Show folder sections"
          />
        </div>
        <div>
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
        </div>
        <div>
          <Show visible={!disabled}>
            <Action
              className="icon-refresh"
              icon={<FaArrowsRotate />}
              label="Regenerate report"
              disabled={isSubmitting}
              onClick={(e) => toggle()}
              direction="row-reverse"
            />
          </Show>
          <Action
            icon={
              <img className="excel-icon" src={'/assets/excel-icon.svg'} alt="Export to Excel" />
            }
            disabled={isSubmitting}
            onClick={() => onExport(values)}
          />
        </div>
      </div>
      <ReportSections
        disabled={disabled}
        showAdd={showAdd}
        form={path1 === 'content' ? 'stories' : 'sections'}
        showFolderSections={showFolders}
        activeRow={activeRow}
        onContentClick={onContentClick}
        updateForm={updateForm}
      />
      <Modal
        headerText="Regenerate Report"
        body={
          <>
            <p>
              Regenerating a report will remove all content, then apply each folder, and rerun each
              filter to populate the report.
            </p>
            <Show visible={values.settings.content.excludeContentInUnsentReport}>
              <p>
                Your report generation options currently will not include any content that currently
                exists in your report.
              </p>
              <Checkbox
                name="excludeContentInUnsentReport"
                label="Exclude content found in this current report"
                checked={excludeContentInUnsentReport}
                onChange={(e) => {
                  setExcludeContentInUnsentReport(!excludeContentInUnsentReport);
                }}
              />
            </Show>
            <p>Do you want to proceed?</p>
          </>
        }
        isShowing={isShowing}
        isSubmitting={isSubmitting}
        hide={toggle}
        type="default"
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            setSubmitting(true);
            // If they have temporarily turned off 'excludeContentInUnsentReport' then we need to turn it back on after regenerating.
            let form: IReportForm | undefined = values;
            if (
              excludeContentInUnsentReport !== values.settings.content.excludeContentInUnsentReport
            ) {
              const result = await updateReport({
                ...values,
                settings: {
                  ...values.settings,
                  content: { ...values.settings.content, excludeContentInUnsentReport },
                },
              });
              form = toForm(result);
            }
            form = await onGenerate(form, true);
            if (
              form &&
              excludeContentInUnsentReport !== values.settings.content.excludeContentInUnsentReport
            ) {
              setExcludeContentInUnsentReport(values.settings.content.excludeContentInUnsentReport);
              const result = await updateReport({
                ...form,
                settings: {
                  ...values.settings,
                  content: {
                    ...values.settings.content,
                    excludeContentInUnsentReport:
                      values.settings.content.excludeContentInUnsentReport,
                  },
                },
              });
              form = toForm(result);
            }
            if (form) updateForm(form);
          } finally {
            setSubmitting(false);
            toggle();
          }
        }}
      />
    </styled.ReportEditContentForm>
  );
});
