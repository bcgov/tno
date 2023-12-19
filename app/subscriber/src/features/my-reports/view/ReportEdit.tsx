import { Action } from 'components/action';
import { Button } from 'components/button';
import { FormikForm } from 'components/formik';
import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { FaArrowLeft, FaCloud, FaFileCirclePlus, FaFileExcel, FaGear } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useApp, useReportInstances, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  IReportMessageModel,
  MessageTargetName,
  Modal,
  ReportStatusName,
  Row,
  useModal,
} from 'tno-core';

import { ReportFormSchema } from '../admin/validation/ReportFormSchema';
import { defaultReport } from '../constants';
import { IReportForm } from '../interfaces';
import { toForm } from '../utils';
import { ReportEditForm } from './ReportEditForm';
import * as styled from './styled';

export const ReportEdit: React.FC = () => {
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ myReports }, { storeReportOutput }] = useProfileStore();
  const [{ generateReport, getReport, updateReport, findMyReports }] = useReports();
  const [{ exportReport, getReportInstance }] = useReportInstances();
  const { isShowing, toggle } = useModal();
  const hub = useApiHub();

  const [report, setReport] = React.useState<IReportForm>(defaultReport(userInfo?.id ?? 0, 0));
  const canEdit = report.instances.length ? !report.instances[0].sentOn : true;

  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    const reportId = parseInt(id ?? '0');
    if (reportId)
      getReport(reportId, true)
        .then(async (report) => {
          if (report) {
            setReport(toForm(report));
            if (!report.instances.length) {
              // The report has either never generated an instance, or the last instance was already sent.
              const result = await generateReport(reportId);
              setReport(toForm(result, true));
            }
          }
        })
        .catch(() => {});
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
      <Header />
      <FormikForm
        initialValues={report}
        validationSchema={ReportFormSchema}
        validateOnChange={false}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ submitForm, isSubmitting, values }) => (
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
                    {canEdit ? (
                      <Button onClick={() => submitForm()} disabled={isSubmitting || !canEdit}>
                        Save
                        <FaCloud />
                      </Button>
                    ) : (
                      <Button
                        onClick={() => handleRegenerate(values, false)}
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
            <ReportEditForm disabled={!canEdit} />
            <span></span>
          </PageSection>
        )}
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
