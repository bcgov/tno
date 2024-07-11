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
  IFilterModel,
  OptionItem,
  Row,
  Show,
  UserAccountTypeName,
} from 'tno-core';

/**
 * The page used to view and edit filters.
 * @returns Component.
 */
export const FilterFormDetails: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFilterModel>();
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
    try {
      const results = await findUsers(
        {
          quantity: 50,
          username: text,
          includeUserId: values.ownerId,
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
    } catch {}
  }, 500);

  return (
    <Col className="form-inputs">
      <FormikText name="name" label="Name" required />
      <FormikTextArea name="description" label="Description" />
      <FormikSelect
        name="ownerId"
        label="Owner"
        options={userOptions}
        value={userOptions.find((u) => u.value === values.ownerId) || ''}
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
      <Row alignItems="center">
        <FormikText
          width={FieldSize.Tiny}
          name="sortOrder"
          label="Sort Order"
          type="number"
          className="sort-order"
        />
        <FormikCheckbox label="Is Enabled" name="isEnabled" />
      </Row>
      <Row>
        <Col flex="2">
          <Show visible={!!values.id}>
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
      </Row>
    </Col>
  );
};
