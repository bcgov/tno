import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import React from 'react';
import { FaPen, FaTrashCan } from 'react-icons/fa6';
import { useNavigate } from 'react-router';
import { Col, IColleagueModel } from 'tno-core';

export interface IColleagueCardProps {
  /** The report to display on this card. */
  colleague: IColleagueModel;
  /** Event fires when user requests to delete report. This event does not delete the report itself. */
  onDelete?: (report: IColleagueModel) => void;
}

/**
 * A card to display report information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ColleagueCard: React.FC<IColleagueCardProps> = ({ colleague, onDelete }) => {
  const navigate = useNavigate();
  return (
    <Section
      key={colleague.colleague.id}
      label={colleague.colleague.username}
      showOpen={false}
      actions={
        <>
          <Button onClick={() => navigate(`/colleague/${colleague.colleague.id}/edit`)}>
            Edit <FaPen />
          </Button>
          <Action
            icon={<FaTrashCan />}
            onClick={() => {
              onDelete?.(colleague);
            }}
          />
        </>
      }
    ></Section>
  );
};
