import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useReports } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  hasErrors,
  IconButton,
  IReportModel,
  IUserReportModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
  useTabValidationToasts,
} from 'tno-core';

import { defaultReport, generateScheduleName } from './constants';
import { ReportFormDetails } from './ReportFormDetails';
import { ReportFormImportExport } from './ReportFormImportExport';
import { ReportFormInstance } from './ReportFormInstances';
import { ReportFormPreview } from './ReportFormPreview';
import { ReportFormScheduler } from './ReportFormScheduler';
import { ReportFormSections } from './ReportFormSections';
import { ReportFormSubscribers } from './ReportFormSubscribers';
import { ReportFormTemplate } from './ReportFormTemplate';
import { ReportTemplateContextProvider } from './ReportTemplateContext';
import * as styled from './styled';
import { ReportFormSchema } from './validation/ReportFormSchema';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
const ReportForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { addReport, deleteReport, getReport, updateReport, publishReport }] = useReports();
  const [{ reportTemplates }, { storeReportTemplates }] = useAdminStore();
  const { toggle, isShowing } = useModal();
  const { setShowValidationToast } = useTabValidationToasts();

  const [report, setReport] = React.useState<IReportModel>({
    ...defaultReport,
    ownerId: userInfo?.id,
  });
  const [active, setActive] = React.useState('report');

  const [savePressed, setSavePressed] = React.useState(false);

  const reportId = Number(id);

  React.useEffect(() => {
    if (!!reportId && report?.id !== reportId) {
      setReport({ ...defaultReport, id: reportId }); // Do this to stop double fetch.
      getReport(reportId)
        .then((data) => {
          setReport(data);
        })
        .catch(() => {});
    }
  }, [getReport, report?.id, reportId]);

  const handleSubmit = async (values: IReportModel) => {
    try {
      const originalId = values.id;
      // Update event schedule information because names must be unique.
      const report = {
        ...values,
        events: [
          ...values.events.map((s, i) => ({
            ...s,
            name: generateScheduleName(`Schedule ${i + 1}`, values),
            description: values.description,
            requestedById: values.ownerId,
          })),
        ],
      };
      const result = !values.id ? await addReport(report) : await updateReport(report);
      setReport(result);

      if (!reportTemplates.some((rt) => rt.id === result.templateId) && result.template) {
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

  const hasSubscribers = (subscribers: IUserReportModel[]) => {
    return subscribers.some((s) => s.isSubscribed);
  };

  return (
    <styled.ReportForm>
      <FormikForm
        initialValues={report}
        validationSchema={ReportFormSchema}
        validateOnChange={false}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, errors, isSubmitting }) => (
          <>
            <Row className="form-actions">
              <IconButton
                iconType="back"
                label="Back to reports"
                className="back-button"
                onClick={() => navigate('/admin/reports')}
              />
              <Col flex="1"></Col>
              <Button
                type="submit"
                disabled={isSubmitting}
                onClick={() => {
                  setSavePressed(true);
                }}
              >
                Save
              </Button>
              <Button
                variant={ButtonVariant.secondary}
                onClick={() => handlePublish(values)}
                disabled={!hasSubscribers(values.subscribers)}
              >
                Send
              </Button>
              <Show visible={!!values.id}>
                <Button
                  variant={ButtonVariant.secondary}
                  onClick={() =>
                    handleSubmit({
                      ...values,
                      id: 0,
                      name: `${values.name}-${new Date().getTime()}`,
                      ownerId: userInfo?.id,
                      version: 0,
                      sections: values.sections.map((section) => ({
                        ...section,
                        id: 0,
                        reportId: 0,
                        version: 0,
                      })),
                      subscribers: [],
                      instances: [],
                      events: [],
                    })
                  }
                >
                  Copy
                </Button>
                <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
            </Row>
            <ReportTemplateContextProvider>
              <Tabs
                tabs={
                  <>
                    <Tab
                      label="Report"
                      showErrorOnSave={{ value: true, savePressed: savePressed }}
                      setShowValidationToast={setShowValidationToast}
                      hasErrors={hasErrors(errors, ['name'])}
                      onClick={() => {
                        setActive('report');
                      }}
                      active={active === 'report'}
                    />
                    <Tab
                      label="Template"
                      showErrorOnSave={{ value: true, savePressed: savePressed }}
                      setShowValidationToast={setShowValidationToast}
                      hasErrors={hasErrors(errors, ['templateId'])}
                      onClick={() => {
                        setActive('template');
                      }}
                      active={active === 'template'}
                    />
                    {!!values.templateId && (
                      <>
                        <Tab
                          label="Sections"
                          showErrorOnSave={{ value: true, savePressed: savePressed }}
                          setShowValidationToast={setShowValidationToast}
                          hasErrors={hasErrors(errors, ['settings'])}
                          onClick={() => {
                            setActive('sections');
                          }}
                          active={active === 'sections'}
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
                          label="Scheduler"
                          showErrorOnSave={{ value: true, savePressed: savePressed }}
                          setShowValidationToast={setShowValidationToast}
                          hasErrors={hasErrors(errors, ['events'])}
                          onClick={() => {
                            setActive('scheduler');
                          }}
                          active={active === 'scheduler'}
                        />
                        <Tab
                          label="Instances"
                          onClick={() => {
                            setActive('instances');
                          }}
                          active={active === 'instances'}
                        />
                      </>
                    )}
                    {/* <Tab
                      label="Import/Export"
                      onClick={() => {
                        setActive('importexport');
                      }}
                      active={active === 'importexport'}
                    /> */}
                  </>
                }
              >
                <div className="form-container">
                  <Show visible={active === 'report'}>
                    <ReportFormDetails />
                  </Show>
                  <Show visible={active === 'template'}>
                    <ReportFormTemplate />
                  </Show>
                  <Show visible={active === 'sections'}>
                    <ReportFormSections />
                  </Show>
                  <Show visible={active === 'preview'}>
                    <ReportFormPreview />
                  </Show>
                  <Show visible={active === 'subscribers'}>
                    <ReportFormSubscribers />
                  </Show>
                  <Show visible={active === 'scheduler'}>
                    <ReportFormScheduler />
                  </Show>
                  <Show visible={active === 'instances'}>
                    <ReportFormInstance />
                  </Show>
                  <Show visible={active === 'importexport'}>
                    <ReportFormImportExport />
                  </Show>
                  <Row justifyContent="center" className="form-actions">
                    <Button
                      type="submit"
                      disabled={isSubmitting}
                      onClick={() => {
                        setSavePressed(true);
                      }}
                    >
                      Save
                    </Button>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => handlePublish(values)}
                      disabled={!hasSubscribers(values.subscribers)}
                    >
                      Send
                    </Button>
                    <Show visible={!!values.id}>
                      <Button
                        onClick={toggle}
                        variant={ButtonVariant.danger}
                        disabled={isSubmitting}
                      >
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
            </ReportTemplateContextProvider>
          </>
        )}
      </FormikForm>
    </styled.ReportForm>
  );
};

export default ReportForm;
