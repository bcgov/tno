import { useFormikContext } from 'formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  IReportTemplateModel,
  Row,
  Show,
} from 'tno-core';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateFormDetails: React.FC = () => {
  const { values } = useFormikContext<IReportTemplateModel>();

  return (
    <>
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
            <FormikText width={FieldSize.Small} disabled name="updatedBy" label="Updated By" />
            <FormikDatePicker
              selectedDate={!!values.updatedOn ? moment(values.updatedOn).toString() : undefined}
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
              selectedDate={!!values.createdOn ? moment(values.createdOn).toString() : undefined}
              onChange={noop}
              name="createdOn"
              label="Created On"
              disabled
              width={FieldSize.Small}
            />
          </Row>
        </Show>
      </Col>
    </>
  );
};
