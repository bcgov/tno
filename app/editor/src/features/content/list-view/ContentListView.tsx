import {
  Button,
  ButtonVariant,
  Checkbox,
  Dropdown,
  IOptionItem,
  OptionItem,
  Page,
  PagedTable,
  RadioGroup,
  SelectDate,
  Text,
} from 'components';
import { IContentFilter, LogicalOperator, useApiEditor } from 'hooks';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useKeycloakWrapper } from 'tno-core';

import { columns, fieldTypes, logicalOperators, timeFrames } from './constants';
import * as styled from './ContentListViewStyled';
import { IContentListFilter } from './interfaces';

const defaultListFilter: IContentListFilter = {
  mediaTypeId: 0,
  ownerId: '',
  newspaper: false,
  included: false,
  onTicker: false,
  commentary: false,
  topStory: false,
  fieldType: 'headline',
  logicalOperator: LogicalOperator.Contains,
  searchTerm: '',
};

export const ContentListView: React.FC = () => {
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState<number | undefined>(10);
  const [mediaTypes, setMediaTypes] = React.useState<IOptionItem[]>([]);
  const [users, setUsers] = React.useState<IOptionItem[]>([]);
  const [currentUserId, setCurrentUserId] = React.useState<number>();
  const [timeFrame, setTimeFrame] = React.useState(timeFrames[0]);
  const [fieldType, setFieldType] = React.useState(fieldTypes[0]);
  const [listFilter, setListFilter] = React.useState(defaultListFilter);
  const keycloak = useKeycloakWrapper();
  const navigate = useNavigate();
  const api = useApiEditor();

  const username = keycloak.instance.tokenParsed.username;

  React.useEffect(() => {
    api.getUsers().then((data) => {
      setUsers(
        [new OptionItem('All Users', 0)].concat(
          data.map((u) => new OptionItem(u.displayName, u.id)),
        ),
      );
      const currentUserId = data.find((u) => u.username === username)?.id ?? 0;
      setCurrentUserId(currentUserId);
      setListFilter((filter) => ({ ...filter, ownerId: currentUserId }));
    });
  }, [api, username]);

  React.useEffect(() => {
    api.getMediaTypes().then((data) => {
      setMediaTypes(
        [new OptionItem('All Media', 0)].concat(data.map((m) => new OptionItem(m.name, m.id))),
      );
    });
  }, [api]);

  const makeFilter = (filter: IContentListFilter): IContentFilter => ({
    mediaTypeId: +filter.mediaTypeId > 0 ? +filter.mediaTypeId : undefined,
    ownerId: +filter.ownerId > 0 ? +filter.ownerId : undefined,
    hasPage: filter.newspaper === true ? true : undefined,
    createdStartOn: filter.createdStartOn ? filter.createdStartOn.toISOString() : undefined,
    createdEndOn: filter.createdEndOn ? filter.createdEndOn.toISOString() : undefined,
  });

  const fetch = React.useCallback(
    async (
      pageIndex: number,
      pageSize?: number,
      filter: IContentListFilter = defaultListFilter,
    ) => {
      try {
        const data = await api.getContents(pageIndex, pageSize, makeFilter(filter));
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [api],
  );

  const handlePageChange = (pi: number, ps?: number) => {
    if (pageIndex !== pi) setPageIndex(pi);
    if (pageSize !== ps) setPageSize(ps);
  };

  const handleTimeChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = +e.target.value;
    let createdStartOn: Date | undefined;
    if (value === 0) createdStartOn = moment().startOf('day').toDate();
    else if (value === 1) createdStartOn = moment().add(-24, 'hours').toDate();
    else if (value === 2) createdStartOn = moment().add(-48, 'hours').toDate();
    else createdStartOn = undefined;
    setListFilter((filter) => ({ ...filter, createdStartOn }));
  };

  return (
    <styled.ContentListView>
      <div className="content-filter">
        <div>
          <Dropdown
            name="mediaType"
            label="Media Type"
            options={mediaTypes}
            value={mediaTypes.find((mt) => mt.value === listFilter.mediaTypeId)}
            defaultValue={mediaTypes[0]}
            onChange={(newValue) => {
              var mediaTypeId = (newValue as IOptionItem).value ?? '';
              setListFilter({
                ...listFilter,
                mediaTypeId: typeof mediaTypeId === 'string' ? '' : mediaTypeId,
              });
            }}
          />
          <Dropdown
            name="user"
            label="User"
            options={users}
            value={users.find((u) => u.value === listFilter.ownerId)}
            onChange={(newValue) => {
              var ownerId = (newValue as IOptionItem).value ?? '';
              setListFilter({ ...listFilter, ownerId: typeof ownerId === 'string' ? '' : ownerId });
            }}
          />
          <RadioGroup
            name="timeFrame"
            label="Time Frame"
            value={timeFrame}
            options={timeFrames}
            onChange={handleTimeChange}
          />
          <div className="frm-in chg">
            <label>Filters</label>
            <Checkbox
              name="newspaper"
              label="Lois"
              value="newspaper"
              checked={listFilter.newspaper}
              onChange={(e) => {
                setListFilter({ ...listFilter, newspaper: e.target.checked });
              }}
            />
            <Checkbox
              name="included"
              label="Included"
              value="included"
              checked={listFilter.included}
              onChange={() => setListFilter({ ...listFilter, included: !listFilter.included })}
            />
            <Checkbox
              name="ticker"
              label="On Ticker"
              value="ticker"
              checked={listFilter.onTicker}
              onChange={() => setListFilter({ ...listFilter, onTicker: !listFilter.onTicker })}
            />
            <Checkbox
              name="commentary"
              label="Commentary"
              value="commentary"
              checked={listFilter.commentary}
              onChange={() => setListFilter({ ...listFilter, commentary: !listFilter.commentary })}
            />
            <Checkbox
              name="topStory"
              label="Top Story"
              value="topStory"
              checked={listFilter.topStory}
              onChange={() => setListFilter({ ...listFilter, topStory: !listFilter.topStory })}
            />
          </div>
        </div>
        <div className="box">
          <h2 className="caps">Advanced Search</h2>
          <div>
            <Dropdown
              name="fieldType"
              label="Field Type"
              options={fieldTypes}
              value={fieldType}
              onChange={(newValue) => {
                setFieldType(newValue as OptionItem);
              }}
            />
            <Dropdown
              name="logicalOperator"
              label="Logical Operator"
              options={logicalOperators}
              value={logicalOperators.find(
                (lo) => (LogicalOperator as any)[lo.value] === listFilter.logicalOperator,
              )}
              onChange={(newValue) => {
                const logicalOperator = (LogicalOperator as any)[(newValue as OptionItem).value];
                setListFilter({ ...listFilter, logicalOperator });
              }}
            />
            <Text
              name="searchTerm"
              label="Search Terms"
              value={listFilter.searchTerm}
              onChange={(e) => {
                setListFilter({ ...listFilter, searchTerm: e.target.value.trim() });
              }}
            ></Text>
          </div>
          <div className="dateRange">
            <label>Date Range</label>
            <div>
              <SelectDate
                name="startDate"
                placeholderText="YYYY MM DD"
                selected={listFilter.createdStartOn}
                showTimeSelect
                dateFormat="Pp"
                onChange={(date) => setListFilter({ ...listFilter, createdStartOn: date })}
              />
              <SelectDate
                name="endDate"
                placeholderText="YYYY MM DD"
                selected={listFilter.createdEndOn}
                showTimeSelect
                dateFormat="Pp"
                onChange={(date) => setListFilter({ ...listFilter, createdEndOn: date })}
              />
            </div>
          </div>
          <Button
            name="search"
            onClick={() => {
              // setListFilter({
              //   ...listFilter,
              //   [fieldType.value]: searchTerm.trim() === '' ? undefined : searchTerm,
              //   createdStartOn: !!startDate ? moment(startDate).toISOString() : undefined,
              //   createdEndOn: !!endDate ? moment(endDate).toISOString() : undefined,
              //   logicalOperator:
              //     searchTerm.trim() === '' ? undefined : (logicalOperator.value as LogicalOperator),
              // });
            }}
          >
            Search
          </Button>
          <Button
            name="clear"
            variant={ButtonVariant.secondary}
            onClick={() => {
              setListFilter({ ...defaultListFilter, ownerId: currentUserId ?? '' });
              setTimeFrame(timeFrames[0]);
              setFieldType(fieldTypes[0]);
            }}
          >
            Clear
          </Button>
        </div>
      </div>
      <div className="content-list">
        <PagedTable
          columns={columns}
          onFetch={(pageIndex, pageSize) => fetch(pageIndex, pageSize, listFilter)}
          onRowClick={(row) => navigate(`/contents/${row.id}`)}
          onPageChange={handlePageChange}
        />
      </div>
      <div className="content-actions">
        <Button name="create" onClick={() => navigate('/contents/0')}>
          Create Snippet
        </Button>
        <div className="addition-actions">
          <Button
            name="create"
            variant={ButtonVariant.secondary}
            disabled={true}
            tooltip="Under Construction"
          >
            Send Lois Front Pages
          </Button>
          <Button
            name="create"
            variant={ButtonVariant.secondary}
            disabled={true}
            tooltip="Under Construction"
          >
            Send Top Stories
          </Button>
          <Button
            name="create"
            variant={ButtonVariant.secondary}
            disabled={true}
            tooltip="Under Construction"
          >
            Send Send Lois to Commentary
          </Button>
        </div>
      </div>
    </styled.ContentListView>
  );
};
