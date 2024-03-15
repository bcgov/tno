import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import {
  useApiHub,
  useApp,
  useLookup,
  useReportInstances,
  useReports,
  useReportTemplates,
} from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IReportMessageModel, MessageTargetName, ReportStatusName, Settings } from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import { sortContent, toForm } from '../utils';
import { ContentEditForm } from './content/components';
import { ReportEditContextProvider } from './ReportEditContext';
import { ReportEditForm } from './ReportEditForm';
import * as styled from './styled';
import { ReportFormSchema } from './validation';

export const ReportEditPage = () => {
  const [{ userInfo }] = useApp();
  const { id, path1, path2 } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [, { generateReport, getReport, addReport, updateReport, findMyReports }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const hub = useApiHub();
  const navigate = useNavigate();
  const [{ getReportTemplate }] = useReportTemplates();
  const [{ isReady, settings }] = useLookup();

  const [defaultReportTemplateId, setDefaultReportTemplateId] = React.useState(0);
  const [report, setReport] = React.useState<IReportForm>(
    defaultReport(userInfo?.id ?? 0, defaultReportTemplateId),
  );

  const instance = report.instances.length ? report.instances[0] : undefined;
  const canEdit = instance ? instance.status === ReportStatusName.Pending : true;

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
    if (isReady) {
      const defaultReportTemplateId = settings.find(
        (s) => s.name === Settings.DefaultReportTemplate,
      )?.value;
      if (defaultReportTemplateId) setDefaultReportTemplateId(+defaultReportTemplateId);
      else toast.error(`Configuration settings '${Settings.DefaultReportTemplate}' is required.`);
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    // TODO: Templates don't change much and don't need to be fetched often.
    if (defaultReportTemplateId && report.templateId !== defaultReportTemplateId) {
      getReportTemplate(defaultReportTemplateId)
        .then((template) => {
          if (template) {
            setReport((report) => ({
              ...report,
              templateId: template.id,
              template: template,
            }));
          } else toast.error('There are no public report templates available.');
        })
        .catch(() => {});
    }
  }, [defaultReportTemplateId, getReportTemplate, report.templateId]);

  React.useEffect(() => {
    // When the page loads the first time the user info will change.
    if (userInfo?.id && !report.id) setReport((report) => ({ ...report, ownerId: userInfo.id }));
  }, [report.id, userInfo?.id]);

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
    // TODO: This can blow away a users' changes.
    try {
      if (message.status === ReportStatusName.Accepted) {
        const instance = await getReportInstance(message.id, true);
        if (instance) {
          setReport({
            ...report,
            instances: report.instances.map((i) => (i.id === message.id ? instance : i)),
          });
          navigate(`/reports/${report.id}/content`);
        }
      }
    } catch {}
  });

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      try {
        const originalId = values.id;
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
          const report = originalId
            ? await updateReport(values, true)
            : await addReport({ ...values, ownerId: values.ownerId ?? userInfo?.id ?? 0 });

          storeReportOutput(undefined); // Clear the preview

          if (!originalId) {
            navigate(`/reports/${report.id}${path1 ? `/${path1}` : ''}${path2 ? `/${path2}` : ''}`);
            toast.success(`Successfully created '${report.name}'.`);
          } else {
            setReport(toForm(report));
            toast.success(`Successfully updated '${report.name}'.`);
          }
        }
      } catch {}
    },
    [addReport, myReports, navigate, path1, path2, storeReportOutput, updateReport, userInfo?.id],
  );

  return (
    <styled.ReportEditPage className="report-edit-page">
      <FormikForm
        initialValues={report}
        validationSchema={ReportFormSchema}
        validateOnChange={false}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        <ReportEditContextProvider>
          <ReportEditForm />

          <ContentEditForm disabled={!canEdit} />
        </ReportEditContextProvider>
      </FormikForm>
    </styled.ReportEditPage>
  );
};
