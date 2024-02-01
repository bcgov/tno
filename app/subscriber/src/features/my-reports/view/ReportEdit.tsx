import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IReportMessageModel, MessageTargetName, ReportStatusName, Row } from 'tno-core';

import { ReportFormSchema } from '../admin/validation/ReportFormSchema';
import { defaultReport } from '../constants';
import { IReportForm, IReportInstanceContentForm } from '../interfaces';
import { sortContent, toForm } from '../utils';
import { ContentEditForm, ReportEditForm } from './components';
import * as styled from './styled';

/**
 * Provides a way to edit a report instance.  Update content, preview the report, and send it.
 * Provides two column layout for editing content.
 * @returns Component
 */
export const ReportEdit: React.FC = () => {
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [, { generateReport, getReport, updateReport, findMyReports }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportForm>(defaultReport(userInfo?.id ?? 0, 0));
  const [activeRow, setActiveRow] = React.useState<IReportInstanceContentForm>();

  // Helper func to generate report data if current report is missing it
  const callGenerateReport = React.useCallback(async () => {
    const reportId = parseInt(id ?? '0');
    const result = await generateReport(reportId);
    setReport(toForm(result, true));
  }, [id, generateReport]);

  // Each time report data is refreshed, check if instances for report are loaded,
  // if not, call generateReport to populate instances array
  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    // The report has either never generated an instance, or the last instance was already sent.
    if (report && !!reportId && report.id === reportId && !report?.instances?.length) {
      callGenerateReport();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [report]);

  React.useEffect(() => {
    if (!myReports?.length) {
      findMyReports()
        .then(async (reports) => {
          const reportId = parseInt(id ?? '0');
          if (reportId) {
            const report = reports.find((r) => r.id === reportId);
            if (report) {
              setReport(toForm(report));
            }
          }
        })
        .catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (!!reportId && myReports?.length) {
      const existingReport = myReports.find((r) => r.id === reportId);
      const hasFetchedContent = existingReport?.instances.some((r) => r.content?.length);
      if (existingReport && hasFetchedContent) {
        // Case 1: Complete data: do nothing
        // We have existing report & it has instance content loaded already
        setReport(toForm(existingReport));
      } else if (existingReport && !hasFetchedContent) {
        // Case 2: Partial Data: call generateReport to get missing data
        // We have existing report loaded, but no content present in instance data
        setReport(toForm({ ...existingReport, instances: [] }));
      } else {
        // Case 3: No report data present, fetch entire report + instance + content data
        getReport(reportId, true)
          .then(async (report) => {
            if (report) {
              setReport(toForm(report));
            }
          })
          .catch(() => {});
      }
    }
    // Only make a request when 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  hub.useHubEffect(MessageTargetName.ReportStatus, async (message: IReportMessageModel) => {
    // Report has been updated, go fetch latest.
    try {
      if (message.status === ReportStatusName.Accepted) {
        const instance = await getReportInstance(message.id, true);
        if (instance) {
          setReport({
            ...report,
            instances: report.instances.map((i) => (i.id === message.id ? instance : i)),
          });
          navigate(`/reports/${report.id}/edit/content`);
        }
      }
    } catch {}
  });

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      try {
        const sameNameReport = myReports.some(
          (r) =>
            r.name.trim().toLocaleLowerCase() === values.name.trim().toLocaleLowerCase() &&
            r.id !== values.id,
        );
        if (sameNameReport) {
          toast.error(`A report with the name '${values.name}' already exists.`);
        } else {
          if (values.instances.length) {
            // Apply new sort order values for content to stop content from moving around when it has the same sort order value.
            values.instances[0].content = sortContent(values.instances[0].content, true);
          }
          const report = await updateReport(values, true);
          storeReportOutput(undefined); // Clear the preview
          setReport(toForm(report));
          toast.success(`Successfully updated '${report.name}'.`);
        }
      } catch {}
    },
    [myReports, storeReportOutput, updateReport],
  );

  return (
    <styled.ReportEdit>
      <FormikForm
        initialValues={report}
        validationSchema={ReportFormSchema}
        validateOnChange={false}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values }) => {
          const instance = values.instances.length ? values.instances[0] : undefined;
          const canEdit = instance ? instance.status === ReportStatusName.Pending : true;
          return (
            <Row>
              <Col flex="1">
                <ReportEditForm
                  disabled={!canEdit}
                  showAdd={!activeRow}
                  activeRow={activeRow}
                  onContentClick={(content) => {
                    setActiveRow(content);
                  }}
                />
              </Col>
              {activeRow && (
                <Col flex="1">
                  <ContentEditForm
                    disabled={!canEdit}
                    row={activeRow}
                    onUpdate={(row) => setActiveRow(row)}
                  />
                </Col>
              )}
            </Row>
          );
        }}
      </FormikForm>
    </styled.ReportEdit>
  );
};
