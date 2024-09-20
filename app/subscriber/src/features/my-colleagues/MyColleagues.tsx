import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { PageSection } from 'components/section';
import React from 'react';
import { FaClipboard, FaUserPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useColleagues } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IUserColleagueModel, Row, useModal } from 'tno-core';

import { ColleagueCard } from './ColleagueCard';
import { ColleagueActionEnum } from './constants/ColleagueActionEnum';
import { IMyColleaguesProps } from './interfaces/IMyColleaguesProps';
import * as styled from './styled/MyColleagues';

export const MyColleagues: React.FC<IMyColleaguesProps> = ({ inFrame, changeAction }) => {
  const [{ myColleagues, init }] = useProfileStore();
  const [{ getColleagues, deleteColleague }] = useColleagues();
  const { toggle, isShowing } = useModal();
  const [colleague, setColleague] = React.useState<IUserColleagueModel>();

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

  return (
    <styled.MyColleagues>
      <PageSection header={!inFrame ? 'My Colleagues' : ''}>
        <h2 className="product-section-title">
          <FaUserPlus /> <span>Add a Colleague</span>
        </h2>
        <p className="info-text">
          Add MMI users that you would like to be able to quickly share stories with. From any of
          the story lists, or from the story itself, you can then choose to "share" and find your
          colleagues in this list.
        </p>
        <Row flex="1" justifyContent="flex-end">
          <Action
            label="Add a Colleague"
            icon={<FaClipboard />}
            onClick={() => changeAction(ColleagueActionEnum.Edit)}
          />
        </Row>
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
