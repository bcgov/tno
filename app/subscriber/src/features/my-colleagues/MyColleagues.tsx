import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useColleagues } from 'store/hooks';
import { IUserColleagueModel, Modal, Row, useModal } from 'tno-core';

import { ColleagueCard } from './ColleagueCard';
import * as styled from './styled/MyColleagues';

export const MyColleagues: React.FC = () => {
  const [{ getColleagues, deleteColleague }] = useColleagues();
  const { toggle, isShowing } = useModal();
  const [colleagues, setColleagues] = React.useState<IUserColleagueModel[]>([]);
  const [colleague, setColleague] = React.useState<IUserColleagueModel>();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!colleagues.length) {
      getColleagues().then((data) => {
        setColleagues(data);
      });
    }
  }, [colleagues.length, getColleagues]);

  const handleDelete = React.useCallback(
    (model: IUserColleagueModel) => {
      if (!!model) {
        deleteColleague(model)
          .then((dataDeleted) => {
            getColleagues().then((data) => {
              setColleagues(data);
            });
            toast.success(`Successfully deleted '${dataDeleted.colleague?.email}' as colleague.`);
          })
          .catch(() => {});
      }
    },
    [deleteColleague, getColleagues],
  );

  return (
    <styled.MyColleagues>
      <PageSection header="My Colleagues">
        <Bar>
          <Row flex="1" justifyContent="flex-end">
            <Action
              label="Add Colleague"
              icon={<FaClipboard />}
              onClick={() => navigate('/colleagues/add')}
            />
          </Row>
        </Bar>
        <div>
          {colleagues.map((colleague, index) => {
            return (
              <ColleagueCard
                key={index}
                model={colleague}
                onDelete={(colleague) => {
                  setColleague(colleague);
                  toggle();
                }}
              />
            );
          })}
        </div>
      </PageSection>
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
