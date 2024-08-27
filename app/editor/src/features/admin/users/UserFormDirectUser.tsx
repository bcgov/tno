import { InputOption } from 'features/content/list-view/components/tool-bar/filter';
import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup, useLookupOptions } from 'store/hooks';
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
  Section,
  Show,
  UserAccountTypeName,
  UserStatusName,
} from 'tno-core';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormDirectUser: React.FC = () => {
  const [{ roles }] = useLookup();
  const [{ mediaTypeOptions, sourceOptions }] = useLookupOptions();
  const { values, setFieldValue } = useFormikContext<IUserModel>();

  const [roleOptions, setRoleOptions] = React.useState(
    roles.map((r) => new OptionItem(r.name, r.id, !r.isEnabled)),
  );

  const statusOptions = getEnumStringOptions(UserStatusName);
  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName).filter(
    (o) => o.value !== UserAccountTypeName.SystemAccount,
  );

  React.useEffect(() => {
    setRoleOptions(roles.map((r) => new OptionItem(r.name, r.id)));
  }, [roles]);

  return (
    <div className="form-container">
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
              />
            </Col>
            <Col justifyContent="center">
              <FormikCheckbox label="Is Enabled" name="isEnabled" />
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
            <Col justifyContent="center">
              <FormikCheckbox label="Email Verified" name="emailVerified" />
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
        <Section className="frm-in">
          <label>Authorization</label>
          <p>Assign roles to the user to grant access to application.</p>
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
          <p>Limit the number of devices an account can sign in with.</p>
          <FormikText
            name="uniqueLogins"
            label="# of devices allowed"
            type="number"
            tooltip="Zero means there is no limit"
          />
          <Section>
            <p>Select the sources and media types this user should not have access to.</p>
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
                (o) => values.mediaTypes?.some((r) => (r === o.value ? +o.value : undefined)) ?? '',
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
    </div>
  );
};
