import { Button, ButtonVariant } from 'components/button';
import {
  Checkbox,
  Dropdown,
  IOptionItem,
  OptionItem,
  RadioGroup,
  SelectDate,
  Text,
} from 'components/form';
import { IPage, Page, PagedTable } from 'components/grid-table';
import { IContentModel, LogicalOperator } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { SortingRule } from 'react-table';
import { useContent, useLookup } from 'store/hooks';
import { initialContentState } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';
import { getSortableOptions, getUserOptions } from 'utils';

import { columns, fieldTypes, logicalOperators, timeFrames } from './constants';
import * as styled from './ContentListViewStyled';
import { IContentListFilter, ISortBy } from './interfaces';
import { makeFilter } from './makeFilter';

const defaultPage: IPage<IContentModel> = {
  pageIndex: 0,
  pageSize: 10,
  pageCount: -1,
  items: [],
};

export const ContentListView: React.FC = () => {
  const [{ contentTypes, mediaTypes, users }] = useLookup();
  const [
    { filter, filterAdvanced, sortBy },
    { findContent },
    { storeFilter, storeFilterAdvanced, storeSortBy },
  ] = useContent();

  const [mediaTypeOptions, setMediaTypes] = React.useState<IOptionItem[]>([]);
  const [contentTypeOptions, setContentTypes] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUsers] = React.useState<IOptionItem[]>([]);
  const [page, setPage] = React.useState(defaultPage);
  const keycloak = useKeycloakWrapper();
  const [username, setUsername] = React.useState(keycloak.instance.tokenParsed.username);
  const navigate = useNavigate();

  const currentUsername = keycloak.instance.tokenParsed.username;
  const printContentId = (contentTypeOptions.find((ct) => ct.label === 'Print')?.value ??
    0) as number;

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
    setContentTypes(getSortableOptions(contentTypes));
    setMediaTypes(getSortableOptions(mediaTypes, [new OptionItem<number>('All Media', 0)]));
    setUsers(getUserOptions(users, [new OptionItem<number>('All Users', 0)]));
  }, [contentTypes, mediaTypes, users]);

  const fetch = React.useCallback(
    async (filter: IContentListFilter, sortBy: ISortBy[]) => {
      try {
        const data = await findContent(
          makeFilter({
            ...filter,
            ...filterAdvanced,
            sortBy,
          }),
        );
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);
        setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [filterAdvanced, findContent],
  );

  React.useEffect(() => {
    fetch(filter, sortBy);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, sortBy]);

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
            direction="row"
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
          <div style={{ display: 'flex', flexDirection: 'row', minWidth: '550px' }}>
            <Dropdown
              name="fieldType"
              className="test"
              label="Field Type"
              options={fieldTypes}
              value={filterAdvanced.fieldType}
              onChange={(newValue) => {
                storeFilterAdvanced({ ...filterAdvanced, fieldType: newValue as IOptionItem });
              }}
            />
            <Dropdown
              className="test"
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
              className="test"
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
            onClick={() => fetch({ ...filter, pageIndex: 0, ...filterAdvanced }, sortBy)}
          >
            Search
          </Button>
          <Button
            name="clear"
            variant={ButtonVariant.action}
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
          onRowClick={(row) => navigate(`/contents/${row.values.createdOn.id}`)}
          onChangePage={handleChangePage}
          onChangeSort={handleChangeSort}
        ></PagedTable>
      </div>
      <div className="content-actions">
        <Button name="create" onClick={() => navigate('/contents/0')}>
          Create Snippet
        </Button>
        <div style={{ marginTop: '2%' }} className="addition-actions">
          <Button
            name="create"
            variant={ButtonVariant.action}
            disabled
            tooltip="Under Construction"
          >
            Send Lois Front Pages
          </Button>
          <Button
            name="create"
            variant={ButtonVariant.action}
            disabled
            tooltip="Under Construction"
          >
            Send Top Stories
          </Button>
          <Button
            name="create"
            variant={ButtonVariant.action}
            disabled
            tooltip="Under Construction"
          >
            Send Send Lois to Commentary
          </Button>
        </div>
      </div>
    </styled.ContentListView>
  );
};
