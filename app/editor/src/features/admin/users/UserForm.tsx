import { IconButton, OptionItem } from 'components/form';
import {
  FormikCheckbox,
  FormikForm,
  FormikSelect,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { Modal } from 'components/modal';
import { useModal, useTooltips } from 'hooks';
import { IUserModel, UserStatusName } from 'hooks/api-editor';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import { Button, ButtonVariant, Show } from 'tno-core';
import { Col, Row } from 'tno-core';
import { getEnumStringOptions } from 'utils';

import { defaultUser } from './constants';
import * as styled from './styled';
import { isAdmin } from './utils/isAdmin';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserForm: React.FC = () => {
  const [, api] = useUsers();
  const { id } = useParams();
  const navigate = useNavigate();
  const userId = Number(id);
  const { toggle, isShowing } = useModal();
  const [lookups] = useLookup();
  useTooltips();

  const [user, setUser] = React.useState<IUserModel>(defaultUser);
  const [roleOptions, setRoleOptions] = React.useState(
    lookups.roles.map((r) => new OptionItem(r.name, r.id)),
  );

  const isAdminUser = isAdmin(user);
  const statusOptions = getEnumStringOptions(UserStatusName);

  React.useEffect(() => {
    if (!!userId && user?.id !== userId) {
      api.getUser(userId).then((data) => {
        setUser(data);
      });
    }
  }, [api, user?.id, userId]);

  React.useEffect(() => {
    setRoleOptions(lookups.roles.map((r) => new OptionItem(r.name, r.id)));
  }, [lookups.roles]);

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
                <FormikText
                  name="username"
                  label="Username"
                  disabled={!isAdminUser}
                  required={!values.id}
                />
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
            <FormikText
              name="email"
              label="Email"
              type="email"
              disabled={!isAdminUser}
              required={!values.id}
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
                <FormikText name="firstName" label="First Name" disabled={!isAdminUser} />
                <FormikText name="lastName" label="Last Name" disabled={!isAdminUser} />
              </Col>
            </Row>
            {!!user.id && (
              <FormikText name="key" label="Key" tooltip="Keycloak UID reference" disabled />
            )}
            <FormikTextArea name="note" label="Note" />
            <div className="roles">
              <Row className="form-inputs">
                <Col flex="1 1 auto">
                  <FormikSelect
                    label="Roles"
                    name="role"
                    options={roleOptions}
                    placeholder="Select Role"
                    tooltip="Add a role to the user"
                  >
                    <Button
                      variant={ButtonVariant.secondary}
                      disabled={!(values as any).role}
                      onClick={(e) => {
                        const id = (values as any).role;
                        const role = lookups.roles.find((r) => r.id === id);
                        if (role && !values.roles?.some((r) => r === id))
                          setUser({ ...values, roles: [...(values.roles ?? []), role.id] });
                      }}
                    >
                      Add
                    </Button>
                  </FormikSelect>
                </Col>
              </Row>
              <hr />
              {user.roles?.map((role) => (
                <Row alignContent="stretch" key={role}>
                  <Col flex="1 1 auto">{role}</Col>
                  <Col>
                    <Button
                      variant={ButtonVariant.danger}
                      onClick={(e) => {
                        setUser({
                          ...values,
                          roles: values.roles?.filter((ur) => ur !== role) ?? [],
                        });
                        setFieldValue('role', 0);
                      }}
                    >
                      <FaTrash />
                    </Button>
                  </Col>
                </Row>
              ))}
            </div>
            <Row justifyContent="center" className="form-inputs">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                Delete
              </Button>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this user?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteUser(user);
                  toast.success(`${user.username} has successfully been deleted.`);
                  navigate('/admin/users');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.UserForm>
  );
};
