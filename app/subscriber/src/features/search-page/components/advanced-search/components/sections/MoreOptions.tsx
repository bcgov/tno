import { getFilterActions } from 'features/search-page/utils';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import {
  ActionName,
  Checkbox,
  Col,
  IFilterActionSettingsModel,
  Row,
  Settings,
  Text,
} from 'tno-core';

export const MoreOptions: React.FC = () => {
  const [{ settings, actions }] = useLookup();
  const [
    {
      search: { filter: filterSettings },
    },
    { storeSearchFilter },
  ] = useContent();

  const [actionFilters] = React.useState<{ [actionName: string]: IFilterActionSettingsModel }>(
    getFilterActions(actions),
  );
  const topStoryAction = actionFilters[ActionName.TopStory];

  const [size, setSize] = React.useState<number | string>(filterSettings.size);

  var frontPageId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;

  React.useEffect(() => {
    setSize(filterSettings.size);
  }, [filterSettings.size]);

  return (
    <div className="more-options">
      <p>
        MMI imports and archives many more stories than those published to out website. You can
        search within the published stories, or expand your search to the whole dataset. Narrow your
        search using these settings.
      </p>
      <Row gap="1rem">
        <Col>
          <Checkbox
            label="featured stories ONLY - uncheck to search ALL content"
            name="featured"
            checked={!filterSettings.searchUnpublished}
            onChange={(e) => {
              storeSearchFilter({
                ...filterSettings,
                searchUnpublished: !e.target.checked,
              });
            }}
          />
          <Checkbox
            label="are marked as top stories"
            name="topStory"
            checked={filterSettings.actions?.map((a) => a.id).includes(topStoryAction.id)}
            onChange={(e) => {
              const newFilterSettings = {
                ...filterSettings,
                actions: e.target.checked
                  ? [...filterSettings.actions!, topStoryAction] // add it
                  : filterSettings.actions?.filter((a) => a.id !== topStoryAction?.id), // remove it
              };
              storeSearchFilter(newFilterSettings);
            }}
          />
          <Checkbox
            label="are marked as a front page"
            name="frontPage"
            checked={filterSettings.mediaTypeIds?.includes(Number(frontPageId))}
            onChange={(e) =>
              storeSearchFilter({
                ...filterSettings,
                mediaTypeIds: e.target.checked
                  ? [...[Number(frontPageId)], ...(filterSettings.mediaTypeIds ?? [])]
                  : filterSettings.mediaTypeIds?.filter((id) => id !== Number(frontPageId)),
              })
            }
          />
          <Checkbox
            label={'bold keywords on search page'}
            name="boldKeywords"
            checked={Boolean(filterSettings.boldKeywords)}
            onChange={(e) =>
              storeSearchFilter({ ...filterSettings, boldKeywords: e.target.checked })
            }
          />
        </Col>
        <Col>
          <Text
            name="size"
            label="Quantity Limit (max 500)"
            type="number"
            width="10ch"
            value={size}
            onChange={(e) => {
              setSize(e.target.value);
              const value = e.target.value ? +e.target.value : 500;
              if (value <= 500) {
                storeSearchFilter({ ...filterSettings, size: value });
              }
            }}
          />
        </Col>
      </Row>
    </div>
  );
};
