import { FormikForm } from 'components/formik';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { Col, Row } from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import { toForm } from '../utils';
import { ReportSnapshotEdit } from './ReportSnapshotEdit';
import { ReportSnapshotView } from './ReportSnapshotView';
import * as styled from './styled';

export const ReportSnapshot: React.FC = () => {
  const { id } = useParams();
  const [{ generateReport, updateReport }] = useReports();

  const [report, setReport] = React.useState<IReportForm>(defaultReport);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (!!reportId && reportId !== report.id) {
      generateReport(reportId)
        .then((result) => {
          if (result) setReport(toForm(result, report, true));
        })
        .catch(() => {});
    }
  }, [id, generateReport, report]);

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      try {
        const result = await updateReport(values, true);
        // The update doesn't fetch all the content again.
        // Need to keep the current content information.
        const instances = values.instances.map((i) => {
          const updatedInstance = result.instances.find((ui) => ui.id === i.id);
          return { ...i, version: updatedInstance?.version };
        });
        setReport(toForm({ ...result, instances }, report, true));
      } catch {}
    },
    [report, updateReport],
  );

  return (
    <styled.ReportSnapshot>
      <FormikForm
        initialValues={report}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <Row gap="1rem">
            <Col className="edit">
              <ReportSnapshotEdit />
            </Col>
            <Col className="preview">
              <ReportSnapshotView />
            </Col>
          </Row>
        )}
      </FormikForm>
    </styled.ReportSnapshot>
  );
};
