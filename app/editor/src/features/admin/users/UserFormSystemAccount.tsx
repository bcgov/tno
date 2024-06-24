import { InputOption } from 'features/content/list-view/components/tool-bar/filter';
import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IUserModel,
  OptionItem,
  Row,
  UserAccountTypeName,
} from 'tno-core';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormSystemAccount: React.FC = () => {
  const [{ roles }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IUserModel>();

  const [roleOptions, setRoleOptions] = React.useState(
    roles.map((r) => new OptionItem(r.name, r.id, !r.isEnabled)),
  );

  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName);

  React.useEffect(() => {
    setRoleOptions(roles.map((r) => new OptionItem(r.name, r.id)));
  }, [roles]);

  return (
    <div className="form-container">
      <FormikSelect
        name="accountType"
        label="Account Type"
        options={accountTypeOptions}
        value={accountTypeOptions.find((s) => s.value === values.accountType) || ''}
        required
        isClearable={false}
      />
      <FormikText
        name="username"
        label="Username"
        required
        onChange={(e) => setFieldValue('username', e.currentTarget.value.toUpperCase())}
      />
      <FormikText name="email" label="Email" type="email" required />
      <Row>
        <Col className="form-inputs">
          <FormikText
            name="displayName"
            label="Display Name"
            tooltip="Friendly name to use instead of username"
          />
          <FormikCheckbox label="Email Verified" name="emailVerified" />
          <FormikCheckbox label="Is Enabled" name="isEnabled" />
        </Col>
        <Col className="form-inputs">
          <FormikText name="firstName" label="First Name" />
          <FormikText name="lastName" label="Last Name" />
        </Col>
      </Row>
      {!!values.id && (
        <FormikText name="key" label="Key" tooltip="Keycloak UID reference" disabled />
      )}
      <FormikTextArea name="note" label="Note" />
      <FormikSelect
        label="Roles"
        name="roles"
        options={roleOptions}
        placeholder="Select roles"
        tooltip="Roles provide a way to grant this user permission to features in the application"
        isMulti
        value={roleOptions.filter((o) => values.roles?.some((r) => r === o.value)) ?? ''}
        onChange={(e) => {
          if (e) {
            const values = e as OptionItem[];
            setFieldValue(
              'roles',
              values.map((v) => v.value),
            );
          }
        }}
        closeMenuOnSelect={false}
        hideSelectedOptions={false}
        components={{
          Option: InputOption,
        }}
      />
    </div>
  );
};
