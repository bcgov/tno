import { Action } from 'components/action';
import { Bar } from 'components/bar';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useColleagues } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IUserColleagueModel, Row, useModal } from 'tno-core';
import { Modal } from 'components/modal';

import { ColleagueCard } from './ColleagueCard';
import * as styled from './styled/MyColleagues';

export const MyColleagues: React.FC = () => {
  const [{ myColleagues, init }] = useProfileStore();
  const [{ getColleagues, deleteColleague }] = useColleagues();
  const { toggle, isShowing } = useModal();
  const [colleague, setColleague] = React.useState<IUserColleagueModel>();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!init.myColleagues) {
      getColleagues().catch(() => {});
    }
  }, [getColleagues, init.myColleagues]);

  const handleDelete = React.useCallback(
    async (model: IUserColleagueModel) => {
      if (!!model) {
        try {
          await deleteColleague(model);
          toast.success(`Successfully deleted '${model.colleague?.email}' as colleague.`);
        } catch {}
      }
    },
    [deleteColleague],
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
          {myColleagues.map((colleague, index) => {
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
