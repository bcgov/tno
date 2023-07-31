import 'rc-slider/assets/index.css';

import Slider from 'rc-slider';
import { FaRegFrown, FaRegMeh, FaRegSmile } from 'react-icons/fa';
import { Row, Show } from 'tno-core';

import { IAdvancedSearchFilter } from '../interfaces';

export interface ISentimentSectionProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  sentimentExpanded: boolean;
  /** function that will update the startOn/endOn for the advanced filter */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** advanced search object, may start as undefined if nothing is set */
  advancedSearch: IAdvancedSearchFilter;
}

/** section that contains the sentiment slider to filter content on tone values */
export const SentimentSection: React.FC<ISentimentSectionProps> = ({
  sentimentExpanded,
  advancedSearch,
  setAdvancedSearch,
}) => {
  // marks are what appears under each slider range option
  const marks = {
    '-5': <FaRegFrown className="tone-icon" color="#DC3545" />,
    '-4': '-4',
    '-3': '-3',
    '-2': '-2',
    '-1': '-1',
    '0': <FaRegMeh className="tone-icon" color="#FFC107" />,
    '1': '1',
    '2': '2',
    '3': '3',
    '4': '4',
    '5': <FaRegSmile className="tone-icon" color="#20C997" />,
  };
  return (
    <Show visible={sentimentExpanded}>
      <Row className="expanded sentiment-range">
        <Slider
          marks={marks}
          range
          min={-5}
          max={5}
          defaultValue={[0, 0]}
          onChange={(e) =>
            // if e is a number then it is a single value, but backend expects an array
            setAdvancedSearch({ ...advancedSearch, sentiment: typeof e === 'number' ? [e] : e })
          }
        />
      </Row>
    </Show>
  );
};
