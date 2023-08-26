import { Col, SentimentSlider, Show } from 'tno-core';

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
  return (
    <Show visible={sentimentExpanded}>
      <Col justifyItems="stretch">
        <SentimentSlider
          value={advancedSearch.sentiment ?? []}
          onChange={(value) => {
            // if e is a number then it is a single value, but backend expects an array
            setAdvancedSearch({
              ...advancedSearch,
              sentiment: typeof value === 'number' ? [value] : value,
            });
          }}
        />
      </Col>
    </Show>
  );
};
