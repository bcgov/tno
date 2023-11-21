import { useContent, useLookup } from 'store/hooks';
import { Checkbox, Col, Settings } from 'tno-core';

export const MoreOptions: React.FC = () => {
  const [{ settings }] = useLookup();
  const [{ filter }, { storeFilter }] = useContent();
  var frontPageId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;
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
          checked={!filter.searchUnpublished}
          onChange={(e) => {
            storeFilter({
              ...filter,
              searchUnpublished: !e.target.checked,
            });
          }}
        />
        <Checkbox
          label="are marked as top stories"
          checked={Boolean(filter.topStory)}
          onChange={(e) => {
            storeFilter({ ...filter, topStory: e.target.checked });
          }}
        />
        <Checkbox
          label="are marked as a front page"
          checked={filter.mediaTypeIds?.includes(Number(frontPageId))}
          onChange={(e) =>
            storeFilter({
              ...filter,
              mediaTypeIds: e.target.checked
                ? [...[Number(frontPageId)], ...(filter.mediaTypeIds ?? [])]
                : filter.mediaTypeIds?.filter((id) => id !== Number(frontPageId)),
            })
          }
        />
        <Checkbox
          label={'bold keywords on search page'}
          checked={Boolean(filter.boldKeywords)}
          onChange={(e) => storeFilter({ ...filter, boldKeywords: e.target.checked })}
        />
      </Col>
    </div>
  );
};
