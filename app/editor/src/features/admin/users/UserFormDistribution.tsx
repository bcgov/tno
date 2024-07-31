import { useFormikContext } from 'formik';
import React from 'react';
import { FaSearch } from 'react-icons/fa';
import { FaPlus, FaTrash } from 'react-icons/fa6';
import { useUsers } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IUserModel,
  OptionItem,
  Row,
  Section,
  Select,
  sortObject,
  Text,
  UserAccountTypeName,
} from 'tno-core';

/**
 * Provides a User Form to manage, create, update and delete a user.
 * @returns React component containing administrative user form.
 */
export const UserFormDistribution: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IUserModel>();
  const [, { getDistributionListById, findUsers }] = useUsers();

  const [keyword, setKeyword] = React.useState('');
  const [users, setUsers] = React.useState<IUserModel[]>([]);
  const [selectedUser, setSelectedUser] = React.useState(0);

  const accountTypeOptions = getEnumStringOptions(UserAccountTypeName).filter(
    (o) => o.value !== UserAccountTypeName.SystemAccount,
  );
  const userOptions = users
    .filter((u) =>
      [UserAccountTypeName.Direct, UserAccountTypeName.Indirect].includes(u.accountType),
    )
    .map(
      (u) =>
        new OptionItem(
          `${u.preferredEmail ? u.preferredEmail : u.email} - ${
            [UserAccountTypeName.Direct].includes(u.accountType)
              ? u.username
              : u.accountType.toString()
          }`,
          u.id,
        ),
    );

  const sortedDistributionList = values.distribution
    ? [...values.distribution].sort(sortObject((u) => u.email.toLowerCase()))
    : [];

  const handleFindUsers = React.useCallback(
    async (keyword: string) => {
      try {
        const results = await findUsers({ keyword });
        const users = results.items.filter(
          (u) => u.accountType !== UserAccountTypeName.Distribution,
        );
        setUsers(users);
        setSelectedUser(users.length ? users[0].id : 0);
      } catch {}
    },
    [findUsers],
  );

  React.useEffect(() => {
    if (values.id) {
      getDistributionListById(values.id)
        .then((users) => {
          setFieldValue('distribution', users);
        })
        .catch(() => {});
    }
    // Only on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.id]);

  return (
    <div className="form-container">
      <Section className="frm-in">
        <label>Account Information</label>
        <Row gap="1rem">
          <Col>
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
              label="Name"
              required
              onChange={(e) => setFieldValue('username', e.currentTarget.value.toUpperCase())}
            />
            <FormikTextArea name="note" label="Note" />
          </Col>
          <Section className="frm-in distribution-list">
            <Row>
              <Text
                name="findUser"
                label="Find User"
                value={keyword}
                onChange={(e) => setKeyword(e.currentTarget.value)}
                onKeyDown={(e) => {
                  if (e.code === 'Enter') {
                    e.preventDefault();
                    handleFindUsers(keyword);
                  }
                }}
              />
              <Button title="Search" onClick={() => handleFindUsers(keyword)}>
                <FaSearch />
              </Button>
            </Row>
            <Select
              name="users"
              options={userOptions}
              value={userOptions.find((o) => o.value === selectedUser) ?? ''}
              onChange={(e) => {
                const option = e as OptionItem;
                if (option && option.value) setSelectedUser(+option.value);
                else setSelectedUser(0);
              }}
            >
              <Button
                title="Add"
                disabled={!selectedUser}
                onClick={(e) => {
                  const user = users.find((u) => u.id === selectedUser);
                  const found = user && sortedDistributionList.some((a) => a.id === user.id);
                  if (user && !found) {
                    setFieldValue('distribution', [...sortedDistributionList, user]);
                  }
                  setSelectedUser(0);
                }}
              >
                <FaPlus />
              </Button>
            </Select>
            <label>Email Addresses</label>
            <Section className="addresses">
              <Col gap="0.5rem">
                {sortedDistributionList.map((user) => {
                  return (
                    <Row key={user.id} alignItems="center" nowrap>
                      <Col flex="1">{user.email}</Col>
                      <Button
                        title="Remove"
                        variant={ButtonVariant.red}
                        onClick={(e) => {
                          setFieldValue(
                            'distribution',
                            sortedDistributionList.filter((a) => a.id !== user.id),
                          );
                        }}
                      >
                        <FaTrash />
                      </Button>
                    </Row>
                  );
                })}
              </Col>
            </Section>
          </Section>
        </Row>
      </Section>
    </div>
  );
};
