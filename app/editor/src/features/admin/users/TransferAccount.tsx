import { useFormikContext } from 'formik';
import React from 'react';
import { FaMagnifyingGlass } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  ITransferAccount,
  IUserModel,
  OptionItem,
  Row,
  Section,
  Select,
  Show,
  Text,
  UserAccountTypeName,
} from 'tno-core';

import { defaultTransferAccount } from './constants';
import { useTransferAccount } from './hooks';
import { TransferObjects } from './TransferObjects';

/**
 * Provides a form to setup an account transfer.
 * @returns Component.
 */
export const TransferAccount = () => {
  const { values } = useFormikContext<IUserModel>();
  const [{ users }, { findUsers, transferAccount }] = useUsers();
  const { userObjects, account, setAccount, createTransferAccount, checkIfReady } =
    useTransferAccount();

  const [isTransferring, setIsTransferring] = React.useState(false);
  const [search, setSearch] = React.useState('');

  const populateUserOptions = React.useCallback((userId: number, users: IUserModel[]) => {
    return users
      .filter(
        (u) =>
          u.id !== userId &&
          [UserAccountTypeName.Direct, UserAccountTypeName.Indirect].includes(u.accountType),
      )
      .map(
        (u) =>
          new OptionItem(`${u.username}, ${u.preferredEmail ? u.preferredEmail : u.email}`, u.id),
      );
  }, []);

  const [userOptions, setUserOptions] = React.useState(populateUserOptions(values.id, users.items));

  React.useEffect(() => {
    setUserOptions(populateUserOptions(values.id, users.items));
  }, [values.id, users, populateUserOptions]);

  React.useEffect(() => {
    if (userOptions.length === 0) {
      setAccount({ ...defaultTransferAccount });
    }
  }, [setAccount, userOptions]);

  const handleSearchUsers = React.useCallback(
    async (keyword: string) => {
      try {
        const users = await findUsers({
          keyword,
          accountTypes: [UserAccountTypeName.Direct, UserAccountTypeName.Indirect],
        });
        if (users.items.length === 1 && users.items[0].id !== values.id)
          setAccount((account) => ({ ...account, fromAccountId: users.items[0].id }));
      } catch {}
    },
    [findUsers, setAccount, values.id],
  );

  const handleTransferAccount = React.useCallback(
    async (account: ITransferAccount) => {
      try {
        setIsTransferring(true);
        await transferAccount(account);
        toast.success(
          `Account has been ${account.transferOwnership ? 'transferred' : 'copied'} successfully.`,
        );
        setAccount({ ...defaultTransferAccount });
      } catch {
      } finally {
        setIsTransferring(false);
      }
    },
    [setAccount, transferAccount],
  );

  return (
    <Section className="frm-in">
      <label>Account Mirroring</label>
      <p>
        Copy or transfer another user account reports, notifications, saved searches, and folders to
        this account. Transferring will change the ownership of the objects, where copying will
        create new objects.
      </p>
      <p>
        When transferring an account if any object has the same name as an existing object, it will
        need to be renamed.
      </p>
      <Row gap="1rem">
        <Col flex="1">
          <label htmlFor="user-search">Search for accounts</label>
          <Row>
            <Col flex="1">
              <Row>
                <Col flex="1">
                  <Text
                    id="user-search"
                    name="search"
                    value={search}
                    onChange={(e) => setSearch(e.currentTarget.value)}
                    onKeyDown={(e) => {
                      if (e.code === 'Enter') {
                        e.preventDefault();
                        handleSearchUsers(search);
                      }
                    }}
                  />
                </Col>
                <Button
                  variant={ButtonVariant.action}
                  title="Search for users"
                  onClick={() => handleSearchUsers(search)}
                >
                  <FaMagnifyingGlass />
                </Button>
              </Row>
            </Col>
          </Row>
        </Col>
        <Col flex="1">
          <label htmlFor="select-user">Select account to transfer from</label>
          <Select
            id="select-user"
            name="fromAccountId"
            options={userOptions}
            clearValue={() => {
              setAccount((account) => ({ ...account, fromAccountId: 0 }));
            }}
            value={userOptions.find((o) => o.value === account.fromAccountId) ?? ''}
            onChange={(newValue) => {
              const value = (newValue as OptionItem)?.value;
              const user = value ? users.items.find((u) => u.id === +value) : undefined;
              if (user)
                createTransferAccount(
                  user.id,
                  values.id,
                  account.transferOwnership,
                  account.includeHistory,
                );
            }}
          />
        </Col>
      </Row>
      <Row gap="1rem">
        <Checkbox
          name="account"
          label="Transfer ownership"
          checked={account.transferOwnership}
          onChange={(e) =>
            setAccount((account) => ({
              ...account,
              transferOwnership: e.target.checked,
              includeHistory: e.target.checked,
            }))
          }
        />
        <Show visible={account.transferOwnership}>
          <Checkbox
            name="includeHistory"
            label="Include history"
            checked={account.includeHistory}
            onChange={(e) =>
              setAccount((account) => ({
                ...account,
                includeHistory: e.target.checked,
              }))
            }
          />
        </Show>
      </Row>
      <Section className="transfer-objects">
        <Show visible={!account.fromAccountId}>
          <Col alignContent="center">
            <p>Select an account to transfer from to begin the process.</p>
          </Col>
        </Show>
        <Show visible={!!account.fromAccountId}>
          <div className="grid-section">
            <div>
              <div>
                <Checkbox
                  name="chk-all"
                  checked={
                    !account.notifications.some((n) => !n.checked) &&
                    !account.reports.some((r) => !r.checked) &&
                    !account.folders.some((f) => !f.checked) &&
                    !account.filters.some((f) => !f.checked) &&
                    !account.products.some((f) => !f.checked)
                  }
                  onChange={(e) => {
                    setAccount((account) => ({
                      ...account,
                      notifications: account.notifications.map((n) => ({
                        ...n,
                        checked: e.target.checked,
                      })),
                      reports: account.reports.map((r) => ({ ...r, checked: e.target.checked })),
                      products: account.products.map((p) => ({ ...p, checked: e.target.checked })),
                      filters: account.filters.map((f) => ({ ...f, checked: e.target.checked })),
                      folders: account.folders.map((f) => ({ ...f, checked: e.target.checked })),
                    }));
                  }}
                />
              </div>
              <div className="header">Original Name</div>
              <div className="header">Action</div>
              <div className="header">New Name</div>
            </div>
          </div>
          <TransferObjects
            name="products"
            label="Products"
            items={account.products}
            names={userObjects.products?.map((n) => n.name) ?? []}
            emptyMessage="The specified account is not subscribed to any products."
            transferOwnership={account.transferOwnership}
            onSelect={(id, checked) => {
              setAccount((account) => {
                const products = account.products.map((an) => ({
                  ...an,
                  checked: id === an.originalId ? checked : an.checked,
                }));
                return { ...account, products, isReady: checkIfReady(account) };
              });
            }}
            onChangeName={(id, name) => {
              setAccount((account) => {
                const products = account.products.map((an) => ({
                  ...an,
                  newName: id === an.originalId ? name : an.newName,
                }));
                return { ...account, products, isReady: checkIfReady(account) };
              });
            }}
          />
          <TransferObjects
            name="notification"
            label="Notifications"
            items={account.notifications}
            names={userObjects.notifications?.map((n) => n.name) ?? []}
            emptyMessage="The specified account has no notifications."
            transferOwnership={account.transferOwnership}
            onSelect={(id, checked) => {
              setAccount((account) => {
                const notifications = account.notifications.map((an) => ({
                  ...an,
                  checked: id === an.originalId ? checked : an.checked,
                }));
                const updateAccount = { ...account, notifications };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
            onChangeName={(id, name) => {
              setAccount((account) => {
                const notifications = account.notifications.map((an) => ({
                  ...an,
                  newName: id === an.originalId ? name : an.newName,
                }));
                const updateAccount = { ...account, notifications };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
          />
          <TransferObjects
            name="report"
            label="Reports"
            items={account.reports}
            names={userObjects.reports?.map((n) => n.name) ?? []}
            emptyMessage="The specified account has no reports."
            transferOwnership={account.transferOwnership}
            onSelect={(id, checked) => {
              setAccount((account) => {
                const reports = account.reports.map((an) => ({
                  ...an,
                  checked: id === an.originalId ? checked : an.checked,
                }));
                const updateAccount = { ...account, reports };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
            onChangeName={(id, name) => {
              setAccount((account) => {
                const reports = account.reports.map((an) => ({
                  ...an,
                  newName: id === an.originalId ? name : an.newName,
                }));
                const updateAccount = { ...account, reports };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
          />
          <TransferObjects
            name="filter"
            label="Saved Searches"
            items={account.filters}
            names={userObjects.filters?.map((n) => n.name) ?? []}
            emptyMessage="The specified account has no saved searches."
            transferOwnership={account.transferOwnership}
            onSelect={(id, checked) => {
              setAccount((account) => {
                const filters = account.filters.map((an) => ({
                  ...an,
                  checked: id === an.originalId ? checked : an.checked,
                }));
                const updateAccount = { ...account, filters };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
            onChangeName={(id, name) => {
              setAccount((account) => {
                const filters = account.filters.map((an) => ({
                  ...an,
                  newName: id === an.originalId ? name : an.newName,
                }));
                const updateAccount = { ...account, filters };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
          />
          <TransferObjects
            name="folder"
            label="Folders"
            items={account.folders}
            names={userObjects.folders?.map((n) => n.name) ?? []}
            emptyMessage="The specified account has no folders."
            transferOwnership={account.transferOwnership}
            onSelect={(id, checked) => {
              setAccount((account) => {
                const folders = account.folders.map((an) => ({
                  ...an,
                  checked: id === an.originalId ? checked : an.checked,
                }));
                const updateAccount = { ...account, folders };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
            onChangeName={(id, name) => {
              setAccount((account) => {
                const folders = account.folders.map((an) => ({
                  ...an,
                  newName: id === an.originalId ? name : an.newName,
                }));
                const updateAccount = { ...account, folders };
                return { ...updateAccount, isReady: checkIfReady(updateAccount) };
              });
            }}
          />
        </Show>
      </Section>
      <Col alignItems="center">
        <Button
          variant={ButtonVariant.success}
          disabled={!account.isReady || isTransferring}
          onClick={() => handleTransferAccount(account)}
        >
          {account.transferOwnership ? 'Transfer' : 'Copy'} Account
        </Button>
      </Col>
    </Section>
  );
};
