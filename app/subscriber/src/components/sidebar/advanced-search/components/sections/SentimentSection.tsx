import { Col, SentimentSlider } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

/** section that contains the sentiment slider to filter content on tone values */
export const SentimentSection: React.FC<IExpandedSectionProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
  return (
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
  );
};
