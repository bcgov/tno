import { useFormikContext } from 'formik';
import { debounce, noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useApp } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getUserOptions,
  INotificationModel,
  OptionItem,
  Row,
  Show,
  UserAccountTypeName,
} from 'tno-core';

import { resendOptions } from './constants';

export const NotificationFormDetails: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<INotificationModel>();
  const [{ userInfo }] = useApp();
  const [{ users }, { findUsers }] = useUsers();

  const [userOptions, setUserOptions] = React.useState(getUserOptions(users.items));

  React.useEffect(() => {
    if (userInfo?.id) {
      findUsers({
        quantity: 50,
        includeUserId: values.ownerId,
        accountTypes: [
          UserAccountTypeName.Direct,
          UserAccountTypeName.Indirect,
          UserAccountTypeName.SystemAccount,
        ],
      })
        .then((results) => {
          setUserOptions(getUserOptions(results.items));
        })
        .catch(() => {});
    }
    // Only fire on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo?.id]);

  const handleFindUsers = debounce(async (text: string) => {
    const results = await findUsers(
      {
        quantity: 50,
        keyword: text,
        accountTypes: [
          UserAccountTypeName.Direct,
          UserAccountTypeName.Indirect,
          UserAccountTypeName.SystemAccount,
        ],
      },
      true,
    );
    setUserOptions(getUserOptions(results.items));
    return results;
  }, 500);

  return (
    <Col className="form-inputs">
      <FormikText name="name" label="Name" />
      <FormikTextArea name="description" label="Description" />
      <Col alignContent="stretch">
        <Row gap="1rem">
          <Col flex="1">
            <FormikCheckbox label="Is Enabled" name="isEnabled" />
            <FormikCheckbox label="Is Public" name="isPublic" />
            <FormikCheckbox
              label="Run notification when content is indexed"
              name="alertOnIndex"
              tooltip="Every time content is indexed this notification will run to determine if it should send out an alert."
            />
            <Show visible={values.alertOnIndex}>
              <FormikSelect
                name="resend"
                label="Resend Option"
                options={resendOptions}
                required
                isClearable={false}
              />
            </Show>
          </Col>
          <Col flex="1">
            <FormikSelect
              name="ownerId"
              label="Owner"
              options={userOptions}
              value={userOptions.find((u) => u.value === values.ownerId) || ''}
              width="50ch"
              onChange={(e) => {
                const option = e as OptionItem;
                setFieldValue(
                  'values.ownerId',
                  option?.value ? parseInt(option.value.toString()) : undefined,
                );
              }}
              onInputChange={(newValue) => {
                // TODO: Does not need to fire when an option is manually selected.
                handleFindUsers(newValue);
              }}
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
