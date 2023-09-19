import { FaRegFrown, FaRegMeh, FaRegSmile } from 'react-icons/fa';
import { Show } from 'tno-core';

import * as styled from './styled';

export interface IDetermineToneIconProps {
  /** the numeric value used to determine the icon to present */
  tone: number;
}

/**
 * Component that does a quick check on the toning value in order to determine which icon to present
 * @param tone the numeric value used to determine the icon to present
 * @returns An svg image that represents the tone
 */
export const DetermineToneIcon: React.FC<IDetermineToneIconProps> = ({ tone }) => {
  return (
    <styled.DetermineToneIcont>
      <Show visible={tone < 0}>
        <FaRegFrown className="tone-icon" color="#DC3545" />
      </Show>
      <Show visible={tone === 0 || !tone}>
        <FaRegMeh className="tone-icon" color="#FFC107" />
      </Show>
      <Show visible={tone > 0}>
        <FaRegSmile className="tone-icon" color="#20C997" />
      </Show>
    </styled.DetermineToneIcont>
  );
};
