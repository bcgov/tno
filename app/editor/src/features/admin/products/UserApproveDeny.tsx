import React from 'react';
import { IUserProductModel, Row, ToggleGroup } from 'tno-core';

export interface IUserApproveDenyProps {
  user: IUserProductModel;
  onChange: (approve: boolean) => void;
}

export const UserApproveDeny: React.FC<IUserApproveDenyProps> = ({ user, onChange }) => {
  return (
    <Row className="user-row">
      <div className="user-name">{`${user.email} `}</div>
      <ToggleGroup
        options={[
          {
            label: 'Approve',
            onClick: () => onChange(true),
          },
          { label: 'Reject', onClick: () => onChange(false) },
        ]}
        className="toggle-group"
      />
    </Row>
  );
};
