import { Checkbox, Col, Show } from 'tno-core';

import { IAdvancedSearchFilter } from '../interfaces';

export interface IMoreOptionsProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  optionsExpanded: boolean;
  /** function that will update the search in terms */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** advanced search object, may start as undefined if nothing is set */
  advancedSearch: IAdvancedSearchFilter;
}

export const MoreOptions: React.FC<IMoreOptionsProps> = ({
  optionsExpanded,
  setAdvancedSearch,
  advancedSearch,
}) => {
  return (
    <Show visible={optionsExpanded}>
      <div className="more-options">
        <p>
          MMIA imports and archives many more stories than those published to out website. You can
          search within the published stories, or expand your search to the whole dataset. Narrow
          your search using these settings.
        </p>
        <Col>
          <Checkbox
            label="featured on the MMIA home page"
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
            label="includes an image"
            checked={Boolean(advancedSearch.hasFile)}
            onChange={(e) => setAdvancedSearch({ ...advancedSearch, hasFile: e.target.checked })}
          />
          <Checkbox
            label={'bold keywords on search page'}
            checked={Boolean(advancedSearch.boldKeywords)}
            onChange={(e) =>
              setAdvancedSearch({ ...advancedSearch, boldKeywords: e.target.checked })
            }
          />
        </Col>
      </div>
    </Show>
  );
};
