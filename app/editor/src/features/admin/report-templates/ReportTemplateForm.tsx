import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  IconButton,
  IReportTemplateModel,
  Modal,
  ReportTypeName,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultReportTemplate } from './constants';
import { ReportTemplateFormCharts } from './ReportTemplateFormCharts';
import { ReportTemplateFormDetails } from './ReportTemplateFormDetails';
import { ReportTemplateFormTemplate } from './ReportTemplateFormTemplate';
import * as styled from './styled';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
const ReportTemplateForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [, { addReportTemplate, deleteReportTemplate, getReportTemplate, updateReportTemplate }] =
    useReportTemplates();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('details');
  const [reportTemplate, setReportTemplate] = React.useState<IReportTemplateModel>({
    ...defaultReportTemplate,
  });

  const reportTemplateId = Number(id);

  React.useEffect(() => {
    if (!!reportTemplateId && reportTemplate?.id !== reportTemplateId) {
      setReportTemplate({ ...defaultReportTemplate, id: reportTemplateId }); // Do this to stop double fetch.
      getReportTemplate(reportTemplateId).then((data) => {
        setReportTemplate(data);
      });
    }
  }, [getReportTemplate, reportTemplate?.id, reportTemplateId]);

  const handleSubmit = async (values: IReportTemplateModel) => {
    try {
      const originalId = values.id;
      const result = !reportTemplate.id
        ? await addReportTemplate(values)
        : await updateReportTemplate(values);
      setReportTemplate(result);

      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/report/templates/${result.id}`);
    } catch {}
  };

  return (
    <styled.ReportTemplateForm>
      <IconButton
        iconType="back"
        label="Back to report templates"
        className="back-button"
        onClick={() => navigate('/admin/report/templates')}
      />
      <Row alignSelf="flex-start">
        <p>
          A report template uses Razor syntax to dynamically generate HTML from a collection of
          content.
        </p>
      </Row>
      <FormikForm
        initialValues={reportTemplate}
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
                  label="Details"
                  onClick={() => {
                    setActive('details');
                  }}
                  active={active === 'details'}
                />
                <Tab
                  label="Template"
                  onClick={() => {
                    setActive('template');
                  }}
                  active={active === 'template'}
                />
                <Show visible={values.reportType === ReportTypeName.Content}>
                  <Tab
                    label="Charts"
                    onClick={() => {
                      setActive('charts');
                    }}
                    active={active === 'charts'}
                  />
                </Show>
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'details'}>
                <ReportTemplateFormDetails />
              </Show>
              <Show visible={active === 'template'}>
                <ReportTemplateFormTemplate />
              </Show>
              <Show visible={active === 'charts'}>
                <ReportTemplateFormCharts />
              </Show>
              <Row justifyContent="center" className="form-inputs">
                <Button type="submit" disabled={isSubmitting}>
                  Save
                </Button>
                <Show visible={!!values.id}>
                  <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                    Delete
                  </Button>
                </Show>
              </Row>
              <Modal
                headerText="Confirm Removal"
                body="Are you sure you wish to remove this report template?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await deleteReportTemplate(reportTemplate);
                    toast.success(`${reportTemplate.name} has successfully been deleted.`);
                    navigate('/admin/report/templates');
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
    </styled.ReportTemplateForm>
  );
};

export default ReportTemplateForm;
