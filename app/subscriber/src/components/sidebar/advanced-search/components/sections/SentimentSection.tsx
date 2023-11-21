import { useContent } from 'store/hooks';
import { Col, SentimentSlider } from 'tno-core';

/** section that contains the sentiment slider to filter content on tone values */
export const SentimentSection: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  return (
    <Col justifyItems="stretch">
      <SentimentSlider
        value={filter.sentiment ?? []}
        onChange={(value) => {
          // if e is a number then it is a single value, but backend expects an array
          storeFilter({
            ...filter,
            sentiment: typeof value === 'number' ? [value] : value,
          });
        }}
      />
    </Col>
  );
};
