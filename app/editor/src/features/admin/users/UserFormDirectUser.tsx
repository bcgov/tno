import { InputOption } from 'features/content/list-view/components/tool-bar/filter';
import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup, useLookupOptions } from 'store/hooks';
import {
  Col,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IOptionItem,
  IUserModel,
  IUserUpdateHistoryModel,
  OptionItem,
  Row,
  Section,
  Show,
  UserAccountTypeName,
  UserChangeTypeName,
  UserStatusName,
} from 'tno-core';

import { IUserFormProps } from './UserForm';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormDirectUser: React.FC<IUserFormProps> = (props) => {
  const [{ roles }] = useLookup();
  const [{ mediaTypeOptions, sourceOptions }] = useLookupOptions();
  const { values, setFieldValue } = useFormikContext<IUserModel>();
  const [{ organizations, organizationOptions }] = useLookupOptions();

  const [roleOptions, setRoleOptions] = React.useState(
    roles.map((r) => new OptionItem(r.name, r.id, !r.isEnabled)),
  );

  const statusOptions = getEnumStringOptions(UserStatusName);
  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName).filter(
    (o) => o.value !== UserAccountTypeName.SystemAccount,
  );

  const handleFieldUpdateHistory = React.useCallback(
    (options: IOptionItem, changeType: UserChangeTypeName) => {
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
    },
    [props, setFieldValue, values.id, values.userUpdateHistory],
  );

  React.useEffect(() => {
    setRoleOptions(roles.map((r) => new OptionItem(r.name, r.id)));
  }, [roles]);

  return (
    <div className="form-container">
      <Col>
        <Row className="no-border">
          <Col flex="2">{props.banner}</Col>
        </Row>
        <Row>
          <Section className="frm-in">
            <label>Account Information</label>
            <Row>
              <Col className="form-inputs" flex="1">
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
              </Col>
            </Row>
            <Row>
              <Col className="form-inputs" flex="1">
                <FormikText
                  name="username"
                  label="Username"
                  required
                  onChange={(e) => setFieldValue('username', e.currentTarget.value.toUpperCase())}
                />
              </Col>
              <Col
                className="form-inputs"
                flex="1"
                onClick={(e) => {
                  if (e.ctrlKey) setFieldValue('status', UserStatusName.Requested);
                }}
              >
                <Show visible={values.status === UserStatusName.Requested}>
                  <FormikSelect name="status" label="Status" options={statusOptions} />
                </Show>
                <Show visible={values.status !== UserStatusName.Requested}>
                  <FormikText name="status" label="Status" disabled />
                </Show>
              </Col>
            </Row>
            <Row>
              <Col flex="1">
                <FormikText name="email" label="Email" type="email" required />
              </Col>
              <Col flex="1">
                <FormikText name="preferredEmail" label="Preferred Email" type="email" />
              </Col>
            </Row>
            <Row>
              <Col className="form-inputs" flex="1">
                <FormikText
                  name="displayName"
                  label="Display Name"
                  tooltip="Friendly name to use instead of username"
                />
              </Col>
            </Row>
            <Row>
              <Col className="form-inputs" flex="1">
                <FormikText name="firstName" label="First Name" />
              </Col>
              <Col className="form-inputs" flex="1">
                <FormikText name="lastName" label="Last Name" />
              </Col>
            </Row>
            <Row>
              {!!values.id && (
                <Col flex="1">
                  <FormikText name="key" label="Key" tooltip="Keycloak UID reference" disabled />
                </Col>
              )}
            </Row>
            <FormikTextArea name="note" label="Note" />
          </Section>
          <Section className="frm-in no-border frm-in-right-most">
            <Section className="frm-in frm-in-right-most">
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
                isMulti
                name="reports"
                options={props.reportOptions}
                value={
                  values.reports?.map((ct) => props.reportOptions.find((o) => o.value === ct.id)) ??
                  []
                }
                onChange={(newValue) => {
                  const options = newValue as IOptionItem[];
                  setFieldValue(
                    'reports',
                    options
                      ? props.reports.filter((report) => options.some((o) => o.value === report.id))
                      : [],
                  );
                }}
              />
            </Section>
            <Section className="frm-in frm-in-right-most">
              <label>Permissions</label>
              <FormikSelect
                label="Roles"
                name="roles"
                options={roleOptions}
                placeholder="Select roles"
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
              <p>Assign roles to the user to grant access to application.</p>
              <FormikText
                name="uniqueLogins"
                label="# of devices allowed"
                type="number"
                tooltip="Zero means there is no limit"
              />
              <p>Limit the number of devices an account can sign in with.</p>
              <FormikSelect
                label="Block access to sources"
                name="source"
                options={sourceOptions}
                placeholder="Select sources"
                tooltip="Block this user from access to the specified sources"
                isMulti
                value={sourceOptions.filter(
                  (o) => values.sources?.some((r) => (r === o.value ? +o.value : undefined)) ?? '',
                )}
                onChange={(e) => {
                  if (e) {
                    const options = e as OptionItem[];
                    setFieldValue(
                      'sources',
                      options.filter((v) => v.value).map((v) => v.value),
                    );
                  }
                }}
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                components={{
                  Option: InputOption,
                }}
              />
              <FormikSelect
                label="Block access to media types"
                name="mediaTypes"
                options={mediaTypeOptions}
                placeholder="Select media types"
                tooltip="Block this user access to the specified media types"
                isMulti
                value={mediaTypeOptions.filter(
                  (o) =>
                    values.mediaTypes?.some((r) => (r === o.value ? +o.value : undefined)) ?? '',
                )}
                onChange={(e) => {
                  if (e) {
                    const options = e as OptionItem[];
                    setFieldValue(
                      'mediaTypes',
                      options.filter((v) => v.value).map((v) => v.value),
                    );
                  }
                }}
                closeMenuOnSelect={false}
                hideSelectedOptions={false}
                components={{
                  Option: InputOption,
                }}
              />
            </Section>
          </Section>
        </Row>
      </Col>
    </div>
  );
};
