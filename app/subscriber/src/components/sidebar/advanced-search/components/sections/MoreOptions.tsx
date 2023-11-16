import { Checkbox, Col } from 'tno-core';

import { IExpandedSectionProps } from '../../interfaces';

export const MoreOptions: React.FC<IExpandedSectionProps> = ({
  setAdvancedSearch,
  advancedSearch,
}) => {
  return (
    <div className="more-options">
      <p>
        MMI imports and archives many more stories than those published to out website. You can
        search within the published stories, or expand your search to the whole dataset. Narrow your
        search using these settings.
      </p>
      <Col>
        <Checkbox
          label="featured on the MMI home page"
          checked={!advancedSearch.useUnpublished}
          onChange={(e) => {
            setAdvancedSearch({
              ...advancedSearch,
              useUnpublished: !e.target.checked,
            });
          }}
        />
        <Checkbox
          label="are marked as top stories"
          checked={Boolean(advancedSearch.topStory)}
          onChange={(e) => {
            setAdvancedSearch({ ...advancedSearch, topStory: e.target.checked });
          }}
        />
        <Checkbox
          label="are marked as a front page"
          checked={Boolean(advancedSearch.frontPage)}
          onChange={(e) => setAdvancedSearch({ ...advancedSearch, frontPage: e.target.checked })}
        />
        <Checkbox
          label={'bold keywords on search page'}
          checked={Boolean(advancedSearch.boldKeywords)}
          onChange={(e) => setAdvancedSearch({ ...advancedSearch, boldKeywords: e.target.checked })}
        />
      </Col>
    </div>
  );
};
