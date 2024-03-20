import { Button } from 'components/button';
import { FaSave } from 'react-icons/fa';
import { FaFileCirclePlus } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { ReportStatusName } from 'tno-core';

import { IReportForm } from '../interfaces';
import { useReportEditContext } from './ReportEditContext';
import * as styled from './styled';

export interface IReportEditActionsProps {
  /** Control which buttons are enabled. */
  disabled?: boolean;
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

/**
 * Provides component that provides action buttons for the report administration (i.e. Cancel, Save, Start next report).
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditActions = ({ disabled, updateForm }: IReportEditActionsProps) => {
  const { isSubmitting, submitForm, onRegenerate } = useReportEditContext();
  const navigate = useNavigate();
  const { values } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;

  return (
    <styled.ReportEditActions className="report-edit-actions">
      <Button variant="secondary" onClick={() => navigate('/reports')}>
        Cancel
      </Button>

      {/* Show save during submitted to handle scenario when email fails */}
      {!disabled || instance?.status === ReportStatusName.Submitted ? (
        <Button onClick={() => submitForm()} disabled={isSubmitting}>
          Save
          <FaSave />
        </Button>
      ) : (
        <Button
          disabled={isSubmitting}
          onClick={async () => {
            const form = await onRegenerate(values, true);
            if (form) updateForm(form);
          }}
        >
          Start next report
          <FaFileCirclePlus />
        </Button>
      )}
    </styled.ReportEditActions>
  );
};
