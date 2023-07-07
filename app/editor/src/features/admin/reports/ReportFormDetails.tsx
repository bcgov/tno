import { useFormikContext } from 'formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IOptionItem,
  IReportModel,
  ReportTypeName,
  Row,
  Show,
} from 'tno-core';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormDetails: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  return (
    <>
      <Col className="form-inputs">
        <FormikText
          name="name"
          label="Name"
          onChange={(e) => {
            setFieldValue('name', e.target.value);
            if (values.templateId === 0)
              setFieldValue('template.name', `${e.target.value}-${Date.now().toString()}`);
          }}
        />
        <FormikTextArea name="description" label="Description" />
        <p>
          A filtered report will make a request for content each time it runs. A custom report is
          populated manually be the user.
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
              A public report is available for all users. If they subscribe to the report they will
              receive a copy every time it is run.
            </p>
            <FormikCheckbox label="Is Public" name="isPublic" />
            <Show visible={!!values.id}>
              <Row>
                <FormikText width={FieldSize.Small} disabled name="updatedBy" label="Updated By" />
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
                <FormikText width={FieldSize.Small} disabled name="createdBy" label="Created By" />
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
    </>
  );
};
