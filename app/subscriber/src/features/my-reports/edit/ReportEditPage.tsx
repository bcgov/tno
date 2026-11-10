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
import {
  Col,
  IReportInstanceContentModel,
  IReportMessageModel,
  MessageTargetKey,
  ReportStatusName,
  Show,
} from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import {
  isAISectionTestMessage,
  sanitizeReport,
  shouldSaveContent,
  sortContent,
  toForm,
} from '../utils';
import { ContentEditForm } from './content';
import { ReportEditContextProvider } from './ReportEditContext';
import { ReportEditForm } from './ReportEditForm';
import * as styled from './styled';
import { ReportFormSchema } from './validation';
import { ReportContainer } from './view';

/** Instances whose stories can still be changed. */
const editableStatuses = [
  ReportStatusName.Pending,
  ReportStatusName.Reopen,
  ReportStatusName.Failed,
  ReportStatusName.Cancelled,
];

/**
 * Provides component to administer a report template, to manage the content in the report, and send the report to subscribers.
 * @returns Component
 */
export const ReportEditPage = () => {
  const [{ userInfo }] = useApp();
  const { id, path1, path2 } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [
    ,
    { generateReport, getReport, addReport, updateReport, updateReportSubscribers, findMyReports },
  ] = useReports();
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
  // The stories as last received from the API, to tell whether the user has changed them.
  const contentBaseline = React.useRef<IReportInstanceContentModel[]>();

  const instance = report.instances.length ? report.instances[0] : undefined;
  const canEdit = instance ? editableStatuses.includes(instance.status) : true;

  /** Show a report received from the API, remembering its stories if they are loaded. */
  const applyReport = React.useCallback((form: IReportForm) => {
    contentBaseline.current =
      form.contentLoaded && form.instances.length ? form.instances[0].content : undefined;
    setReport(form);
  }, []);

  /** Load the current instance's stories, keeping any unsaved changes to the report. */
  const loadContent = React.useCallback(
    async (values: IReportForm) => {
      const current = values.instances.length ? values.instances[0] : undefined;
      if (!current || values.contentLoaded) return;
      const loaded = await getReportInstance(current.id, true);
      if (!loaded) return;
      applyReport(
        toForm(
          { ...values, instances: [{ ...current, ...loaded }, ...values.instances.slice(1)] },
          false,
          true,
        ),
      );
    },
    [applyReport, getReportInstance],
  );

  React.useEffect(() => {
    // The list is only needed to check report names; the form is never filled from it.
    if (!myReports?.length) {
      findMyReports().catch(() => {});
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
        applyReport(toForm(result, true));
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
    if (userInfo && !!reportId) {
      // Always fetch the report and its instance; stories load when a page needs them.
      // The form is saved back whole, so it is never filled from the copy in the store: that copy
      // can come from the report list, or be out of date, and saving it would overwrite the
      // report's sections and their charts with whatever it happened to hold.
      // Generating is only needed when the report has no instance yet (see the effect above),
      // as generating returns every story of an existing instance.
      getReport(reportId, false)
        .then(async (report) => {
          if (report) {
            applyReport(
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
    // Only make a request when 'id' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, userInfo]);

  hub.useHubEffect(MessageTargetKey.ReportStatus, async (message: IReportMessageModel) => {
    // A finished AI section test run is for the prompt workbench; nothing about the report changed.
    if (isAISectionTestMessage(message)) return;
    // Report has been updated, go fetch latest.
    // TODO: This can blow away a users' changes.
    try {
      if (message.reportId === report.id) {
        if (message.message === 'ai-sections') {
          // The preview refreshes itself when its AI sections are ready.
          return;
        } else if (message.message === 'status') {
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
          // Only fetch the stories if the page has loaded them.
          const instance = await getReportInstance(message.id, !!report.contentLoaded);
          if (instance) {
            // If there is a new instance, prepend it.
            const add = !report.instances.some((i) => i.id === message.id);
            applyReport({
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
          const current = values.instances.length ? values.instances[0] : undefined;
          // Stories are saved only when they were loaded and changed (see shouldSaveContent).
          const saveContent = shouldSaveContent(
            values,
            sanitizedValues,
            contentBaseline.current,
            !!current && editableStatuses.includes(current.status),
          );
          if (saveContent && sanitizedValues.instances.length) {
            // Apply new sort order values for content to stop content from moving around when it has the same sort order value.
            sanitizedValues.instances[0] = {
              ...sanitizedValues.instances[0],
              content: sortContent(sanitizedValues.instances[0].content, true),
            };
          }
          if (originalId) {
            // Only save subscribers that were actually changed on the subscribers form.
            // Subscriptions absent from this list are left untouched by the API.
            const changedSubscribers = sanitizedValues.subscribers.filter((subscriber) => {
              const original = report.subscribers.find((s) => s.userId === subscriber.userId);
              return (
                !original ||
                original.isSubscribed !== subscriber.isSubscribed ||
                original.format !== subscriber.format ||
                original.sendTo !== subscriber.sendTo
              );
            });
            if (changedSubscribers.length)
              await updateReportSubscribers(originalId, changedSubscribers);
          }

          const result = originalId
            ? await updateReport(sanitizedValues, saveContent)
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
            navigate(`/reports/${result.id}${path1 ? `/${path1}` : ''}${path2 ? `/${path2}` : ''}`);
            toast.success(`Successfully created '${result.name}'.`);
          } else {
            // Without its stories the API returns no instances, so keep the page's own (and any
            // stories it has loaded) rather than generating the report again.
            const keepInstances = !saveContent;
            applyReport(
              toForm(
                {
                  ...result,
                  instances: keepInstances ? sanitizedValues.instances : result.instances,
                  sections: result.sections.map((section, index) => {
                    const originalSection =
                      originalReportSections.length > index
                        ? originalReportSections[index]
                        : undefined;
                    return { ...section, open: originalSection?.open };
                  }),
                },
                false,
                keepInstances ? !!values.contentLoaded : true,
              ),
            );
            toast.success(`Successfully updated '${result.name}'.`);
          }
        }
      } catch (ex) {
        console.error(ex);
      }
    },
    [
      addReport,
      applyReport,
      myReports,
      navigate,
      path1,
      path2,
      report.subscribers,
      storeReportOutput,
      updateReport,
      updateReportSubscribers,
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
          // Validation copies the whole form, which can hold tens of thousands of stories, so it
          // only runs when the report is saved.
          validateOnChange={false}
          validateOnBlur={false}
          onSubmit={async (values, { setSubmitting }) => {
            await handleSubmit(values);
            setSubmitting(false);
          }}
        >
          <ReportEditContextProvider loadContent={loadContent}>
            <ReportEditForm
              ref={editRef}
              disabled={!canEdit}
              updateForm={applyReport}
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
