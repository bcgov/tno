import { useFormikContext } from 'formik';
import React from 'react';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IUserModel,
  Row,
  Section,
  UserAccountTypeName,
} from 'tno-core';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormIndirectUser: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IUserModel>();

  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName).filter(
    (o) => o.value !== UserAccountTypeName.SystemAccount,
  );

  return (
    <div className="form-container">
      <Section className="frm-in">
        <label>Account Information</label>
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
        <FormikText
          name="email"
          label="Email"
          type="email"
          required
          onChange={(e) => setFieldValue('email', e.currentTarget.value.toUpperCase())}
        />
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
        <FormikTextArea name="note" label="Note" />
      </Section>
    </div>
  );
};
