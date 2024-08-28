import { Action } from 'components/action';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
import React from 'react';
import { FaTrash, FaUserPlus } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useColleagues } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Grid, IUserColleagueModel, Row, Text, useModal, validateEmail } from 'tno-core';

import * as styled from './styled';

export const MyColleaguesLanding = () => {
  const [{ myColleagues, init }] = useProfileStore();
  const [{ getColleagues, deleteColleague, addColleague }] = useColleagues();
  const { toggle, isShowing } = useModal();
  const [colleague, setColleague] = React.useState<IUserColleagueModel>();
  const [emailForAdd, setEmailForAdd] = React.useState('');

  React.useEffect(() => {
    if (!init.myColleagues) {
      getColleagues().catch(() => {});
    }
  }, [getColleagues, init.myColleagues]);

  const handleDelete = React.useCallback(
    async (model: IUserColleagueModel) => {
      if (model) {
        try {
          await deleteColleague(model);
          toast.success(`Successfully deleted '${model.colleague?.email}' as colleague.`);
        } catch {}
      }
    },
    [deleteColleague],
  );

  const [colleagueEmail] = React.useState('');

  const handleSubmit = React.useCallback(
    async (values: string) => {
      try {
        await addColleague(values);
      } catch {
        toast.warning(`No users found for the specified email "${values}".`);
      }
    },
    [addColleague],
  );

  return (
    <styled.MyColleagues>
      <div className="header-row">
        <FaUserPlus className="icon" />
        <span className="header-text">Add a Colleague</span>
      </div>
      <p className="description">
        Add MMI users that you would like to be able to quickly share stories with. From any of the
        story lists, or from the story itself, you can then choose to "share" and find your
        colleagues in this list.
      </p>
      <Row gap="1rem">
        <Col flex="1">
          <div className="colleague-block">
            <div>
              <FaUserPlus size={20} />
            </div>
            <Col>
              <div className="subscriber-title">Add a Colleague</div>
              <div className="subscriber-describe">
                Subscribers will receive this report by email each time it is sent out. To be added,
                a person must have an active MMI account (direct or indirect).
              </div>{' '}
              <Text
                name="email"
                label="FIND BY EMAIL"
                value={colleagueEmail}
                onChange={(e) => setEmailForAdd(e.target.value)}
                width="300px"
              >
                <Button
                  className="request-button"
                  variant="secondary"
                  disabled={!validateEmail(emailForAdd)}
                  onClick={() => handleSubmit(emailForAdd)}
                  style={{ backgroundColor: 'transparent' }}
                >
                  <FaUserPlus />
                  Add
                </Button>
              </Text>
            </Col>
          </div>
        </Col>
      </Row>
      <div className="colleague-block">
        <FaUserPlus size={20} />
        <Col className="subscribers">
          <Row justifyContent="space-between">
            <div className="subscriber-title">My Colleagues</div>
          </Row>
          <Grid
            items={myColleagues}
            renderHeader={() => {
              return [
                <div key="username">Username</div>,
                <div key="firstName">First Name</div>,
                <div key="lastName">Last Name</div>,
                <div key="email">Email</div>,
              ];
            }}
            renderColumns={(row) => {
              return [
                <div key="username">{row.colleague?.username}</div>,
                <div key="firstName">{row.colleague?.firstName}</div>,
                <div key="lastName">{row.colleague?.lastName}</div>,
                <div key="email">
                  {row.colleague?.preferredEmail.length
                    ? row.colleague?.preferredEmail
                    : row.colleague?.email}
                </div>,
                <div key="actions">
                  <Action icon={<FaTrash />} onClick={() => deleteColleague(row)} />
                </div>,
              ];
            }}
          />
        </Col>
      </div>
      <Modal
        headerText="Confirm Delete"
        body={`Are you sure you wish to delete this colleague: '${colleague?.colleague?.email}' ?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          if (colleague) handleDelete(colleague);
          toggle();
        }}
      />
    </styled.MyColleagues>
  );
};
