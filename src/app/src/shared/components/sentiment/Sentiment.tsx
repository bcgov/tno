import { FaRegCircle, FaRegFrown, FaRegMeh, FaRegSmile } from 'react-icons/fa';

import { Show } from '../show';
import * as styled from './styled';

export interface ISentimentProps {
  /** the numeric value used to determine the icon to present */
  value?: number;
  /** The title attribute */
  title?: string;
}

/**
 * Component that does a quick check on the toning value in order to determine which icon to present
 * @param tone the numeric value used to determine the icon to present
 * @returns An svg image that represents the tone
 */
export const Sentiment: React.FC<ISentimentProps> = ({ value, title }) => {
  return (
    <styled.Sentiment className="sentiment">
      <Show visible={value === undefined}>
        <FaRegCircle className="tone-icon" color="#E0E0E0" title={title} />
      </Show>
      <Show visible={value !== undefined && value < 0}>
        <FaRegFrown className="tone-icon" color="#DC3545" title={title} />
      </Show>
      <Show visible={value !== undefined && (value === 0 || !value)}>
        <FaRegMeh className="tone-icon" color="#FFC107" title={title} />
      </Show>
      <Show visible={value !== undefined && value > 0}>
        <FaRegSmile className="tone-icon" color="#20C997" title={title} />
      </Show>
    </styled.Sentiment>
  );
};
