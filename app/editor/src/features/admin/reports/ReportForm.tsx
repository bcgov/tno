import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IconButton,
  IOptionItem,
  IReportModel,
  LabelPosition,
  Modal,
  ReportTypeName,
  Row,
  Show,
  Text,
  TextArea,
  useModal,
} from 'tno-core';

import { defaultReport } from './constants';
import * as styled from './styled';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [, api] = useReports();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [report, setReport] = React.useState<IReportModel>((state as any)?.report ?? defaultReport);
  const [filter, setFilter] = React.useState(JSON.stringify(report.filter));
  const [settings, setSettings] = React.useState(JSON.stringify(report.settings));
  const [sendTo, setSendTo] = React.useState('');

  const reportId = Number(id);
  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  React.useEffect(() => {
    if (!!reportId && report?.id !== reportId) {
      setReport({ ...defaultReport, id: reportId }); // Do this to stop double fetch.
      api.getReport(reportId).then((data) => {
        setReport(data);
        setFilter(JSON.stringify(data.filter));
        setSettings(JSON.stringify(data.settings));
      });
    }
  }, [api, report?.id, reportId]);

  const handleSubmit = async (values: IReportModel) => {
    try {
      const originalId = values.id;
      const result = !report.id ? await api.addReport(values) : await api.updateReport(values);
      setReport(result);
      setFilter(JSON.stringify(result.filter));
      setSettings(JSON.stringify(result.settings));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/reports/${result.id}`);
    } catch {}
  };

  const handleSend = async (values: IReportModel, to: string) => {
    try {
      console.debug(values, to);
      await api.sendReport(values, to);
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
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <TextArea
                name="settings"
                label="Settings"
                tooltip="Configuration settings"
                value={settings}
                onChange={(e) => {
                  setSettings(e.target.value);
                  setFieldValue('settings', JSON.parse(e.target.value));
                }}
              />
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
                <TextArea
                  name="filter"
                  label="Filter"
                  tooltip="Elasticsearch query statement"
                  value={filter}
                  onChange={(e) => {
                    setFilter(e.target.value);
                    setFieldValue('filter', JSON.parse(e.target.value));
                  }}
                />
              </Show>
              <FormikTextArea name="template" label="Template" tooltip="Razor syntax template" />
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
              </Row>
            </Col>
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
                  navigate('/admin/programs');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.ReportForm>
  );
};
