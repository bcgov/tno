import { Action } from 'components/action';
import { Section } from 'components/section';
import React from 'react';
import { FaTrashCan } from 'react-icons/fa6';
import { IUserColleagueModel } from 'tno-core';

export interface IColleagueCardProps {
  /** The report to display on this card. */
  model: IUserColleagueModel;
  /** Event fires when user requests to delete report. This event does not delete the report itself. */
  onDelete?: (report: IUserColleagueModel) => void;
}

/**
 * A card to display report information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ColleagueCard: React.FC<IColleagueCardProps> = ({ model, onDelete }) => {
  return (
    <Section
      key={model.colleague?.id}
      label={`${model.colleague?.username} - ${
        model.colleague?.preferredEmail ? model.colleague?.preferredEmail : model.colleague?.email
      }`}
      showOpen={false}
      actions={
        <>
          <Action
            icon={<FaTrashCan />}
            onClick={() => {
              onDelete?.(model);
            }}
          />
        </>
      }
    />
  );
};
