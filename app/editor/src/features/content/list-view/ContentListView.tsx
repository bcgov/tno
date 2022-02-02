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
  Text,
} from 'components';
import { IContentFilter, useApiEditor } from 'hooks';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';

import { columns, fieldTypes, logicalOperators, timeFrames } from './constants';
import * as styled from './ContentListViewStyled';

export const ContentListView: React.FC = () => {
  const [mediaTypes, setMediaTypes] = React.useState<IOptionItem[]>([]);
  const [users, setUsers] = React.useState<IOptionItem[]>([]);
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState<number | undefined>(10);
  const [filter, setFilter] = React.useState<IContentFilter>({});
  const navigate = useNavigate();
  const api = useApiEditor();

  React.useEffect(() => {
    api.getUsers().then((data) => {
      setUsers(data.map((u) => new OptionItem(u.displayName, u.id)));
    });
  }, [api]);

  React.useEffect(() => {
    api.getMediaTypes().then((data) => {
      setMediaTypes(
        [new OptionItem('All Media', 0, true)].concat(
          data.map((m) => new OptionItem(m.name, m.id)),
        ),
      );
    });
  }, [api]);

  const fetch = React.useCallback(
    async (pageIndex: number, pageSize?: number, filter?: IContentFilter) => {
      try {
        const data = await api.getContents(pageIndex, pageSize, filter);
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
    console.debug(e.target.value);
    const value = +e.target.value;
    let createdStartOn: Date | undefined;
    if (value === 0) createdStartOn = moment().startOf('day').toDate();
    else if (value === 1) createdStartOn = moment().add(-24, 'hours').toDate();
    else if (value === 2) createdStartOn = moment().add(-48, 'hours').toDate();
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
            onChange={(newValue) => {
              var mediaTypeId = (newValue as IOptionItem).value as number;
              setFilter({ ...filter, mediaTypeId: mediaTypeId > 0 ? mediaTypeId : undefined });
            }}
          />
          <Dropdown
            name="user"
            label="User"
            options={users}
            onChange={(newValue) => {
              var ownerId = (newValue as IOptionItem).value as number;
              setFilter({ ...filter, ownerId: ownerId > 0 ? ownerId : undefined });
            }}
          />
          <RadioGroup
            name="timeFrame"
            label="Time Frame"
            options={timeFrames}
            onChange={handleTimeChange}
          />
          <div className="frm-in">
            <label>Filters</label>
            <Checkbox
              name="newspaper"
              label="Lois"
              value="hasPage"
              onChange={(e) => {
                setFilter({ ...filter, hasPage: e.target.checked ? true : undefined });
              }}
            />
            <Checkbox
              name="included"
              label="Included"
              value="included"
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="ticker"
              label="On Ticker"
              value="ticker"
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="commentary"
              label="Commentary"
              value="commentary"
              onChange={handleActionFilterChange}
            />
            <Checkbox
              name="topStory"
              label="Top Story"
              value="topStory"
              onChange={handleActionFilterChange}
            />
          </div>
        </div>
        <div className="box">
          <h2 className="caps">Advanced Search</h2>
          <div>
            <Dropdown name="fieldType" label="Field Type" options={fieldTypes} />
            <Dropdown name="logicalOperator" label="Logical Operator" options={logicalOperators} />
            <Text name="searchTerm" label="Search Terms"></Text>
          </div>
          <Text name="dateRange" label="Date Range"></Text>
          <Button name="search">Search</Button>
          <Button name="clear" variant={ButtonVariant.secondary}>
            Clear
          </Button>
        </div>
      </div>
      <div className="content-list">
        <PagedTable
          columns={columns}
          fetchData={(pageIndex, pageSize) => fetch(pageIndex, pageSize, filter)}
          onRowClick={(row) => navigate(`/contents/${row.id}`)}
          onPageChange={handlePageChange}
        />
      </div>
      <div className="content-actions">
        <Button name="create" onClick={() => navigate('/contents/0')}>
          Create Snippet
        </Button>
        <Button name="create" variant={ButtonVariant.secondary}>
          Send Lois Front Pages
        </Button>
        <Button name="create" variant={ButtonVariant.secondary}>
          Send Top Stories
        </Button>
        <Button name="create" variant={ButtonVariant.secondary}>
          Send Send Lois to Commentary
        </Button>
      </div>
    </styled.ContentListView>
  );
};
