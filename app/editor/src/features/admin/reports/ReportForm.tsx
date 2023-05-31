import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useReports, useUsers } from 'store/hooks/admin';
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
  IconButton,
  IOptionItem,
  IReportModel,
  ITableInternal,
  ITablePage,
  ITableSort,
  IUserModel,
  LabelPosition,
  Modal,
  ReportTypeName,
  Row,
  Show,
  Tab,
  Tabs,
  Text,
  useModal,
} from 'tno-core';

import { defaultReport, subscriberColumns } from './constants';
import { ReportFilter } from './ReportFilter';
import * as styled from './styled';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, api] = useReports();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ users }, { findUsers }] = useUsers();

  const [report, setReport] = React.useState<IReportModel>(
    (state as any)?.report ?? { ...defaultReport, ownerId: userInfo?.id ?? 0 },
  );
  const [filter, setFilter] = React.useState(JSON.stringify(report.filter, null, 2));
  const [sendTo, setSendTo] = React.useState('');
  const [active, setActive] = React.useState('report');

  const reportId = Number(id);
  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  React.useEffect(() => {
    findUsers({}).catch(() => {
      // Handled already.
    });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (!!reportId && report?.id !== reportId) {
      setReport({ ...defaultReport, id: reportId }); // Do this to stop double fetch.
      api.getReport(reportId).then((data) => {
        setReport(data);
        setFilter(JSON.stringify(data.filter, null, 2));
      });
    }
  }, [api, report?.id, reportId]);

  const handleSubmit = async (values: IReportModel) => {
    try {
      const originalId = values.id;
      const result = !report.id ? await api.addReport(values) : await api.updateReport(values);
      setReport(result);
      setFilter(JSON.stringify(result.filter, null, 2));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/reports/${result.id}`);
    } catch {}
  };

  const handleSend = async (values: IReportModel, to: string) => {
    try {
      await api.sendReport(values, to);
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
                ></Tab>
                <Tab
                  label="Subscribers"
                  onClick={() => {
                    setActive('subscribers');
                  }}
                  active={active === 'subscribers'}
                ></Tab>
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'report'}>
                <Col className="form-inputs">
                  <FormikText name="name" label="Name" />
                  <FormikTextArea name="description" label="Description" />
                  <FormikSelect
                    name="reportType"
                    label="Report Type"
                    options={reportTypeOptions}
                    value={reportTypeOptions.filter((rt) =>
                      values.reportType.includes(rt.value as ReportTypeName),
                    )}
                    onChange={(newValue) => {
                      const option = newValue as IOptionItem;
                      setFieldValue('reportType', option.value);
                    }}
                  />
                  <Show visible={values.reportType === ReportTypeName.Filter}>
                    <Col className="code frm-in">
                      <label htmlFor="txa-filter">Elasticsearch Filter</label>
                      <Col className="editor">
                        <Editor
                          id="txa-filter"
                          value={filter}
                          onValueChange={(code) => {
                            setFilter(code);
                            setFieldValue('filter', JSON.parse(code));
                          }}
                          highlight={(code) => {
                            return highlight(code, languages.json, 'json');
                          }}
                        />
                      </Col>
                    </Col>
                  </Show>
                  <Col className="code frm-in">
                    <label htmlFor="txa-subject">Subject Template</label>
                    <Col className="editor">
                      <Editor
                        id="txa-subject"
                        value={values.settings.subject ?? ''}
                        onValueChange={(code) => setFieldValue('settings.subject', code)}
                        highlight={(code) => {
                          return highlight(code, languages.cshtml, 'razor');
                        }}
                      />
                    </Col>
                  </Col>
                  <Col className="code frm-in">
                    <label htmlFor="txa-template">Report Template</label>
                    <Col className="editor">
                      <Editor
                        id="txa-template"
                        value={values.template}
                        onValueChange={(code) => setFieldValue('template', code)}
                        highlight={(code) => {
                          return highlight(code, languages.cshtml, 'razor');
                        }}
                      />
                    </Col>
                  </Col>
                  <Row>
                    <Col>
                      <Row gap="1rem">
                        <FormikCheckbox
                          labelPosition={LabelPosition.Top}
                          label="Is Enabled"
                          name="isEnabled"
                        />
                        <FormikCheckbox
                          labelPosition={LabelPosition.Top}
                          label="Is Public"
                          name="isPublic"
                        />
                        <FormikText
                          width={FieldSize.Tiny}
                          name="sortOrder"
                          label="Sort Order"
                          type="number"
                          className="sort-order"
                        />
                      </Row>
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
                    {values.id && (
                      <Col>
                        <h2>Test Report</h2>
                        <p>You can test this report and send it to the following email address.</p>
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
                </Col>
              </Show>
              <Show visible={active === 'subscribers'}>
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
                body="Are you sure you wish to remove this report?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await api.deleteReport(report);
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
