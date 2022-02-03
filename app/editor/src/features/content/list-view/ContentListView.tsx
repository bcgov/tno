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

const defaultFilter: IContentFilter = {
  mediaTypeId: 0,
};

export const ContentListView: React.FC = () => {
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState<number | undefined>(10);
  const [mediaTypes, setMediaTypes] = React.useState<IOptionItem[]>([]);
  const [users, setUsers] = React.useState<IOptionItem[]>([]);
  const [currentUserId, setCurrentUserId] = React.useState<number>();
  const [timeFrame, setTimeFrame] = React.useState(timeFrames[0]);
  const [fieldType, setFieldType] = React.useState(fieldTypes[0]);
  const [logicalOperator, setLogicalOperator] = React.useState(logicalOperators[0]);
  const [searchTerm, setSearchTerm] = React.useState<string>('');
  const [startDate, setStartDate] = React.useState<Date | null>();
  const [endDate, setEndDate] = React.useState<Date | null>();
  const [filter, setFilter] = React.useState<IContentFilter>(defaultFilter);
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
      setFilter((filter) => ({ ...filter, ownerId: currentUserId }));
    });
  }, [api, username]);

  React.useEffect(() => {
    api.getMediaTypes().then((data) => {
      setMediaTypes(
        [new OptionItem('All Media', 0)].concat(data.map((m) => new OptionItem(m.name, m.id))),
      );
    });
  }, [api]);

  const fetch = React.useCallback(
    async (pageIndex: number, pageSize?: number, filter?: IContentFilter) => {
      try {
        const cleanFilter = {
          ...filter,
          mediaTypeId: filter?.mediaTypeId === 0 ? undefined : filter?.mediaTypeId,
        };
        const data = await api.getContents(pageIndex, pageSize, cleanFilter);
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
    let createdStartOn: string | undefined;
    if (value === 0) createdStartOn = moment().startOf('day').toISOString();
    else if (value === 1) createdStartOn = moment().add(-24, 'hours').toISOString();
    else if (value === 2) createdStartOn = moment().add(-48, 'hours').toISOString();
    else createdStartOn = undefined;
    setFilter({ ...filter, createdStartOn });
  };

  const handleActionFilterChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = e.target.value;
    if (e.target.checked) {
      const values = (filter.actions ?? []).concat([value]);
      setFilter({
        ...filter,
        actions: [...values.filter((v, i, a) => a.indexOf(v) === i)],
      });
    } else {
      setFilter({
        ...filter,
        actions: filter.actions?.filter((a) => a === value),
      });
    }
  };

  return (
    <styled.ContentListView>
      <div className="content-filter">
        <div>
          <Dropdown
            name="mediaType"
            label="Media Type"
            options={mediaTypes}
            value={mediaTypes.find((mt) => mt.value === filter.mediaTypeId)}
            defaultValue={mediaTypes[0]}
            onChange={(newValue) => {
              var mediaTypeId = (newValue as IOptionItem).value as number;
              setFilter({ ...filter, mediaTypeId: mediaTypeId > 0 ? mediaTypeId : undefined });
            }}
          />
          <Dropdown
            name="user"
            label="User"
            options={users}
            value={users.find((u) => u.value === filter.ownerId)}
            onChange={(newValue) => {
              var ownerId = (newValue as IOptionItem).value as number;
              setFilter({ ...filter, ownerId: ownerId > 0 ? ownerId : undefined });
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
              value="hasPage"
              checked={filter.hasPage}
              onChange={(e) => {
                setFilter({ ...filter, hasPage: e.target.checked ? true : undefined });
              }}
            />
            <Checkbox
              name="included"
              label="Included"
              value="included"
              checked={filter.actions?.find((a) => a === 'included') !== undefined}
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="ticker"
              label="On Ticker"
              value="ticker"
              checked={filter.actions?.find((a) => a === 'ticker') !== undefined}
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="commentary"
              label="Commentary"
              value="commentary"
              checked={filter.actions?.find((a) => a === 'commentary') !== undefined}
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="topStory"
              label="Top Story"
              value="topStory"
              checked={filter.actions?.find((a) => a === 'topStory') !== undefined}
              onChange={handleActionFilterChange}
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
              value={logicalOperator}
              onChange={(newValue) => {
                setLogicalOperator(newValue as OptionItem);
              }}
            />
            <Text
              name="searchTerm"
              label="Search Terms"
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
              }}
            ></Text>
          </div>
          <div className="dateRange">
            <label>Date Range</label>
            <div>
              <SelectDate
                name="startDate"
                selected={startDate}
                showTimeSelect
                dateFormat="Pp"
                onChange={(date) => setStartDate(date)}
              />
              <SelectDate
                name="endDate"
                selected={endDate}
                showTimeSelect
                dateFormat="Pp"
                onChange={(date) => setEndDate(date)}
              />
            </div>
          </div>
          <Button
            name="search"
            onClick={() => {
              setFilter({
                ...filter,
                [fieldType.value]: searchTerm.trim() === '' ? undefined : searchTerm,
                createdStartOn: !!startDate ? moment(startDate).toISOString() : undefined,
                createdEndOn: !!endDate ? moment(endDate).toISOString() : undefined,
                logicalOperator:
                  searchTerm.trim() === '' ? undefined : (logicalOperator.value as LogicalOperator),
              });
            }}
          >
            Search
          </Button>
          <Button
            name="clear"
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFilter({ ...defaultFilter, ownerId: currentUserId });
              setTimeFrame(timeFrames[0]);
              setFieldType(fieldTypes[0]);
              setLogicalOperator(logicalOperators[0]);
              setSearchTerm('');
              setStartDate(null);
              setEndDate(null);
            }}
          >
            Clear
          </Button>
        </div>
      </div>
      <div className="content-list">
        <PagedTable
          columns={columns}
          onFetch={(pageIndex, pageSize) => fetch(pageIndex, pageSize, filter)}
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
