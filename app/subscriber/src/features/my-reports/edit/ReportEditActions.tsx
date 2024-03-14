import { Button } from 'components/button';
import { FaSave } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';

import { useReportEditContext } from './ReportEditContext';
import * as styled from './styled';

export const ReportEditActions = () => {
  const { isSubmitting, submitForm } = useReportEditContext();
  const navigate = useNavigate();

  return (
    <styled.ReportEditActions className="report-edit-actions">
      <Button variant="secondary" onClick={() => navigate('/reports')} disabled={isSubmitting}>
        Cancel
      </Button>
      <Button onClick={() => submitForm()} disabled={isSubmitting}>
        Save
        <FaSave />
      </Button>
    </styled.ReportEditActions>
  );
};
