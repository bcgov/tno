import { FormikForm } from 'components/formik';
import { SearchWithLogout } from 'components/search-with-logout';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useApiHub, useReportInstances, useReports } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Col, Container, IReportMessageModel, MessageTargetName, Row } from 'tno-core';

import { defaultReport } from '../../constants';
import { IReportForm } from '../../interfaces';
import { toForm } from '../../utils';
import { ReportSnapshotEdit } from './ReportSnapshotEdit';
import { ReportSnapshotView } from './ReportSnapshotView';
import * as styled from './styled';

const loading = ['generate-report', 'update-report', 'delete-report', 'export-report'];

export const ReportSnapshot: React.FC = () => {
  const { id } = useParams();
  const [{ updateReport, generateReport }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const [{ requests }] = useAppStore();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportForm>(defaultReport);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (!!reportId) {
      generateReport(reportId)
        .then((result) => {
          if (result) setReport(toForm(result));
        })
        .catch(() => {});
    }
    // Only make a request if the 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  hub.useHubEffect(MessageTargetName.ReportStatus, async (message: IReportMessageModel) => {
    // Report has been updated, go fetch latest.
    try {
      const result = await getReportInstance(message.id);
      if (result)
        setReport(
          toForm({
            ...report,
            instances: report.instances.map((i) => (i.id === result.id ? result : i)),
          }),
        );
    } catch {}
  });

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      try {
        const result = await updateReport(values, true);
        setReport(toForm(result));
      } catch {}
    },
    [updateReport],
  );

  return (
    <styled.ReportSnapshot>
      <SearchWithLogout />
      <FormikForm
        loading={false}
        initialValues={report}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <Row gap="1rem">
            <Col className="edit">
              <Container isLoading={requests.some((r) => loading.includes(r.url))}>
                <ReportSnapshotEdit />
              </Container>
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
