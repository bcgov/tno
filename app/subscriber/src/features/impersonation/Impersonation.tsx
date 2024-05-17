import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { Button } from 'components/button';
import { PageSection } from 'components/section';
import React from 'react';
import {
  FaArrowRightFromBracket,
  FaArrowRightToBracket,
  FaCaretLeft,
  FaCaretRight,
  FaMagnifyingGlass,
} from 'react-icons/fa6';
import { useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IPaged, ISubscriberUserModel, IUserFilter, IUserModel, Row, Text } from 'tno-core';

import { defaultPage } from './constants';
import * as styled from './styled';

export const Impersonation = () => {
  const { findUsers, updateUser } = useUsers();
  const [{ profile }] = useProfileStore();

  const [page, setPage] = React.useState('');
  const [keyword, setKeyword] = React.useState('');
  const [users, setUsers] = React.useState<IPaged<IUserModel>>(defaultPage);

  const handleSearch = React.useCallback(
    async (filter: IUserFilter) => {
      try {
        const page = await findUsers(filter);
        setUsers(page);
        setPage(`${filter.page ?? 1}`);
      } catch {}
    },
    [findUsers],
  );

  const handleImpersonate = React.useCallback(
    async (user: ISubscriberUserModel | undefined) => {
      try {
        if (profile) {
          await updateUser({
            ...profile,
            preferences: { ...profile?.preferences, impersonate: user?.key },
          });
          window.location.reload();
        }
      } catch {}
    },
    [profile, updateUser],
  );

  React.useEffect(() => {
    handleSearch({ page: 1, quantity: 100 }).catch(() => {});
    // Initialize on first load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.Impersonation>
      <PageSection header="Users">
        <Bar>
          <Row>
            <Text
              name="search"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  handleSearch({ page: 1, quantity: users.quantity, keyword });
                  e.preventDefault();
                  return false;
                }
              }}
            >
              <Button
                onClick={() => {
                  handleSearch({ page: 1, quantity: users.quantity, keyword });
                }}
              >
                <FaMagnifyingGlass />
              </Button>
            </Text>
            <Action
              icon={<FaArrowRightFromBracket />}
              label="Exit impersonation"
              onClick={() => handleImpersonate(undefined)}
              disabled={profile?.preferences?.impersonate === undefined}
            />
          </Row>
        </Bar>
        <Col className="table">
          <Row className="table-header">
            <Col flex="1">Username</Col>
            <Col flex="1">Email</Col>
            <Col flex="1">First Name</Col>
            <Col flex="1">Last Name</Col>
            <Col></Col>
          </Row>
          <Col className="table-rows">
            {users.items.map((user) => {
              return (
                <Row
                  key={user.id}
                  className={profile?.preferences?.impersonate === user.key ? 'active' : ''}
                >
                  <Col flex="1">{user.username}</Col>
                  <Col flex="1">{user.preferredEmail ? user.preferredEmail : user.email}</Col>
                  <Col flex="1">{user.firstName}</Col>
                  <Col flex="1">{user.lastName}</Col>
                  <Col>
                    <Action
                      icon={<FaArrowRightToBracket />}
                      title="Impersonate"
                      onClick={() => handleImpersonate(user)}
                    />
                  </Col>
                </Row>
              );
            })}
          </Col>
          <Row className="table-footer">
            <Action
              icon={<FaCaretLeft />}
              disabled={users.page <= 1}
              onClick={() => {
                handleSearch({ page: users.page - 1, quantity: users.quantity, keyword });
              }}
            />
            <Col>
              <Text
                name="page"
                value={page}
                width="5ch"
                type="number"
                onChange={(e) => {
                  setPage(e.target.value);
                }}
                onBlur={() => {
                  const pageValue = parseInt(page);
                  if (Number.isNaN(pageValue)) {
                    setPage(users.page.toString());
                  } else if (pageValue > 0 && pageValue !== users.page) {
                    setPage(pageValue.toString());
                    handleSearch({ page: pageValue, quantity: users.quantity, keyword });
                  }
                }}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    const pageValue = parseInt(page);
                    if (Number.isNaN(pageValue)) {
                      setPage(users.page.toString());
                    } else if (pageValue > 0 && pageValue !== users.page) {
                      setPage(pageValue.toString());
                      handleSearch({ page: pageValue, quantity: users.quantity, keyword });
                    }
                    e.preventDefault();
                    return false;
                  }
                }}
              />
            </Col>
            <Action
              icon={<FaCaretRight />}
              disabled={users.total <= users.page * users.quantity}
              onClick={() => {
                handleSearch({ page: users.page + 1, quantity: users.quantity, keyword });
              }}
            />
          </Row>
        </Col>
      </PageSection>
    </styled.Impersonation>
  );
};
