import { Checkbox, IOptionItem, OptionItem, RadioGroup, Select, SelectDate } from 'components/form';
import { IContentModel } from 'hooks/api-editor';
import React from 'react';
import ReactTooltip from 'react-tooltip';
import { useContent, useLookup } from 'store/hooks';
import { initialContentState } from 'store/slices';
import { Button, ButtonVariant, FieldSize, Loader, Text } from 'tno-core';
import { Col, Row } from 'tno-core/dist/components/flex';
import { Page } from 'tno-core/dist/components/grid-table';
import { getSortableOptions, getUserOptions } from 'utils';

import { fieldTypes, timeFrames } from './constants';
import { IContentListFilter } from './interfaces';
import * as styled from './styled';

export interface IContentFilterProps {
  search: (filter: IContentListFilter) => Promise<Page<IContentModel>>;
  onReady?: (isReady: boolean) => void;
  updated?: boolean;
  setUpdated?: (updated: boolean) => void;
}

export const ContentFilter: React.FC<IContentFilterProps> = ({
  search,
  onReady,
  updated,
  setUpdated,
}) => {
  const [{ contentTypes, mediaTypes, users }] = useLookup();
  const [{ filter, filterAdvanced }, { storeFilter, storeFilterAdvanced }] = useContent();

  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [contentTypeOptions, setContentTypeOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [timeframe, setTimeframe] = React.useState(timeFrames[Number(filter.timeFrame)]);

  const printContentId = (contentTypeOptions.find((ct) => ct.label === 'Print')?.value ??
    0) as number;

  React.useEffect(() => {
    if (!!updated) {
      search({ ...filter, pageIndex: 0, ...filterAdvanced });
      setUpdated && setUpdated(false);
    }
  }, [updated, filter, storeFilterAdvanced, search, setUpdated, filterAdvanced]);

  React.useEffect(() => {
    setContentTypeOptions(getSortableOptions(contentTypes));
  }, [contentTypes]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(mediaTypes, [new OptionItem<number>('All Media', 0)]));
  }, [mediaTypes]);

  React.useEffect(() => {
    setUserOptions(
      getUserOptions(
        users.filter((u) => !u.isSystemAccount),
        [new OptionItem<number>('All Users', 0)],
      ),
    );
  }, [users]);

  React.useEffect(() => {
    onReady?.(!!users.length && !!contentTypes.length && !!mediaTypes.length);
  }, [users, contentTypes, mediaTypes, onReady]);

  /** Handle enter key pressed for advanced filter */
  React.useEffect(() => {
    const keyDownHandler = (event: KeyboardEvent) => {
      if (event.key === 'Enter') {
        event.preventDefault();
        search({ ...filter, pageIndex: 0, ...filterAdvanced });
      }
    };

    document.addEventListener('keydown', keyDownHandler);

    return () => {
      document.removeEventListener('keydown', keyDownHandler);
    };
  }, [filter, filterAdvanced, search]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const handleTimeChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = +e.target.value;
    const timeFrame = timeFrames.find((tf) => tf.value === value);
    setTimeframe(timeframe);
    storeFilter({
      ...filter,
      pageIndex: 0,
      timeFrame: timeFrame?.value ?? 0,
    });
  };

  return (
    <styled.ContentFilter className="content-filter">
      <div>
        <Loader size="5em" visible={!users.length || !contentTypes.length || !mediaTypes.length} />
        <Select
          name="mediaType"
          label="Media Type"
          options={mediaTypeOptions}
          value={mediaTypeOptions.find((mt) => mt.value === filter.mediaTypeId)}
          defaultValue={mediaTypeOptions[0]}
          onChange={(newValue) => {
            var mediaTypeId = (newValue as IOptionItem).value ?? 0;
            storeFilter({
              ...filter,
              pageIndex: 0,
              mediaTypeId: mediaTypeId as number,
            });
          }}
        />
        <Select
          name="user"
          label="User"
          options={userOptions}
          value={userOptions.find((u) => u.value === filter.userId)}
          onChange={(newValue) => {
            var userId = (newValue as IOptionItem).value ?? '';
            storeFilter({
              ...filter,
              pageIndex: 0,
              userId: typeof userId === 'string' ? '' : userId,
            });
          }}
        />
        <RadioGroup
          name="timeFrame"
          label="Time Frame"
          direction="col-row"
          tooltip="Date created"
          value={timeframe}
          options={timeFrames}
          onChange={handleTimeChange}
          disabled={!!filterAdvanced.startDate || !!filterAdvanced.endDate}
        />
        <div className="frm-in chg">
          <label>Filters</label>
          <div className="action-filters">
            <div>
              <Checkbox
                name="isPrintContent"
                label="Print Content"
                tooltip="Newspaper content without audio/video"
                value={printContentId}
                checked={filter.contentTypeId !== 0}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    contentTypeId: e.target.checked ? printContentId : 0,
                  });
                }}
              />
              <Checkbox
                name="included"
                label="Included"
                value="Included"
                checked={filter.included !== ''}
                onChange={(e) =>
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    included: e.target.checked ? e.target.value : '',
                  })
                }
              />
              <Checkbox
                name="ticker"
                label="On Ticker"
                value="On Ticker"
                checked={filter.onTicker !== ''}
                onChange={(e) =>
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    onTicker: e.target.checked ? e.target.value : '',
                  })
                }
              />
            </div>
            <div>
              <Checkbox
                name="commentary"
                label="Commentary"
                value="Commentary"
                checked={filter.commentary !== ''}
                onChange={(e) =>
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    commentary: e.target.checked ? e.target.value : '',
                  })
                }
              />
              <Checkbox
                name="topStory"
                label="Top Story"
                value="Top Story"
                checked={filter.topStory !== ''}
                onChange={(e) =>
                  storeFilter({
                    ...filter,
                    pageIndex: 0,
                    topStory: e.target.checked ? e.target.value : '',
                  })
                }
              />
            </div>
          </div>
        </div>
      </div>
      <div className="box">
        <h2 className="caps">Advanced Search</h2>
        <div style={{ display: 'flex', flexDirection: 'row', minWidth: '550px' }}>
          <Select
            name="fieldType"
            label="Field Type"
            options={fieldTypes}
            value={filterAdvanced.fieldType}
            width={FieldSize.Medium}
            onChange={(newValue) => {
              const value =
                newValue instanceof OptionItem ? newValue.toInterface() : (newValue as IOptionItem);
              storeFilterAdvanced({ ...filterAdvanced, fieldType: value });
            }}
          />
          <Text
            className="test"
            name="searchTerm"
            label="Search Terms"
            value={filterAdvanced.searchTerm}
            onChange={(e) => {
              storeFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value.trim() });
            }}
          ></Text>
        </div>
        <Col className="frm-in dateRange" alignItems="flex-start">
          <label data-for="main-tooltip" data-tip="Date created">
            Date Range
          </label>
          <Row>
            <SelectDate
              name="startDate"
              placeholderText="YYYY MM DD"
              selected={!!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined}
              width={FieldSize.Small}
              onChange={(date) =>
                storeFilterAdvanced({
                  ...filterAdvanced,
                  startDate: !!date ? date.toString() : undefined,
                })
              }
            />
            <SelectDate
              name="endDate"
              placeholderText="YYYY MM DD"
              selected={!!filterAdvanced.endDate ? new Date(filterAdvanced.endDate) : undefined}
              width={FieldSize.Small}
              onChange={(date) => {
                date?.setHours(23, 59, 59);
                storeFilterAdvanced({
                  ...filterAdvanced,
                  endDate: !!date ? date.toString() : undefined,
                });
              }}
            />
          </Row>
        </Col>
        <Button
          name="search"
          onClick={() => search({ ...filter, pageIndex: 0, ...filterAdvanced })}
        >
          Search
        </Button>
        <Button
          name="clear"
          variant={ButtonVariant.secondary}
          onClick={() => {
            storeFilterAdvanced({
              ...initialContentState.filterAdvanced,
            });
            storeFilter({ ...filter, pageIndex: 0 });
          }}
        >
          Clear
        </Button>
      </div>
    </styled.ContentFilter>
  );
};
