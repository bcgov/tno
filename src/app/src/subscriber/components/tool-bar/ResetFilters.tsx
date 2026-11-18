import React from 'react';
import { IoMdRefresh } from 'react-icons/io';

import * as styled from './styled';

export interface IResetFiltersProps {
  /** Callback to clear the selected content. */
  onReset?: () => void;
}

export const ResetFilters: React.FC<IResetFiltersProps> = ({ onReset }) => {
  return (
    <styled.AddToMenu>
      <div className="action" onClick={() => onReset?.()}>
        <IoMdRefresh /> <span>RESET</span>
      </div>
    </styled.AddToMenu>
  );
};
