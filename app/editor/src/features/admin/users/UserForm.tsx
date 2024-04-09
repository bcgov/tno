import { FormikForm } from 'components/formik';
import { InputOption } from 'features/content/list-view/components/tool-bar/filter';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup, useLookupOptions } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IconButton,
  IUserModel,
  Modal,
  OptionItem,
  Row,
  Section,
  Show,
  useModal,
  UserStatusName,
} from 'tno-core';

import { defaultUser } from './constants';
import * as styled from './styled';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
const UserForm: React.FC = () => {
  const [, api] = useUsers();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();
  const [{ roles }] = useLookup();
  const [{ mediaTypeOptions, sourceOptions }] = useLookupOptions();

  const [user, setUser] = React.useState<IUserModel>(defaultUser);
  const [roleOptions, setRoleOptions] = React.useState(
    roles.map((r) => new OptionItem(r.name, r.id, !r.isEnabled)),
  );

  const userId = Number(id);
  const statusOptions = getEnumStringOptions(UserStatusName);

  React.useEffect(() => {
    if (!!userId && user?.id !== userId) {
      setUser({ ...defaultUser, id: userId }); // Do this to stop double fetch.
      api.getUser(userId).then((data) => {
        setUser(data);
      });
    }
  }, [api, user?.id, userId]);

  React.useEffect(() => {
    setRoleOptions(roles.map((r) => new OptionItem(r.name, r.id)));
  }, [roles]);

  const handleSubmit = async (values: IUserModel) => {
    try {
      const originalId = values.id;
      const result = !user.id ? await api.addUser(values) : await api.updateUser(values);
      setUser(result);
      toast.success(`${result.username} has successfully been saved.`);
      if (!originalId) navigate(`/admin/users/${result.id}`);
    } catch {}
  };

  return (
    <styled.UserForm>
      <IconButton
        iconType="back"
        label="Back to Users"
        className="back-button"
        onClick={() => navigate('/admin/users')}
      />
      <FormikForm
        initialValues={user}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, isSubmitting, setFieldValue }) => (
          <div className="form-container">
            <Row>
              <Col className="form-inputs">
                <FormikText name="username" label="Username" required={!values.id} />
              </Col>
              <Col
                className="form-inputs"
                onClick={(e) => {
                  if (e.ctrlKey) setUser({ ...user, status: UserStatusName.Requested });
                }}
              >
                <Show visible={user.status === UserStatusName.Requested}>
                  <FormikSelect name="status" label="Status" options={statusOptions} />
                </Show>
                <Show visible={user.status !== UserStatusName.Requested}>
                  <FormikText name="status" label="Status" disabled />
                </Show>
              </Col>
            </Row>
            <FormikText name="email" label="Email" type="email" required={!values.id} />
            <FormikText name="preferredEmail" label="Preferred Email" type="email" />
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
            <Row>
              <FormikText
                name="uniqueLogins"
                label="Number of allowed devices"
                type="number"
                tooltip="Zero means there is no limit"
                width="8ch"
              />
            </Row>
            {!!user.id && (
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
            <Row justifyContent="center" className="form-inputs">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Show visible={!!values.id}>
                <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this user?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                toggle();
                await api.deleteUser(user);
                toast.success(`${user.username} has successfully been deleted.`);
                navigate('/admin/users');
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.UserForm>
  );
};

export default UserForm;
