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
  INotificationModel,
  NotificationTypeName,
  ResendOptionName,
  Row,
  Show,
} from 'tno-core';

export const NotificationFormDetails: React.FC = () => {
  const { values } = useFormikContext<INotificationModel>();

  const NotificationTypeOptions = getEnumStringOptions(NotificationTypeName);
  const resendOptions = getEnumStringOptions(ResendOptionName);

  return (
    <Col className="form-inputs">
      <FormikText name="name" label="Name" />
      <FormikTextArea name="description" label="Description" />
      <Col alignContent="stretch">
        <Row gap="1rem">
          <Col flex="1">
            <FormikCheckbox
              label="Run notification when content is indexed"
              name="alertOnIndex"
              tooltip="Every time content is indexed this notification will run to determine if it should send out an alert."
            />
            <FormikCheckbox label="Is Enabled" name="isEnabled" />
            <FormikCheckbox label="Is Public" name="isPublic" />
          </Col>
          <Col flex="1">
            <FormikSelect
              name="notificationType"
              label="Notification Type"
              options={NotificationTypeOptions}
              required
              isClearable={false}
            />
            <FormikSelect
              name="resend"
              label="Resend Option"
              options={resendOptions}
              required
              isClearable={false}
            />
            <FormikText
              width={FieldSize.Tiny}
              name="sortOrder"
              label="Sort Order"
              type="number"
              className="sort-order"
            />
          </Col>
        </Row>
        <Show visible={!!values.id}>
          <Row justifyContent="center">
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
          <Row justifyContent="center">
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
    </Col>
  );
};
