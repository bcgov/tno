import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  IconButton,
  IUserModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
  UserAccountTypeName,
} from 'tno-core';

import { defaultUser } from './constants';
import * as styled from './styled';
import { TransferAccount } from './TransferAccount';
import { UserFormDirectUser } from './UserFormDirectUser';
import { UserFormDistribution } from './UserFormDistribution';
import { UserFormIndirectUser } from './UserFormIndirectUser';
import { UserFormSystemAccount } from './UserFormSystemAccount';
import { UserNotificationSubscriptions } from './UserNotificationSubscriptions';
import { UserProductSubscriptions } from './UserProductSubscriptions';
import { UserReportSubscriptions } from './UserReportSubscriptions';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
const UserForm: React.FC = () => {
  const [, api] = useUsers();
  const { id, type, tab } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const [active, setActive] = React.useState('details');

  const initUser = React.useCallback((type?: string) => {
    const accountType =
      type === 'direct'
        ? UserAccountTypeName.Direct
        : type === 'indirect'
        ? UserAccountTypeName.Indirect
        : type === 'distribution'
        ? UserAccountTypeName.Distribution
        : UserAccountTypeName.Direct;
    return { ...defaultUser, accountType };
  }, []);

  const [user, setUser] = React.useState<IUserModel>(initUser(type));
  const userId = Number(id);

  React.useEffect(() => {
    if (!!userId && user?.id !== userId) {
      setUser({ ...initUser(type), id: userId }); // Do this to stop double fetch.
      api.getUser(userId).then((data) => {
        setUser(data);
      });
    }
  }, [api, initUser, type, user?.id, userId]);

  React.useEffect(() => {
    setActive(tab ?? 'details');
  }, [tab]);

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
        {({ values, isSubmitting }) => (
          <Col>
            <Tabs
              tabs={
                <>
                  <Tab
                    label="Details"
                    onClick={() => {
                      navigate(`/admin/users/${values.id}`);
                    }}
                    active={active === 'details'}
                  />
                  <Show visible={!!values.id}>
                    <Tab
                      label="Products"
                      onClick={() => {
                        navigate(
                          `/admin/users/${values.id}/${values.accountType.toLowerCase()}/products`,
                        );
                      }}
                      active={active === 'products'}
                    />
                    <Tab
                      label="Reports"
                      onClick={() => {
                        navigate(
                          `/admin/users/${values.id}/${values.accountType.toLowerCase()}/reports`,
                        );
                      }}
                      active={active === 'reports'}
                    />
                    <Tab
                      label="Notifications"
                      onClick={() => {
                        navigate(
                          `/admin/users/${
                            values.id
                          }/${values.accountType.toLowerCase()}/notifications`,
                        );
                      }}
                      active={active === 'notifications'}
                    />
                    <Tab
                      label="Account Mirroring"
                      onClick={() => {
                        navigate(
                          `/admin/users/${values.id}/${values.accountType.toLowerCase()}/transfer`,
                        );
                      }}
                      active={active === 'transfer'}
                    />
                  </Show>
                </>
              }
            >
              <Show visible={active === 'details'}>
                <Show visible={values.accountType === UserAccountTypeName.Direct}>
                  <UserFormDirectUser />
                </Show>
                <Show visible={values.accountType === UserAccountTypeName.Indirect}>
                  <UserFormIndirectUser />
                </Show>
                <Show visible={values.accountType === UserAccountTypeName.Distribution}>
                  <UserFormDistribution />
                </Show>
                <Show visible={values.accountType === UserAccountTypeName.SystemAccount}>
                  <UserFormSystemAccount />
                </Show>
              </Show>
              <Show visible={!!values.id}>
                <Show visible={active === 'products'}>
                  <UserProductSubscriptions />
                </Show>
                <Show visible={active === 'reports'}>
                  <UserReportSubscriptions />
                </Show>
                <Show visible={active === 'notifications'}>
                  <UserNotificationSubscriptions />
                </Show>
                <Show visible={active === 'transfer'}>
                  <TransferAccount />
                </Show>
              </Show>
            </Tabs>
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
          </Col>
        )}
      </FormikForm>
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
    </styled.UserForm>
  );
};

export default UserForm;
