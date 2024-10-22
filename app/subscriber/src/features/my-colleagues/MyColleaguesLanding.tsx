import { Action } from 'components/action';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
import React from 'react';
import { FaTrash, FaUserPlus, FaUsers } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useColleagues } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Grid, IUserColleagueModel, Text, useModal, validateEmail } from 'tno-core';

import * as styled from './styled';

export const MyColleaguesLanding = () => {
  const [{ myColleagues, init }] = useProfileStore();
  const [{ getColleagues, deleteColleague, addColleague }] = useColleagues();
  const { toggle, isShowing } = useModal();
  const [colleague, setColleague] = React.useState<IUserColleagueModel>();
  const [colleagueEmail, setEmailForAdd] = React.useState('');

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

  const handleSubmit = React.useCallback(
    async (values: string) => {
      try {
        await addColleague(values);
        setEmailForAdd('');
        toast.success(`Successfully added '${values}' as colleague.`);
      } catch {
        //toast.warning(`No users found for the specified email "${values}".`);
        //error message handled by addColleague()
      }
    },
    [addColleague],
  );

  return (
    <styled.MyColleagues>
      <div className="colleague-block">
        <Col>
          <div className="colleague-title">
            <FaUserPlus className="icon" />
            <span>Add a Colleague</span>
          </div>
          <div className="colleague-body">
            <div className="colleague-describe">
              Add MMI users that you would like to be able to quickly share stories with. From any
              of the story lists, or from the story itself, you can then choose to "share" and find
              your colleagues in this list.
            </div>{' '}
            <Text
              name="email"
              label="FIND BY EMAIL:"
              value={colleagueEmail}
              onChange={(e) => setEmailForAdd(e.target.value)}
              width="300px"
            >
              <Button
                className="request-button"
                variant="secondary"
                disabled={!validateEmail(colleagueEmail)}
                onClick={() => handleSubmit(colleagueEmail)}
                style={{ backgroundColor: 'transparent' }}
              >
                <FaUserPlus />
                Add
              </Button>
            </Text>
          </div>
        </Col>
      </div>
      <div className="colleague-block">
        <Col>
          <div className="colleague-title">
            <FaUsers className="icon" />
            <span>My Colleagues</span>
          </div>
          <div className="colleague-body">
            <Grid
              className="colleague-table"
              items={myColleagues}
              renderHeader={() => {
                return [
                  <div key="username">Username</div>,
                  <div key="firstName">First Name</div>,
                  <div key="lastName">Last Name</div>,
                  <div key="email">Email</div>,
                  <div key="trash"></div>,
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
                    <Action
                      icon={<FaTrash />}
                      onClick={() => {
                        setColleague(row);
                        toggle();
                      }}
                    />
                  </div>,
                ];
              }}
            />
          </div>
        </Col>
      </div>
      <Modal
        headerText="Confirm Delete"
        body={`Are you sure you wish to delete this colleague: '${colleague?.colleague?.email}' ?`}
        isShowing={isShowing}
        onClose={toggle}
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
