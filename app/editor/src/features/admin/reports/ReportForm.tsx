import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useReports, useUsers } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  IconButton,
  IReportModel,
  Modal,
  ReportTypeName,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultReport } from './constants';
import { ReportFormDetails } from './ReportFormDetails';
import { ReportFormFilter } from './ReportFormFilter';
import { ReportFormInstance } from './ReportFormInstances';
import { ReportFormPreview } from './ReportFormPreview';
import { ReportFormSubscribers } from './ReportFormSubscribers';
import { ReportFormTemplate } from './ReportFormTemplate';
import { ReportSections } from './ReportSections';
import * as styled from './styled';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addReport, deleteReport, getReport, updateReport, publishReport }] = useReports();
  const [{ reportTemplates }, { storeReportTemplates }] = useAdminStore();
  const { toggle, isShowing } = useModal();
  const [{ users }, { findUsers }] = useUsers();

  const [report, setReport] = React.useState<IReportModel>({
    ...defaultReport,
    ownerId: userInfo?.id ?? 0,
  });
  const [active, setActive] = React.useState('report');

  const reportId = Number(id);

  React.useEffect(() => {
    if (!users.items.length)
      findUsers({}).catch(() => {
        // Handled already.
      });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (!!reportId && report?.id !== reportId) {
      setReport({ ...defaultReport, id: reportId }); // Do this to stop double fetch.
      getReport(reportId).then((data) => {
        setReport(data);
      });
    }
  }, [getReport, report?.id, reportId]);

  const handleSubmit = async (values: IReportModel) => {
    try {
      const originalId = values.id;
      const result = !report.id ? await addReport(values) : await updateReport(values);
      setReport(result);

      if (!reportTemplates.some((rt) => rt.id === result.templateId)) {
        const templates = [...reportTemplates, result.template];
        storeReportTemplates(templates);
      }
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/reports/${result.id}`);
    } catch {}
  };

  const handlePublish = async (values: IReportModel) => {
    try {
      await publishReport(values);
      toast.success('Report has been successfully requested');
    } catch {}
  };

  return (
    <styled.ReportForm>
      <IconButton
        iconType="back"
        label="Back to reports"
        className="back-button"
        onClick={() => navigate('/admin/reports')}
      />
      <FormikForm
        initialValues={report}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <Tabs
            tabs={
              <>
                <Tab
                  label="Report"
                  onClick={() => {
                    setActive('report');
                  }}
                  active={active === 'report'}
                />
                <Show visible={values.reportType === ReportTypeName.Filter}>
                  <Tab
                    label="Primary Filter"
                    onClick={() => {
                      setActive('filter');
                    }}
                    active={active === 'filter'}
                  />
                </Show>
                <Tab
                  label="Sections"
                  onClick={() => {
                    setActive('sections');
                  }}
                  active={active === 'sections'}
                />
                <Tab
                  label="Template"
                  onClick={() => {
                    setActive('template');
                  }}
                  active={active === 'template'}
                />
                <Tab
                  label="Preview"
                  onClick={() => {
                    setActive('preview');
                  }}
                  active={active === 'preview'}
                />
                <Tab
                  label="Subscribers"
                  onClick={() => {
                    setActive('subscribers');
                  }}
                  active={active === 'subscribers'}
                />
                <Tab
                  label="Instances"
                  onClick={() => {
                    setActive('instances');
                  }}
                  active={active === 'instances'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'report'}>
                <ReportFormDetails />
              </Show>
              <Show visible={active === 'filter'}>
                <ReportFormFilter />
              </Show>
              <Show visible={active === 'sections'}>
                <h2>{values.name}</h2>
                <ReportSections />
              </Show>
              <Show visible={active === 'template'}>
                <ReportFormTemplate />
              </Show>
              <Show visible={active === 'preview'}>
                <ReportFormPreview />
              </Show>
              <Show visible={active === 'subscribers'}>
                <ReportFormSubscribers />
              </Show>
              <Show visible={active === 'instances'}>
                <ReportFormInstance />
              </Show>
              <Row justifyContent="center" className="form-inputs">
                <Button type="submit" disabled={isSubmitting}>
                  Save
                </Button>
                <Button variant={ButtonVariant.secondary} onClick={() => handlePublish(values)}>
                  Send
                </Button>
                <Show visible={!!values.id}>
                  <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                    Delete
                  </Button>
                </Show>
              </Row>
              <Modal
                headerText="Confirm Removal"
                body="Are you sure you wish to remove this report?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await deleteReport(report);
                    toast.success(`${report.name} has successfully been deleted.`);
                    navigate('/admin/reports');
                  } catch {
                    // Globally handled
                  } finally {
                    toggle();
                  }
                }}
              />
            </div>
          </Tabs>
        )}
      </FormikForm>
    </styled.ReportForm>
  );
};
