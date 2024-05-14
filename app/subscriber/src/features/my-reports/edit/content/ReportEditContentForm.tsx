import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { StartNextReportInfo } from 'features/my-reports/components';
import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import React from 'react';
import { FaArrowsSpin, FaToggleOff, FaToggleOn } from 'react-icons/fa6';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Show, ToggleButton, useModal } from 'tno-core';

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
}

/**
 * Provides component to manage the content in a report.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditContentForm = React.forwardRef<
  HTMLDivElement | null,
  IReportEditContentFormProps
>(({ disabled, showAdd, activeRow, onContentClick }, ref) => {
  const { values, isSubmitting, setFieldValue, onExport, onGenerate } = useReportEditContext();
  const { path1 } = useParams();
  const { isShowing, toggle } = useModal();
  const [{ profile }] = useProfileStore();
  const { updateUser } = useUsers();

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
              icon={<FaArrowsSpin />}
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
      />
      <Modal
        headerText="Regenerate Report"
        body={
          <>
            <p>
              Regenerating a report will remove all content, then apply each folder, and rerun each
              filter to populate the report.
            </p>
            <p>Do you want to proceed?</p>
          </>
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            await onGenerate(values, true);
          } finally {
            toggle();
          }
        }}
      />
    </styled.ReportEditContentForm>
  );
});
