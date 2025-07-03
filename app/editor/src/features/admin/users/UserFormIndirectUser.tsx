import { useFormikContext } from 'formik';
import React from 'react';
import { useLookupOptions } from 'store/hooks';
import { useReports } from 'store/hooks/admin';
import {
  Col,
  FieldSize,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  getSortableOptions,
  IOptionItem,
  IReportModel,
  IUserModel,
  IUserUpdateHistoryModel,
  Row,
  Section,
  UserAccountTypeName,
  UserChangeTypeName,
} from 'tno-core';

import { IUserFormProps } from './UserForm';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormIndirectUser: React.FC<IUserFormProps> = (props) => {
  const { values, setFieldValue } = useFormikContext<IUserModel>();
  const [{ organizations, organizationOptions }] = useLookupOptions();
  const [, { findReports }] = useReports();
  const [reports, setReports] = React.useState<IReportModel[]>([]);
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);

  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName).filter(
    (o) => o.value !== UserAccountTypeName.SystemAccount,
  );

  React.useEffect(() => {
    if (values.id) {
      try {
        findReports({})
          .then((data) => {
            setReportOptions(getSortableOptions(data));
            setReports(data);
          })
          .catch(() => {});
      } catch {}
    }
  }, []);

  const handleFieldUpdateHistory = (options: IOptionItem, changeType: UserChangeTypeName) => {
    const changedValue = options?.value ? options.value : '';
    const change: IUserUpdateHistoryModel = {
      userId: values.id,
      value: changedValue.toString(),
      ChangeType: changeType,
      id: 0,
      dateOfChange: new Date(),
    };
    setFieldValue('userUpdateHistory', [...(values.userUpdateHistory ?? []), change]);
    if (props.onUserChange) {
      props.onUserChange(changeType);
    }
  };

  return (
    <div className="form-container">
      <Row>
        <Section className="frm-in">
          <label>Account Information</label>
          <FormikSelect
            name="accountType"
            label="Account Type"
            options={accountTypeOptions}
            value={accountTypeOptions.find((s) => s.value === values.accountType) || ''}
            required
            isClearable={false}
            onChange={(newValue) => {
              const options = newValue as IOptionItem;
              handleFieldUpdateHistory(options, UserChangeTypeName.AccountType);
            }}
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
            onChange={(e) => setFieldValue('email', e.currentTarget.value)}
          />
          <FormikText
            name="displayName"
            label="Display Name"
            tooltip="Friendly name to use instead of username"
          />
          <Row>
            <Col className="form-inputs" flex="1">
              <FormikText name="firstName" label="First Name" />
            </Col>
            <Col className="form-inputs" flex="1">
              <FormikText name="lastName" label="Last Name" />
            </Col>
          </Row>
          <FormikTextArea name="note" label="Note" />
        </Section>
        <Section className="frm-in">
          <FormikSelect
            name="organizations"
            value={
              values.organizations?.map((ct) =>
                organizationOptions.find((o) => o.value === ct.id),
              ) ?? []
            }
            label="Ministry or organization"
            options={organizationOptions}
            onChange={(newValue) => {
              const options = newValue as IOptionItem;
              setFieldValue(
                'organizations',
                options ? organizations.filter((org) => options.value === org.id) : [],
              );
              handleFieldUpdateHistory(options, UserChangeTypeName.Organization);
            }}
          />
          <FormikSelect
            label="Reports"
            width={FieldSize.Big}
            isMulti
            name="reports"
            options={reportOptions}
            value={values.reports?.map((ct) => reportOptions.find((o) => o.value === ct.id)) ?? []}
            onChange={(newValue) => {
              const options = newValue as IOptionItem[];
              setFieldValue(
                'reports',
                options
                  ? reports.filter((report) => options.some((o) => o.value === report.id))
                  : [],
              );
            }}
          />
        </Section>
      </Row>
    </div>
  );
};
