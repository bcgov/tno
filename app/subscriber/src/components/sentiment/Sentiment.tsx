import { FaRegCircle, FaRegFrown, FaRegMeh, FaRegSmile } from 'react-icons/fa';
import { Show } from 'tno-core';

import * as styled from './styled';

export interface ISentimentProps {
  /** the numeric value used to determine the icon to present */
  value?: number;
  /** The title attribute */
  title?: string;
  /** Whether to also show the value. */
  showValue?: boolean;
}

/**
 * Component that does a quick check on the toning value in order to determine which icon to present
 * @param tone the numeric value used to determine the icon to present
 * @returns An svg image that represents the tone
 */
export const Sentiment: React.FC<ISentimentProps> = ({ value, title, showValue }) => {
  return (
    <styled.Sentiment value={value}>
      <Show visible={value === undefined}>
        <FaRegCircle className="tone-icon" color="#E0E0E0" title={title} />
      </Show>
      <Show visible={value !== undefined && value < 0}>
        <FaRegFrown className="tone-icon" color="#DC3545" title={title} />
      </Show>
      <Show visible={value !== undefined && value === 0}>
        <FaRegMeh className="tone-icon" color="#FFC107" title={title} />
      </Show>
      <Show visible={value !== undefined && value > 0}>
        <FaRegSmile className="tone-icon" color="#20C997" title={title} />
      </Show>
      {showValue && <span>{value}</span>}
    </styled.Sentiment>
  );
};
