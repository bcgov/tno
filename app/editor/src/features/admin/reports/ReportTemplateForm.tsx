import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useChartTemplates, useReportTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  IChartTemplateModel,
  IconButton,
  IReportTemplateModel,
  Modal,
  OptionItem,
  Row,
  Select,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultRazorTemplate, defaultReportTemplate } from './constants';
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
  const [{ chartTemplates }, { findAllChartTemplates }] = useChartTemplates();

  const [chartOptions, setChartOptions] = React.useState(getSortableOptions(chartTemplates));
  const [chart, setChart] = React.useState<IChartTemplateModel>();
  const [active, setActive] = React.useState('report');
  const [reportTemplate, setReportTemplate] = React.useState<IReportTemplateModel>({
    ...defaultReportTemplate,
  });

  const reportTemplateId = Number(id);

  React.useEffect(() => {
    if (!chartTemplates.length)
      findAllChartTemplates()
        .then((results) => setChartOptions(getSortableOptions(results)))
        .catch();
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
                <Tab
                  label="Charts"
                  onClick={() => {
                    setActive('charts');
                  }}
                  active={active === 'charts'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'report'}>
                <Col className="form-inputs">
                  <FormikText name="name" label="Name" required />
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
              </Show>
              <Show visible={active === 'template'}>
                <h2>{values.name}</h2>
                <Row>
                  <Col flex="1">
                    <p>Editing this template will change all reports that use this template.</p>
                  </Col>
                  <Button
                    variant={ButtonVariant.secondary}
                    onClick={() => setFieldValue('body', defaultRazorTemplate)}
                  >
                    Use Default Template
                  </Button>
                </Row>
                <Col className="code frm-in">
                  <label htmlFor="txa-subject" className="required">
                    Subject Template
                  </label>
                  <Col className="editor">
                    <Editor
                      id="txa-subject-template"
                      required
                      value={values.subject}
                      onValueChange={(code) => setFieldValue('subject', code)}
                      highlight={(code) => {
                        return highlight(code, languages.cshtml, 'razor');
                      }}
                    />
                  </Col>
                </Col>
                <Col className="code frm-in">
                  <label htmlFor="txa-template" className="required">
                    Report Template
                  </label>
                  <Col className="editor">
                    <Editor
                      id="txa-body-template"
                      required
                      value={values.body}
                      onValueChange={(code) => setFieldValue('body', code)}
                      highlight={(code) => {
                        return highlight(code, languages.cshtml, 'razor');
                      }}
                    />
                  </Col>
                </Col>
              </Show>
              <Show visible={active === 'charts'}>
                <Col>
                  <h2>{values.name}</h2>
                  <p>Select the charts that are supported by this template.</p>
                </Col>
                <Row>
                  <Col flex="2" className="frm-in">
                    <label>Template Options</label>
                    <p>
                      Enable the following options to provide sections, summaries, and charts. These
                      are used to control which options are available to users when they are
                      configuring their report. Only enable the options that are implemented in the
                      template.
                    </p>
                    <Row className="enable-options">
                      <FormikCheckbox
                        label="Enable Section Charts"
                        name="settings.enableSectionCharts"
                        tooltip="This template support charts within each section"
                      />
                      <FormikCheckbox
                        label="Enable Summary Charts"
                        name="settings.enableSummaryCharts"
                        tooltip="This template supports a summary section"
                      />
                    </Row>
                  </Col>
                </Row>
                <Col>
                  <Row className="add-chart">
                    <Select
                      name="charts"
                      label="Charts"
                      options={chartOptions}
                      value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
                      onChange={(e) => {
                        const o = e as OptionItem;
                        const chart = chartTemplates.find((ct) => ct.id === o?.value);
                        setChart(chart);
                      }}
                    />
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        const charts = [...values.chartTemplates, { ...chart }].map((ct, i) => {
                          return { ...ct, sortOrder: i };
                        });
                        setFieldValue(`chartTemplates`, charts);
                        setChart(undefined);
                      }}
                      disabled={!chart || values.chartTemplates.some((ct) => ct.id === chart.id)}
                    >
                      Add Chart
                    </Button>
                  </Row>
                  <Col className="charts">
                    {values.chartTemplates.map((ct, ctIndex) => (
                      <Row key={ct.id}>
                        <Col flex="1">{ct.name}</Col>
                        <Col flex="2">{ct.description}</Col>
                        <Col>
                          <Button
                            variant={ButtonVariant.danger}
                            onClick={() => {
                              let items = [...values.chartTemplates];
                              items.splice(ctIndex, 1);
                              setFieldValue(`chartTemplates`, items);
                            }}
                          >
                            <FaTrash />
                          </Button>
                        </Col>
                      </Row>
                    ))}
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
