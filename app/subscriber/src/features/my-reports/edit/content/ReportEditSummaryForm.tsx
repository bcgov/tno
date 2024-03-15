import { Action } from 'components/action';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';

import { useReportEditContext } from '../ReportEditContext';
import { ReportSections } from './components';
import * as styled from './styled';

export interface IReportEditSummaryFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
}

export const ReportEditSummaryForm = ({ disabled }: IReportEditSummaryFormProps) => {
  const { values, setFieldValue } = useReportEditContext();

  return (
    <styled.ReportEditSummaryForm className="report-edit-section">
      <div className="section-actions">
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
      <ReportSections disabled={disabled} form={'sections'} />
    </styled.ReportEditSummaryForm>
  );
};
