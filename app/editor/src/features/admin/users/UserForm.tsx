import { FormikForm } from 'components/formik';
import { formatDate } from 'features/admin/users/utils/formatDate';
import React, { ReactElement } from 'react';
import { FaExclamationTriangle } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks/admin';
import { useReports } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  getSortableOptions,
  IconButton,
  IOptionItem,
  IReportModel,
  IUserModel,
  IUserUpdateHistoryModel,
  Modal,
  Row,
  SelectDate,
  Show,
  Tab,
  Tabs,
  useModal,
  UserAccountTypeName,
  UserChangeTypeName,
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

export interface IUserFormProps {
  onUserChange?: (changeType: UserChangeTypeName) => void;
  banner?: ReactElement<any, any>;
  reportOptions: IOptionItem<string | number | undefined>[];
  reports: IReportModel[];
}

enum ActionType {
  Save,
  Deactivate,
  Reactivate,
  Delete,
}

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
const UserForm: React.FC = () => {
  const [, api] = useUsers();
  const { id, type, tab } = useParams();
  const navigate = useNavigate();
  const { toggle: toggleDelete, isShowing: deleteShowing } = useModal();

  const { toggle: toggleAccountBillingChange, isShowing: accountBillingChangeShowing } = useModal();
  const { toggle: toggleDeactivate, isShowing: deactivateShowing } = useModal();
  const { toggle: toggleReactivate, isShowing: reactivateShowing } = useModal();
  const [dateOfChange, setDateOfChange] = React.useState(new Date());
  const [accountTypeChange, setAccountTypeChange] = React.useState(false);
  const [orgChange, setOrgChange] = React.useState(false);
  const [active, setActive] = React.useState('details');
  const [, { findReports }] = useReports();
  const [reports, setReports] = React.useState<IReportModel[]>([]);
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);

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
        try {
          findReports({})
            .then((reportsData) => {
              setReportOptions(getSortableOptions(reportsData));
              setReports(reportsData);
            })
            .catch(() => {});
        } catch {}
      });
    }
  }, [api, findReports, initUser, type, user?.id, user.isEnabled, userId]);

  React.useEffect(() => {
    setActive(tab ?? 'details');
  }, [tab]);

  const updateHistoryDates = React.useCallback(
    async (values: IUserModel) => {
      const newHistoryData = values.userUpdateHistory?.filter((x) => x.id === 0);
      newHistoryData?.map((x) => (x.dateOfChange = dateOfChange));
    },
    [dateOfChange],
  );

  const handleSave = React.useCallback(
    async (values: IUserModel) => {
      try {
        updateHistoryDates(values);
        const originalId = values.id;
        const result = !values.id ? await api.addUser(values) : await api.updateUser(values);
        setUser(result);
        setAccountTypeChange(false);
        setOrgChange(false);
        setDateOfChange(new Date());
        toast.success(`${result.username} has successfully been saved.`);
        if (!originalId) navigate(`/admin/users/${result.id}`);
      } catch {}
    },
    [api, navigate, updateHistoryDates],
  );

  const handleUserActivation = React.useCallback(
    async (values: IUserModel, changeType: UserChangeTypeName) => {
      const toUpdate = { ...values };
      toUpdate.isEnabled = !toUpdate.isEnabled;
      const change: IUserUpdateHistoryModel = {
        userId: toUpdate.id,
        value: toUpdate.isEnabled.toString(),
        ChangeType: changeType,
        id: 0,
        dateOfChange: dateOfChange,
      };
      try {
        toUpdate.userUpdateHistory = [...(values.userUpdateHistory ?? []), change];
        handleSave(toUpdate);
      } catch {}
    },
    [dateOfChange, handleSave],
  );

  const handleSubmit = React.useCallback(
    async (values: IUserModel) => {
      if (accountTypeChange || orgChange) {
        toggleAccountBillingChange();
        setUser(values);
      } else {
        handleSave(values);
      }
    },
    [accountTypeChange, handleSave, orgChange, toggleAccountBillingChange],
  );

  const getUserChangeNotes = React.useCallback(() => {
    return (
      <div>
        {orgChange ? <li>User ministry or organization has been updated.</li> : null}
        {accountTypeChange ? <li>User account type has been updated.</li> : null}
      </div>
    );
  }, [accountTypeChange, orgChange]);

  const isEnabledString = user.isEnabled ? '' : 'Account deactivated';

  const handleUserFieldChange = React.useCallback((changeType: UserChangeTypeName) => {
    switch (changeType) {
      case UserChangeTypeName.AccountType:
        setAccountTypeChange(true);
        break;
      case UserChangeTypeName.Organization:
        setOrgChange(true);
        break;
      default:
    }
  }, []);

  const getModalNotes = React.useCallback(
    (actionType: ActionType) => {
      return (
        <div className="form-container">
          <div>
            <ul>
              {(() => {
                switch (actionType) {
                  case ActionType.Save:
                    return <div>{getUserChangeNotes()}</div>;
                  case ActionType.Deactivate:
                    return (
                      <div>
                        <li>
                          Deactivated user data is maintained and the account can be reactivated.
                        </li>
                        {getUserChangeNotes()}
                      </div>
                    );
                  case ActionType.Reactivate:
                    return (
                      <div>
                        <li>
                          Reactivating an account will restore user access to content on MMI
                          websites.
                        </li>
                        {getUserChangeNotes()}
                      </div>
                    );
                  case ActionType.Delete:
                    return (
                      <>
                        <div>
                          Deleting will permanently remove all user data, including historical
                          billing records from MMI.
                        </div>
                        <p>This action cannot be reversed.</p>
                      </>
                    );
                  default:
                    return null;
                }
              })()}
            </ul>
          </div>
        </div>
      );
    },
    [getUserChangeNotes],
  );

  const HeaderBanner = React.useCallback(
    (isSubmitting: any) => {
      return (
        <Show visible={user.isEnabled.toString() === 'false'}>
          <div className="info-bar">
            <span>
              <FaExclamationTriangle className="info-bar-icon" key="warning" />
              <span className="info-bar-header">{isEnabledString}</span>
            </span>
            <Button
              className="info-bar-button"
              onClick={toggleReactivate}
              variant={ButtonVariant.secondary}
              disabled={isSubmitting.isSubmitting}
            >
              Reactivate
            </Button>
          </div>
        </Show>
      );
    },
    [isEnabledString, toggleReactivate, user.isEnabled],
  );

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
          <>
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
                            `/admin/users/${
                              values.id
                            }/${values.accountType.toLowerCase()}/products`,
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
                            `/admin/users/${
                              values.id
                            }/${values.accountType.toLowerCase()}/transfer`,
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
                    <UserFormDirectUser
                      onUserChange={handleUserFieldChange}
                      banner={<HeaderBanner isSubmitting={isSubmitting} />}
                      reports={reports}
                      reportOptions={reportOptions}
                    />
                  </Show>
                  <Show visible={values.accountType === UserAccountTypeName.Indirect}>
                    <UserFormIndirectUser
                      onUserChange={handleUserFieldChange}
                      banner={<HeaderBanner isSubmitting={isSubmitting} />}
                      reports={reports}
                      reportOptions={reportOptions}
                    />
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
                <Button type="submit" disabled={isSubmitting} className="button-actions">
                  Save
                </Button>
                <Show
                  visible={
                    values.isEnabled.toString() === 'false' &&
                    values.id > 0 &&
                    values.accountType !== UserAccountTypeName.Distribution
                  }
                >
                  <Button
                    className="button-actions"
                    onClick={toggleReactivate}
                    variant={ButtonVariant.secondary}
                    disabled={isSubmitting}
                  >
                    Reactivate
                  </Button>
                </Show>
                <Show
                  visible={
                    values.isEnabled.toString() === 'true' &&
                    values.id > 0 &&
                    values.accountType !== UserAccountTypeName.Distribution
                  }
                >
                  <Button
                    className="button-actions"
                    onClick={toggleDeactivate}
                    variant={ButtonVariant.secondary}
                    disabled={isSubmitting}
                  >
                    Deactivate
                  </Button>
                </Show>
                <Show visible={!!values.id}>
                  <Button
                    className="button-actions"
                    onClick={toggleDelete}
                    variant={ButtonVariant.red}
                    disabled={isSubmitting}
                  >
                    Delete
                  </Button>
                </Show>
              </Row>
            </Col>
            {/* delete account modal */}
            <Modal
              headerText="Delete account"
              component={<div>{getModalNotes(ActionType.Delete)}</div>}
              isShowing={deleteShowing}
              hide={toggleDelete}
              type="default"
              confirmText="Delete"
              onConfirm={async () => {
                toggleDelete();
                await api.deleteUser(user);
                toast.success(`${user.username} has successfully been deleted.`);
                navigate('/admin/users');
              }}
            />
            {/* reactivate account modal */}
            <Modal
              headerText="Reactivate account"
              component={
                <div>
                  {getModalNotes(ActionType.Reactivate)}
                  <SelectDate
                    name="reactivateDate"
                    label="Date of change"
                    width={FieldSize.Big}
                    dateFormat="MMMM dd, yyyy"
                    value={!!dateOfChange ? formatDate(dateOfChange.toString(), false) : ''}
                    onChange={(date) => {
                      if (date) {
                        setDateOfChange(date);
                      }
                    }}
                  />
                  <p>Enter date changes are effective for billing purposes</p>
                </div>
              }
              isShowing={reactivateShowing}
              hide={toggleReactivate}
              type="default"
              confirmText="Reactivate"
              onConfirm={async () => {
                handleUserActivation(values, UserChangeTypeName.Enable);
                toggleReactivate();
              }}
            />
            {/* deactivate account modal */}
            <Modal
              headerText="Deactivate account"
              component={
                <div>
                  {getModalNotes(ActionType.Deactivate)}
                  <SelectDate
                    name="deactivateDate"
                    label="Date of change"
                    width={FieldSize.Big}
                    dateFormat="MMMM dd, yyyy"
                    value={!!dateOfChange ? formatDate(dateOfChange.toString(), false) : ''}
                    onChange={(date) => {
                      if (date) {
                        setDateOfChange(date);
                      }
                    }}
                  />
                  <p>Enter date changes are effective for billing purposes</p>
                </div>
              }
              isShowing={deactivateShowing}
              hide={toggleDeactivate}
              type="default"
              confirmText="Deactivate"
              onConfirm={async () => {
                handleUserActivation(values, UserChangeTypeName.Disable);
                toggleDeactivate();
              }}
            />
            {/* account type change modal */}
            <Modal
              headerText="Account billing change"
              component={
                <div>
                  {getModalNotes(ActionType.Save)}
                  <SelectDate
                    name="startDate"
                    label="Date of change"
                    width={FieldSize.Big}
                    dateFormat="MMMM dd, yyyy"
                    value={!!dateOfChange ? formatDate(dateOfChange.toString(), false) : ''}
                    onChange={(date) => {
                      if (date) {
                        setDateOfChange(date);
                      }
                    }}
                  />
                </div>
              }
              isShowing={accountBillingChangeShowing}
              hide={toggleAccountBillingChange}
              type="default"
              confirmText="Confirm"
              onConfirm={() => {
                try {
                  handleSave(values);
                } finally {
                  toggleAccountBillingChange();
                }
              }}
            />
          </>
        )}
      </FormikForm>
    </styled.UserForm>
  );
};

export default UserForm;
