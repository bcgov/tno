import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import { Col } from 'tno-core';

import { ReportInstanceView } from './ReportInstanceView';

export const ReportPreviewForm: React.FC = () => {
  const { values } = useFormikContext<IReportForm>();

  const instanceId = values.instances.length ? values.instances[0].id : 0;

  return (
    <Col className="preview-section">
      {instanceId ? (
        <ReportInstanceView instanceId={instanceId} />
      ) : (
        <>The report has not been generated</>
      )}
    </Col>
  );
};
