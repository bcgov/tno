import {
  Button,
  ButtonVariant,
  Checkbox,
  Dropdown,
  IOptionItem,
  IPage,
  OptionItem,
  Page,
  PagedTable,
  RadioGroup,
  SelectDate,
  Text,
} from 'components';
import { IContentModel, LogicalOperator, useApiEditor } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { initialContentState, useContentStore, useLookup } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';

import { columns, fieldTypes, logicalOperators, timeFrames } from './constants';
import * as styled from './ContentListViewStyled';
import { IContentListAdvancedFilter, IContentListFilter } from './interfaces';
import { IFilter, makeFilter } from './makeFilter';

const defaultPage: IPage<IContentModel> = {
  pageIndex: 0,
  pageSize: 10,
  pageCount: -1,
  items: [],
};

const defaultListFilter: IContentListFilter = {
  pageIndex: 0,
  pageSize: 10,
  mediaTypeId: 0,
  ownerId: '',
  userId: '',
  timeFrame: timeFrames[0],
  isPrintContent: false,
  included: false,
  onTicker: false,
  commentary: false,
  topStory: false,
};

const defaultListAdvancedFilter: IContentListAdvancedFilter = {
  fieldType: fieldTypes[0],
  logicalOperator: LogicalOperator.Contains,
  searchTerm: '',
};

const defaultPage: IPage<IContentModel> = {
  pageIndex: 0,
  pageSize: 10,
  pageCount: -1,
  items: [],
};

export const ContentListView: React.FC = () => {
  const {
    storeContentTypes,
    storeMediaTypes,
    storeUsers,
    state: { contentTypes, mediaTypes, users },
  } = useLookup();
  const {
    storeFilter,
    storeFilterAdvanced,
    storeSortBy,
    state: { filter, filterAdvanced, sortBy },
  } = useContentStore();

  const [mediaTypeOptions, setMediaTypes] = React.useState<IOptionItem[]>([]);
  const [contentTypeOptions, setContentTypes] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUsers] = React.useState<IOptionItem[]>([]);
  const [page, setPage] = React.useState(defaultPage);
  const keycloak = useKeycloakWrapper();
  const [username, setUsername] = React.useState(keycloak.instance.tokenParsed.username);
  const navigate = useNavigate();
  const api = useApiEditor();

  const currentUsername = keycloak.instance.tokenParsed.username;
  const printContentId = (contentTypeOptions.find((ct) => ct.label === 'Print')?.value ??
    0) as number;

  React.useEffect(() => {
    if (users.length === 0) {
      api.getUsers().then((data) => {
        storeUsers(data);
      });
    }

    if (contentTypes.length === 0) {
      api.getContentTypes().then((data) => {
        storeContentTypes(data);
      });
    }

    if (mediaTypes.length === 0) {
      api.getMediaTypes().then((data) => {
        storeMediaTypes(data);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [api]);

  React.useEffect(() => {
    setUsername(currentUsername);
  }, [currentUsername]);

  React.useEffect(() => {
    // TODO: switch to user.id when keycloak is setup.
    const currentUserId =
      filter.userId === '' || filter.userId === 0
        ? users.find((u) => u.username === username)?.id ?? 0
        : filter.userId;
    storeFilter((filter: IContentListFilter) => ({ ...filter, userId: currentUserId }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [username, users]);

  React.useEffect(() => {
    setContentTypes(contentTypes.map((m) => new OptionItem(m.name, m.id)));
    setMediaTypes(
      [new OptionItem('All Media', 0)].concat(mediaTypes.map((m) => new OptionItem(m.name, m.id))),
    );
    setUsers(
      [new OptionItem('All Users', 0)].concat(
        users.map((u) => new OptionItem(u.displayName, u.id)),
      ),
    );
  }, [contentTypes, mediaTypes, users]);

  const fetch = React.useCallback(
    async (filter) => {
      try {
        const data = await api.getContents(filter.pageIndex, filter.pageSize, makeFilter(filter));
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);
        setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [api],
  );

  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced, sortBy });
    // We don't want a render when the advanced filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fetch, filter, sortBy]);

  const handleChangePage = React.useCallback(
    (pi: number, ps?: number) => {
      if (filter.pageIndex !== pi) storeFilter({ ...filter, pageIndex: pi });
      if (filter.pageSize !== ps)
        storeFilter({ ...filter, pageSize: ps ?? initialContentState.filter.pageSize });
    },
    [filter, storeFilter],
  );

  const handleChangeSort = React.useCallback(
    (sortBy: SortingRule<IContentModel>[]) => {
      storeSortBy(sortBy.map((sb) => ({ id: sb.id, desc: sb.desc })));
    },
    [storeSortBy],
  );

  const handleTimeChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = +e.target.value;
    const timeFrame = timeFrames.find((tf) => tf.value === value);
    storeFilter((filter: IContentListFilter) => ({
      ...filter,
      pageIndex: 0,
      timeFrame: timeFrame?.toInterface() ?? timeFrames[0].toInterface(),
    }));
  };

  return (
    <styled.ContentListView>
      <div className="content-filter">
        <div>
          <Dropdown
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
          <Dropdown
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
            tooltip="Date created"
            value={filter.timeFrame}
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
                  label="Lois"
                  tooltip="Print Content"
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
          <div>
            <Dropdown
              name="fieldType"
              label="Field Type"
              options={fieldTypes}
              value={filterAdvanced.fieldType}
              onChange={(newValue) => {
                storeFilterAdvanced({ ...filterAdvanced, fieldType: newValue as IOptionItem });
              }}
            />
            <Dropdown
              name="logicalOperator"
              label="Logical Operator"
              options={logicalOperators}
              value={logicalOperators.find(
                (lo) => (LogicalOperator as any)[lo.value] === filterAdvanced.logicalOperator,
              )}
              onChange={(newValue) => {
                const logicalOperator = (LogicalOperator as any)[
                  (newValue as IOptionItem).value ?? 0
                ];
                storeFilterAdvanced({ ...filterAdvanced, logicalOperator });
              }}
            />
            <Text
              name="searchTerm"
              label="Search Terms"
              value={filterAdvanced.searchTerm}
              onChange={(e) => {
                storeFilterAdvanced({ ...filterAdvanced, searchTerm: e.target.value.trim() });
              }}
            ></Text>
          </div>
          <div className="frm-in dateRange">
            <label data-for="main-tooltip" data-tip="Date created">
              Date Range
            </label>
            <div>
              <SelectDate
                name="startDate"
                placeholderText="YYYY MM DD"
                selected={
                  !!filterAdvanced.startDate ? new Date(filterAdvanced.startDate) : undefined
                }
                showTimeSelect
                dateFormat="Pp"
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
                showTimeSelect
                dateFormat="Pp"
                onChange={(date) =>
                  storeFilterAdvanced({
                    ...filterAdvanced,
                    endDate: !!date ? date.toString() : undefined,
                  })
                }
              />
            </div>
          </div>
          <Button
            name="search"
            onClick={() => fetch({ ...filter, pageIndex: 0, ...filterAdvanced })}
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
      </div>
      <div className="content-list">
        <PagedTable
          columns={columns}
          page={page}
          sortBy={sortBy}
          onRowClick={(row) => navigate(`/contents/${row.id}`)}
          onChangePage={handleChangePage}
          onChangeSort={handleChangeSort}
        ></PagedTable>
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
