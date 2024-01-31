import { Action } from 'components/action';
import { Button } from 'components/button';
import { FormikForm } from 'components/formik';
import { PageSection } from 'components/section';
import React from 'react';
import { FaArrowLeft, FaCloud, FaFileCirclePlus, FaFileExcel, FaGear } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useContent, useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  IReportMessageModel,
  MessageTargetName,
  Modal,
  ReportStatusName,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { ReportFormSchema } from '../admin/validation/ReportFormSchema';
import { defaultReport } from '../constants';
import { IReportForm, IReportInstanceContentForm } from '../interfaces';
import { sortContent, toForm } from '../utils';
import { ContentForm, ReportEditForm, UserContentForm } from './components';
import * as styled from './styled';

export const ReportEdit: React.FC = () => {
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [, { generateReport, getReport, updateReport, findMyReports }] = useReports();
  const [{ exportReport, getReportInstance }] = useReportInstances();
  const [, { addContent, updateContent }] = useContent();
  const { isShowing, toggle } = useModal();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportForm>(defaultReport(userInfo?.id ?? 0, 0));
  const [showEdit, setShowEdit] = React.useState<IReportInstanceContentForm>();
  const [loading, setLoading] = React.useState(false);

  const userId = userInfo?.id ?? 0;

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

  const handleExport = React.useCallback(
    async (report: IReportForm) => {
      try {
        if (report?.id) {
          const instance = report.instances.length ? report.instances[0] : 0;
          if (instance) {
            const filename = report.name.replace(/[^a-zA-Z0-9 ]/g, '');
            await toast.promise(exportReport(instance.id, filename), {
              pending: 'Downloading file',
              success: 'Download complete',
              error: 'Download failed',
            });
          } else {
            toast.error(`The report has not been generated.`);
          }
        }
      } catch {}
    },
    [exportReport],
  );

  const handleRegenerate = React.useCallback(
    async (values: IReportForm, regenerate: boolean) => {
      try {
        const report = await generateReport(values.id, regenerate);
        setReport(toForm(report, true));
        if (regenerate) toast.success('Report has been regenerated');
        else {
          toast.success('Report has been generated');
          navigate(`/reports/${values.id}/edit/content`);
        }
      } catch {}
    },
    [generateReport, navigate],
  );

  const handleAddUpdateContent = React.useCallback(
    async (values: IReportForm, row: IReportInstanceContentForm) => {
      try {
        setLoading(true);
        const content = row.content;

        if (!content) return null;

        const originalId = content.id;
        const contentResult = !content.id
          ? await addContent(content)
          : await updateContent(content);
        if (contentResult) {
          const instanceContent: IReportInstanceContentForm = {
            contentId: contentResult.id,
            content: contentResult,
            instanceId: row.instanceId,
            sectionName: row.sectionName,
            sortOrder: 0,
            originalIndex: row.originalIndex,
          };
          setShowEdit(instanceContent);
          if (!originalId) {
            // Added content needs to update the report instance.
            const instance = values.instances.length ? values.instances[0] : undefined;
            if (!instance) return null;

            // Resort section and place new content at the beginning.
            const sectionIndex = instance.content.findIndex(
              (c) => c.sectionName === row.sectionName,
            );
            if (sectionIndex === -1) return null;

            instance.content.splice(sectionIndex, 0, instanceContent);
            let contentIndex = 0;

            const updatedReport = {
              ...values,
              instances: values.instances.map((instance) => ({
                ...instance,
                content: instance.content.map((c, i) => {
                  if (c.sectionName === row.sectionName) {
                    return { ...c, sortOrder: contentIndex++ };
                  }
                  return c;
                }),
              })),
            };

            // Update the report instances with the latest content.
            const reportResult = await updateReport(updatedReport, true);
            return toForm(reportResult);
          } else {
            return {
              ...values,
              instances: values.instances.map((instance, index) =>
                index === 0
                  ? {
                      ...instance,
                      content: instance.content.map((c) =>
                        c.contentId === contentResult.id ? { ...c, content: contentResult } : c,
                      ),
                    }
                  : instance,
              ),
            };
          }
        }
      } catch {
      } finally {
        setLoading(false);
      }
    },
    [addContent, updateContent, updateReport],
  );

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
        {({ submitForm, isSubmitting, values, setValues, setSubmitting }) => {
          const instance = values.instances.length ? values.instances[0] : undefined;
          const canEdit = instance ? instance.status === ReportStatusName.Pending : true;
          return (
            <Row>
              <Col flex="1">
                <PageSection
                  header={
                    <Row flex="1" alignItems="center" gap="1rem">
                      <Col flex="1" gap="0.5rem">
                        <Row>
                          <Action
                            icon={<FaArrowLeft />}
                            label="Back to my reports"
                            onClick={() => navigate('/reports')}
                          />
                        </Row>
                        <Row alignItems="center">
                          <label>Edit Report</label>
                        </Row>
                      </Col>
                      <Col gap="0.5rem">
                        <Row gap="1rem" justifyContent="flex-end">
                          <Action
                            disabled={isSubmitting}
                            icon={<FaGear />}
                            title="Edit report template"
                            onClick={(e) => {
                              if (e.ctrlKey) toggle();
                              else navigate(`/reports/${values.id}`);
                            }}
                          />
                          <Action
                            disabled={isSubmitting}
                            icon={<FaFileExcel />}
                            title="Export to Excel"
                            onClick={() => handleExport(values)}
                          />
                          {canEdit || instance?.status === ReportStatusName.Submitted ? (
                            <Button
                              onClick={() => submitForm()}
                              disabled={isSubmitting || !canEdit}
                            >
                              Save
                              <FaCloud />
                            </Button>
                          ) : (
                            <Button
                              onClick={() => handleRegenerate(values, true)}
                              disabled={isSubmitting || canEdit}
                            >
                              Start next report
                              <FaFileCirclePlus />
                            </Button>
                          )}
                        </Row>
                      </Col>
                    </Row>
                  }
                >
                  <ReportEditForm
                    disabled={!canEdit}
                    showAdd={!showEdit}
                    onContentClick={(content) => {
                      setShowEdit(content);
                    }}
                  />
                  <span></span>
                </PageSection>
              </Col>
              {showEdit && (
                <Col flex="1">
                  <PageSection
                    header={
                      <Col flex="1">
                        <Row flex="1" alignItems="center" gap="1rem">
                          <Col flex="1" gap="0.5rem">
                            <Row alignItems="center">
                              <label>Story Preview</label>
                            </Row>
                          </Col>
                          <Col gap="0.5rem">
                            <Row gap="1rem" justifyContent="flex-end">
                              <Button
                                onClick={() => setShowEdit(undefined)}
                                disabled={isSubmitting || !canEdit}
                                variant="secondary"
                              >
                                Cancel
                              </Button>
                              <Button
                                onClick={async () => {
                                  // Save the updated story and then apply the results to the report.
                                  if (showEdit.content) {
                                    try {
                                      setSubmitting(true);
                                      const result = await handleAddUpdateContent(values, showEdit);
                                      if (result) setValues(result);
                                    } catch {
                                    } finally {
                                      setSubmitting(false);
                                    }
                                  }
                                }}
                                disabled={isSubmitting || !canEdit}
                              >
                                Save edits
                                <FaCloud />
                              </Button>
                            </Row>
                          </Col>
                        </Row>
                        <Row className="sub-title">
                          <Show visible={!!showEdit.contentId}>
                            <Col>
                              <label className="h2">Editing this story:</label>
                              <p>
                                Any changes made to the headline or story will be reflected in your
                                reports.
                              </p>
                            </Col>
                          </Show>
                          <Show visible={!showEdit.contentId}>
                            <Col>
                              <label className="h2">Add this story:</label>
                            </Col>
                          </Show>
                        </Row>
                      </Col>
                    }
                  >
                    {showEdit.content?.ownerId === userId && showEdit.content?.isPrivate ? (
                      <UserContentForm
                        content={showEdit}
                        show={'all'}
                        onContentChange={(content) => {
                          setShowEdit({ ...content });
                        }}
                        loading={loading}
                      />
                    ) : (
                      <ContentForm
                        content={showEdit.content}
                        show={'all'}
                        onContentChange={(content) => {
                          setShowEdit({ ...showEdit, content });
                        }}
                        loading={loading}
                      />
                    )}
                  </PageSection>
                </Col>
              )}
            </Row>
          );
        }}
      </FormikForm>
      <Modal
        headerText="Regenerate Report"
        body="Regenerating a report will rerun all filters and update content in the report.  Do you want to proceed?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            await handleRegenerate(report, true);
          } finally {
            toggle();
          }
        }}
      />
    </styled.ReportEdit>
  );
};
