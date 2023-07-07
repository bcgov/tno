import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useReportTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  IconButton,
  IReportTemplateModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultReportTemplate } from './constants';
import * as styled from './styled';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [, { addReportTemplate, deleteReportTemplate, getReportTemplate, updateReportTemplate }] =
    useReportTemplates();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('report');
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
    <styled.ReportForm>
      <IconButton
        iconType="back"
        label="Back to report templates"
        className="back-button"
        onClick={() => navigate('/admin/report/templates')}
      />
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
                    setActive('report');
                  }}
                  active={active === 'report'}
                />
                <Tab
                  label="Template"
                  onClick={() => {
                    setActive('template');
                  }}
                  active={active === 'template'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'report'}>
                <Col className="form-inputs">
                  <FormikText name="name" label="Name" />
                  <FormikTextArea name="description" label="Description" />
                  <Row gap="1em">
                    <FormikCheckbox label="Is Enabled" name="isEnabled" />
                    <FormikText
                      width={FieldSize.Tiny}
                      name="sortOrder"
                      label="Sort Order"
                      type="number"
                      className="sort-order"
                    />
                  </Row>
                  <Row>
                    <Col flex="2">
                      <p>
                        Enable the following options to provide sections, summaries, and charts.
                        These are used to control which options are available to users when they are
                        configuring their report. Only enable the options that are implemented in
                        the template.
                      </p>
                      <Row className="enable-options">
                        <FormikCheckbox label="Enable Sections" name="enableSections" />
                        <FormikCheckbox
                          label="Enable Section Summary"
                          name="enableSectionSummary"
                        />
                        <FormikCheckbox label="Enable Summary" name="enableSummary" />
                        <FormikCheckbox label="Enable Charts" name="enableCharts" />
                        <FormikCheckbox
                          label="Enable Charts Over Time"
                          name="enableChartsOverTime"
                        />
                      </Row>
                      <Show visible={!!values.id}>
                        <hr />
                        <Row>
                          <FormikText
                            width={FieldSize.Small}
                            disabled
                            name="updatedBy"
                            label="Updated By"
                          />
                          <FormikDatePicker
                            selectedDate={
                              !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                            }
                            onChange={noop}
                            name="updatedOn"
                            label="Updated On"
                            disabled
                            width={FieldSize.Small}
                          />
                        </Row>
                        <Row>
                          <FormikText
                            width={FieldSize.Small}
                            disabled
                            name="createdBy"
                            label="Created By"
                          />
                          <FormikDatePicker
                            selectedDate={
                              !!values.createdOn ? moment(values.createdOn).toString() : undefined
                            }
                            onChange={noop}
                            name="createdOn"
                            label="Created On"
                            disabled
                            width={FieldSize.Small}
                          />
                        </Row>
                      </Show>
                    </Col>
                  </Row>
                </Col>
              </Show>
              <Show visible={active === 'template'}>
                <h2>{values.name}</h2>
                <Col className="code frm-in">
                  <label htmlFor="txa-subject">Subject Template</label>
                  <Col className="editor">
                    <Editor
                      id="txa-subject-template"
                      value={values.subject}
                      onValueChange={(code) => setFieldValue('subject', code)}
                      highlight={(code) => {
                        return highlight(code, languages.cshtml, 'razor');
                      }}
                    />
                  </Col>
                </Col>
                <Col className="code frm-in">
                  <label htmlFor="txa-template">Report Template</label>
                  <p>Editing this template will change all reports that use this template.</p>
                  <Col className="editor">
                    <Editor
                      id="txa-body-template"
                      value={values.body}
                      onValueChange={(code) => setFieldValue('body', code)}
                      highlight={(code) => {
                        return highlight(code, languages.cshtml, 'razor');
                      }}
                    />
                  </Col>
                </Col>
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
    </styled.ReportForm>
  );
};
