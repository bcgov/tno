import { FaCircle, FaFrown, FaMeh, FaSmile } from 'react-icons/fa';
import { Show } from 'tno-core';

import * as styled from './styled';

export interface ISentimentProps {
  /** the numeric value used to determine the icon to present */
  value?: number;
  /** The title attribute */
  title?: string;
  /** Whether to also show the value. */
  showValue?: boolean;
  /** Whether to show the icon. */
  showIcon?: boolean;
}

/**
 * Component that does a quick check on the toning value in order to determine which icon to present
 * @param tone the numeric value used to determine the icon to present
 * @returns An svg image that represents the tone
 */
export const Sentiment: React.FC<ISentimentProps> = ({
  value,
  title,
  showIcon = true,
  showValue,
}) => {
  return (
    <styled.Sentiment value={value}>
      <Show visible={showIcon}>
        <Show visible={value === undefined}>
          <FaCircle className="tone-icon" color="#E0E0E0" title={title} />
        </Show>
        <Show visible={value !== undefined && value < 0}>
          <FaFrown className="tone-icon" color="#EB8585" title={title} />
        </Show>
        <Show visible={value !== undefined && value === 0}>
          <FaMeh className="tone-icon" color="#F1C02D" title={title} />
        </Show>
        <Show visible={value !== undefined && value > 0}>
          <FaSmile className="tone-icon" color="#59E9BE" title={title} />
        </Show>
      </Show>
      {showValue && <span>{value}</span>}
    </styled.Sentiment>
  );
};
