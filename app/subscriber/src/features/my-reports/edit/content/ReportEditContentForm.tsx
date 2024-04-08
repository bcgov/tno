import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { toForm } from 'features/my-reports/utils';
import React from 'react';
import { FaRecycle } from 'react-icons/fa';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';
import { useModal } from 'tno-core';

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
export const ReportEditContentForm = ({
  disabled,
  showAdd,
  activeRow,
  onContentClick,
}: IReportEditContentFormProps) => {
  const navigate = useNavigate();
  const { values, isSubmitting, setValues, setFieldValue, onExport } = useReportEditContext();
  const { path1 } = useParams();
  const { isShowing, toggle } = useModal();
  const [, { generateReport }] = useReports();

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
    <styled.ReportEditContentForm className="report-edit-section">
      <div className="section-actions">
        <div></div>
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
          <Action icon={<FaRecycle />} disabled={isSubmitting} onClick={(e) => toggle()} />
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
        activeRow={activeRow}
        onContentClick={onContentClick}
      />
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
    </styled.ReportEditContentForm>
  );
};
