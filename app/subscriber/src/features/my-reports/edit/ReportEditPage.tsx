import { FormikForm } from 'components/formik';
import { PageSection } from 'components/section';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import {
  useApiHub,
  useApp,
  useReportInstances,
  useReports,
  useReportTemplates,
  useSettings,
} from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IReportMessageModel, MessageTargetKey, ReportStatusName, Show } from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import { sanitizeReport, sortContent, toForm } from '../utils';
import { ContentEditForm } from './content';
import { ReportEditContextProvider } from './ReportEditContext';
import { ReportEditForm } from './ReportEditForm';
import * as styled from './styled';
import { ReportFormSchema } from './validation';
import { ReportContainer } from './view';

/**
 * Provides component to administer a report template, to manage the content in the report, and send the report to subscribers.
 * @returns Component
 */
export const ReportEditPage = () => {
  const [{ userInfo }] = useApp();
  const { id, path1, path2 } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [, { generateReport, getReport, addReport, updateReport, findMyReports }] = useReports();
  const [{ getReportInstance }] = useReportInstances();
  const hub = useApiHub();
  const navigate = useNavigate();
  const [{ getReportTemplate }] = useReportTemplates();
  const { defaultReportTemplateId } = useSettings();

  const editRef = React.useRef<HTMLDivElement>(null);
  const contentRef = React.useRef<HTMLDivElement>(null);

  const [report, setReport] = React.useState<IReportForm>(
    defaultReport(userInfo?.id ?? 0, defaultReportTemplateId ?? 0),
  );
  const [autoGeneratePending, setAutoGeneratePending] = React.useState(false);

  const instance = report.instances.length ? report.instances[0] : undefined;
  const canEdit = instance
    ? [
        ReportStatusName.Pending,
        ReportStatusName.Reopen,
        ReportStatusName.Failed,
        ReportStatusName.Cancelled,
      ].includes(instance.status)
    : true;

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
    // Only fetch the template for new reports.
    if (userInfo && defaultReportTemplateId && report.templateId !== defaultReportTemplateId) {
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
  }, [userInfo, defaultReportTemplateId, getReportTemplate, report.id, report.templateId]);

  React.useEffect(() => {
    // When the page loads the first time the user info will change.
    // Make sure the report is assigned to them as the owner.
    if (userInfo?.id && !report.id) setReport((report) => ({ ...report, ownerId: userInfo.id }));
  }, [report.id, userInfo?.id]);

  // Each time report data is refreshed, check if instances for report are loaded,
  // if not, call generateReport to populate instances array
  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (!report || !reportId || report.id !== reportId) {
      setAutoGeneratePending(false);
      return;
    }

    if (report.instances?.length) {
      if (autoGeneratePending) setAutoGeneratePending(false);
      return;
    }

    if (autoGeneratePending) return;

    setAutoGeneratePending(true);
    generateReport(reportId)
      .then((result) => {
        setReport(toForm(result, true));
      })
      .catch(() => {
        setAutoGeneratePending(false);
      });
    // Only generate a report if it's missing an instance.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [report, autoGeneratePending]);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    const originalReportSections = report.sections;
    if (userInfo && !!reportId && myReports?.length) {
      const existingReport = myReports.find((r) => r.id === reportId);
      const hasFetchedContent = existingReport?.instances.some((r) => r.content?.length);
      if (existingReport && hasFetchedContent) {
        // Case 1: Complete data: do nothing
        // We have existing report & it has instance content loaded already
        setReport(toForm(existingReport));
      } else if (existingReport && !hasFetchedContent) {
        // Case 2: Partial Data: call generateReport to get missing data
        // We have existing report loaded, but no content present in instance data
        setAutoGeneratePending(false);
        setReport(toForm({ ...existingReport, instances: [] }));
      } else {
        // Case 3: No report data present, fetch entire report + instance + content data
        getReport(reportId, true)
          .then(async (report) => {
            if (report) {
              setReport(
                toForm({
                  ...report,
                  sections: report.sections.map((section, index) => {
                    const originalSection =
                      originalReportSections.length > index
                        ? originalReportSections[index]
                        : undefined;
                    return { ...section, open: originalSection?.open };
                  }),
                }),
              );
            }
          })
          .catch(() => {
            setReport((report) => ({ ...report, id: -1 }));
          });
      }
    }
    // Only make a request when 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, myReports.length, userInfo]);

  hub.useHubEffect(MessageTargetKey.ReportStatus, async (message: IReportMessageModel) => {
    // Report has been updated, go fetch latest.
    // TODO: This can blow away a users' changes.
    try {
      if (message.reportId === report.id) {
        if (message.message === 'status') {
          setReport({
            ...report,
            instances: report.instances.map((i) =>
              i.id === message.id ? { ...i, status: message.status, version: message.version } : i,
            ),
          });
        } else if (message.message === 'event') {
          const updateReport = await getReport(report.id, false);
          if (updateReport) {
            setReport({
              ...report,
              events: updateReport.events,
              version: updateReport.version,
            });
          }
        } else {
          const instance = await getReportInstance(message.id, true);
          if (instance) {
            // If there is a new instance, prepend it.
            const add = !report.instances.some((i) => i.id === message.id);
            setReport({
              ...report,
              instances: add
                ? [instance, ...report.instances]
                : report.instances.map((i) => (i.id === message.id ? instance : i)),
            });
            navigate(`/reports/${report.id}/content`);
          }
        }
      }
    } catch {}
  });

  const handleSubmit = React.useCallback(
    async (values: IReportForm) => {
      const sanitizedValues = sanitizeReport(values);
      try {
        const originalId = sanitizedValues.id;
        const originalReportSections = sanitizedValues.sections;
        const sameNameReport = myReports.some(
          (r) =>
            r.name.trim().toLocaleLowerCase() === sanitizedValues.name.trim().toLocaleLowerCase() &&
            r.id !== sanitizedValues.id,
        );
        if (sameNameReport) {
          toast.error(`A report with the name '${sanitizedValues.name}' already exists.`);
        } else {
          if (sanitizedValues.instances.length) {
            // Apply new sort order values for content to stop content from moving around when it has the same sort order value.
            sanitizedValues.instances[0] = {
              ...sanitizedValues.instances[0],
              content: sortContent(sanitizedValues.instances[0].content, true),
            };
          }
          const report = originalId
            ? await updateReport(
                sanitizedValues,
                instance &&
                  [
                    ReportStatusName.Pending,
                    ReportStatusName.Reopen,
                    ReportStatusName.Failed,
                    ReportStatusName.Cancelled,
                  ].includes(instance.status),
              )
            : await addReport({
                ...sanitizedValues,
                ownerId: sanitizedValues.ownerId ?? userInfo?.id ?? 0,
                settings: {
                  ...sanitizedValues.settings,
                  subject: {
                    ...sanitizedValues.settings.subject,
                    text: sanitizedValues.settings.subject.text.length // Default email subject line
                      ? sanitizedValues.settings.subject.text
                      : sanitizedValues.name,
                  },
                },
              });

          storeReportOutput(undefined); // Clear the preview

          if (!originalId) {
            navigate(`/reports/${report.id}${path1 ? `/${path1}` : ''}${path2 ? `/${path2}` : ''}`);
            toast.success(`Successfully created '${report.name}'.`);
          } else {
            setReport(
              toForm({
                ...report,
                sections: report.sections.map((section, index) => {
                  const originalSection =
                    originalReportSections.length > index
                      ? originalReportSections[index]
                      : undefined;
                  return { ...section, open: originalSection?.open };
                }),
              }),
            );
            toast.success(`Successfully updated '${report.name}'.`);
          }
        }
      } catch (ex) {
        console.error(ex);
      }
    },
    [
      addReport,
      instance,
      myReports,
      navigate,
      path1,
      path2,
      storeReportOutput,
      updateReport,
      userInfo?.id,
    ],
  );

  return (
    <styled.ReportEditPage className="report-edit-page">
      <Show visible={report.id === -1}>
        <PageSection>
          <Col alignContent="center" justifyContent="center">
            Report does not exist
          </Col>
        </PageSection>
      </Show>
      <Show visible={report.id !== -1}>
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
            <ReportEditForm
              ref={editRef}
              disabled={!canEdit}
              updateForm={(form) => setReport(form)}
              onContentClick={(content) => {
                if (contentRef.current)
                  contentRef.current.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                    inline: 'nearest',
                  });
                else if (editRef.current) editRef.current.scrollIntoView();
              }}
            />
            <ContentEditForm disabled={!canEdit} ref={contentRef} />
            <ReportContainer />
          </ReportEditContextProvider>
        </FormikForm>
      </Show>
    </styled.ReportEditPage>
  );
};
