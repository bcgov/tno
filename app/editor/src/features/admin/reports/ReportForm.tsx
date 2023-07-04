import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { AxiosError } from 'axios';
import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useApp, useLookupOptions } from 'store/hooks';
import { useReports, useReportTemplates, useUsers } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FlexboxTable,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  getSortableOptions,
  IconButton,
  IOptionItem,
  IReportModel,
  IReportPreviewModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserModel,
  Modal,
  OptionItem,
  ReportTypeName,
  Row,
  Show,
  Tab,
  Tabs,
  Text,
  useModal,
} from 'tno-core';

import {
  defaultReport,
  defaultReportTemplate,
  instanceColumns,
  subscriberColumns,
} from './constants';
import { ReportFilter } from './ReportFilter';
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
  const [
    ,
    { addReport, deleteReport, getReport, updateReport, sendReport, previewReport, publishReport },
  ] = useReports();
  const [{ reportTemplates }, { storeReportTemplates }] = useAdminStore();
  const [, { findAllReportTemplates }] = useReportTemplates();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ users }, { findUsers }] = useUsers();
  const [{ productOptions }] = useLookupOptions();

  const [report, setReport] = React.useState<IReportModel>(
    (state as any)?.report ?? { ...defaultReport, ownerId: userInfo?.id ?? 0 },
  );
  const [filter, setFilter] = React.useState(JSON.stringify(report.filter, null, 2));
  const [sendTo, setSendTo] = React.useState('');
  const [active, setActive] = React.useState('report');
  const [preview, setPreview] = React.useState<IReportPreviewModel>();
  const [templateOptions, setTemplateOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(reportTemplates, [new OptionItem('[New Template]', 0)]),
  );

  const reportId = Number(id);
  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  React.useEffect(() => {
    if (!users.items.length)
      findUsers({}).catch(() => {
        // Handled already.
      });
    findAllReportTemplates()
      .then((templates) =>
        setTemplateOptions(getSortableOptions(templates, [new OptionItem('[New Template]', 0)])),
      )
      .catch(() => {
        // Handled already.
      });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (!!reportId && report?.id !== reportId) {
      setReport({ ...defaultReport, id: reportId }); // Do this to stop double fetch.
      getReport(reportId, true).then((data) => {
        setReport(data);
        setFilter(JSON.stringify(data.filter, null, 2));
      });
    }
  }, [getReport, report?.id, reportId]);

  const handleSubmit = async (values: IReportModel) => {
    try {
      const originalId = values.id;
      const result = !report.id ? await addReport(values) : await updateReport(values);
      setReport(result);
      setFilter(JSON.stringify(result.filter, null, 2));

      if (!reportTemplates.some((rt) => rt.id === result.templateId)) {
        const templates = [...reportTemplates, result.template];
        storeReportTemplates(templates);
        setTemplateOptions(getSortableOptions(templates, [new OptionItem('[New Template]', 0)]));
      }
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/reports/${result.id}`);
    } catch {}
  };

  const handleSend = async (values: IReportModel, to: string) => {
    try {
      await sendReport(values, to);
      toast.success('Report has been successfully requested');
    } catch {}
  };

  const handlePublish = async (values: IReportModel) => {
    try {
      await publishReport(values);
      toast.success('Report has been successfully requested');
    } catch {}
  };

  const handlePageChange = React.useCallback(
    async (page: ITablePage, table: ITableInternal<IUserModel>) => {
      await findUsers({ page: page.pageIndex + 1, quantity: page.pageSize });
    },
    [findUsers],
  );

  const handleSortChange = React.useCallback(
    async (sort: ITableSort<IUserModel>[], table: ITableInternal<IUserModel>) => {
      const sorts = sort
        .filter((s) => s.isSorted)
        .map((s) => `${s.id}${s.isSortedDesc ? ' desc' : ''}`);
      await findUsers({ page: 1, quantity: users.quantity, sort: sorts });
    },
    [findUsers, users.quantity],
  );

  const handlePreviewReport = React.useCallback(
    async (model: IReportModel) => {
      try {
        const response = await previewReport({
          ...model,
          instances: [],
          subscribers: [],
          owner: undefined,
        });
        setPreview(response);
      } catch (ex) {
        const error = ex as AxiosError;
        const response = error.response;
        const data = response?.data as any;
        setPreview({
          subject: data.error,
          body: `${data.details}<div>${data.stackTrace}</div>`,
          results: {},
        });
      }
    },
    [previewReport],
  );

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
                  label="Sent"
                  onClick={() => {
                    setActive('sent');
                  }}
                  active={active === 'sent'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'report'}>
                <Col className="form-inputs">
                  <FormikText
                    name="name"
                    label="Name"
                    onChange={(e) => {
                      setFieldValue('name', e.target.value);
                      if (values.templateId === 0)
                        setFieldValue(
                          'template.name',
                          `${e.target.value}-${Date.now().toString()}`,
                        );
                    }}
                  />
                  <FormikTextArea name="description" label="Description" />
                  <p>
                    A filtered report will make a request for content each time it runs. A custom
                    report is populated manually be the user.
                  </p>
                  <Row>
                    <FormikSelect
                      name="reportType"
                      label="Report Type"
                      options={reportTypeOptions}
                      width="20ch"
                      value={reportTypeOptions.filter((rt) =>
                        values.reportType.includes(rt.value as ReportTypeName),
                      )}
                      isClearable={false}
                      onChange={(newValue) => {
                        const option = newValue as IOptionItem;
                        setFieldValue('reportType', option.value);
                      }}
                    />
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
                      <FormikCheckbox label="Is Enabled" name="isEnabled" />
                      <p>
                        A public report is available for all users. If they subscribe to the report
                        they will receive a copy every time it is run.
                      </p>
                      <FormikCheckbox label="Is Public" name="isPublic" />
                      <Show visible={!!values.id}>
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
              <Show visible={active === 'filter'}>
                <Col>
                  <h2>{values.name}</h2>
                  <p>
                    A primary filter can be used to find content to include in the report. If a
                    report has sections, you can add filters to each section instead.
                  </p>
                  <div>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        setFieldValue('filter', {});
                        setFilter('{}');
                      }}
                    >
                      Clear Filter
                    </Button>
                  </div>
                  <Row>
                    <FormikText
                      name="filter.size"
                      label="Number of Stories"
                      type="number"
                      width="10ch"
                      onChange={(e) => {
                        if (!!e.target.value) {
                          const value = parseInt(e.target.value);
                          setFieldValue('filter.size', value);
                          setFilter(JSON.stringify({ ...values.filter, size: value }, null, 2));
                        }
                      }}
                    />
                    <p>
                      All filters must have a upward limit of content returned in a single request.
                      The default limit is 10.
                    </p>
                  </Row>
                  <FormikSelect
                    name="productId"
                    value={
                      productOptions.find(
                        (mt) => mt.value === values.filter?.query?.match?.productId,
                      ) ?? ''
                    }
                    onChange={(newValue: any) => {
                      if (!!newValue) {
                        const filter = {
                          ...values.filter,
                          query: {
                            ...values.filter?.query,
                            match: { ...values.filter?.query?.match, productId: newValue.value },
                          },
                        };
                        setFieldValue('filter', filter);
                        setFilter(JSON.stringify(filter, null, 2));
                      } else {
                        setFieldValue('filter.query', {});
                        setFilter(JSON.stringify({ ...values.filter, query: {} }, null, 2));
                      }
                    }}
                    label="Product"
                    width={FieldSize.Small}
                    options={productOptions}
                  />
                </Col>
                <Col className="code frm-in">
                  <label htmlFor="txa-filter">Elasticsearch Query</label>
                  <p>
                    The query is the expression that is sent to Elasticsearch to find content. Read
                    up on how to create a query on the official page{' '}
                    <a
                      href="https://www.elastic.co/guide/en/elasticsearch/reference/current/search-your-data.html"
                      target="_blank"
                      rel="noreferrer"
                    >
                      here
                    </a>
                    .
                  </p>
                  <Col className="editor">
                    <Editor
                      id="txa-filter"
                      value={filter}
                      onValueChange={(code) => {
                        setFilter(code);
                        try {
                          const json = JSON.parse(code);
                          setFieldValue('filter', json);
                        } catch {
                          // Ignore errors.
                          // TODO: Inform user of formatting issues on blur/validation.
                        }
                      }}
                      highlight={(code) => {
                        return highlight(code, languages.json, 'json');
                      }}
                    />
                  </Col>
                </Col>
              </Show>
              <Show visible={active === 'sections'}>
                <h2>{values.name}</h2>
                <ReportSections />
              </Show>
              <Show visible={active === 'template'}>
                <h2>{values.name}</h2>
                <p>Select a template to build this report with.</p>
                <Row>
                  <FormikSelect
                    name="templateId"
                    label="Template"
                    tooltip="Select a template to base this report on"
                    options={templateOptions}
                    value={templateOptions.filter(
                      (rt) => values.templateId === (rt.value === undefined ? 0 : +rt.value),
                    )}
                    isClearable={false}
                    onChange={(newValue) => {
                      const option = newValue as IOptionItem;
                      const templateId = option.value !== undefined ? +option.value : 0;
                      if (templateId) {
                        const template = reportTemplates.find((rt) => rt.id === templateId);
                        if (template) {
                          setFieldValue('templateId', template.id);
                          setFieldValue('template', template);
                        }
                      } else {
                        setFieldValue('templateId', defaultReportTemplate.id);
                        setFieldValue('template', {
                          ...defaultReportTemplate,
                          name: `${report.name}-${Date.now().toString()}`,
                        });
                      }
                    }}
                  />
                </Row>
                <Col className="code frm-in">
                  <label htmlFor="txa-subject">Subject Template</label>
                  <Col className="editor">
                    <Editor
                      id="txa-subject-template"
                      value={values.template.subject}
                      onValueChange={(code) => setFieldValue('template.subject', code)}
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
                      value={values.template.body}
                      onValueChange={(code) => setFieldValue('template.body', code)}
                      highlight={(code) => {
                        return highlight(code, languages.cshtml, 'razor');
                      }}
                    />
                  </Col>
                </Col>
              </Show>
              <Show visible={active === 'preview'}>
                <h2>{values.name}</h2>
                <Row>
                  <Col flex="1" alignItems="center" justifyContent="center">
                    <p>
                      Before saving the report, generate a preview to ensure it is working and
                      returning the correct content. Previewed reports must have a filter. When
                      testing a custom report change it temporarily to a filter.
                    </p>
                    <Button
                      variant={ButtonVariant.success}
                      onClick={() => handlePreviewReport(values)}
                    >
                      Generate Preview
                    </Button>
                  </Col>
                  {values.id && (
                    <Col flex="1">
                      <p>After you save the report, send a test email to the following address.</p>
                      <Text
                        name="to"
                        label="Email To"
                        value={sendTo}
                        onChange={(e) => setSendTo(e.target.value)}
                      >
                        <Button
                          variant={ButtonVariant.secondary}
                          disabled={!sendTo}
                          onClick={async () => await handleSend(values, sendTo)}
                        >
                          Send
                        </Button>
                      </Text>
                    </Col>
                  )}
                </Row>
                <Col className="preview-report">
                  <div
                    className="preview-subject"
                    dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
                  ></div>
                  <div
                    className="preview-body"
                    dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
                  ></div>
                </Col>
              </Show>
              <Show visible={active === 'subscribers'}>
                <h2>{values.name}</h2>
                <ReportFilter
                  onSearch={async (value: string) => {
                    await findUsers({ page: 1, quantity: users.quantity, keyword: value });
                  }}
                />
                <FlexboxTable
                  rowId="id"
                  columns={subscriberColumns(values, setFieldValue)}
                  data={users.items}
                  manualPaging
                  pageIndex={users.page}
                  pageSize={users.quantity}
                  pageCount={Math.ceil(users.total / users.quantity)}
                  onPageChange={handlePageChange}
                  onSortChange={handleSortChange}
                  showSort
                />
              </Show>
              <Show visible={active === 'sent'}>
                <h2>{values.name}</h2>
                <FlexboxTable rowId="id" data={values.instances} columns={instanceColumns()} />
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
