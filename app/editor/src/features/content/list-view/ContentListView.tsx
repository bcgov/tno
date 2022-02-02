import {
  Button,
  ButtonVariant,
  CheckboxGroup,
  Dropdown,
  IOptionItem,
  OptionItem,
  Page,
  PagedTable,
  RadioGroup,
  Text,
} from 'components';
import { useApiEditor } from 'hooks';
import React from 'react';
import { useNavigate } from 'react-router-dom';

import { columns, timeFrames } from './constants';
import * as styled from './ContentListViewStyled';

export const ContentListView: React.FC = () => {
  const mediaTypes = ['All Media', 'Syndication'];
  const [users, setUsers] = React.useState<IOptionItem[]>([]);
  const filters = ['Lois', 'Included', 'Ticker', 'Commentary', 'Top Stories'];
  const fieldTypes = ['Headline', 'Page', 'Username', 'Status', 'Source'];
  const logicalOperators = ['Contains'];
  const navigate = useNavigate();
  const api = useApiEditor();

  React.useEffect(() => {
    api.getUsers().then((data) => {
      setUsers(data.map((u) => new OptionItem(u.displayName, u.id)));
    });
  }, []);

  const fetch = React.useCallback(
    async (pageIndex: number, pageSize?: number) => {
      try {
        const data = await api.getContents(pageIndex, pageSize);
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [api],
  );

  return (
    <styled.ContentListView>
      <div className="content-filter">
        <div>
          <Dropdown name="mediaType" label="Media Type" options={mediaTypes}></Dropdown>
          <Dropdown name="user" label="User" options={users}></Dropdown>
          <RadioGroup name="timeFrame" label="Time Frame" options={timeFrames}></RadioGroup>
          <CheckboxGroup name="filters" label="Filters" options={filters}></CheckboxGroup>
        </div>
        <div className="box">
          <h2 className="caps">Advanced Search</h2>
          <div>
            <Dropdown name="fieldType" label="Field Type" options={fieldTypes}></Dropdown>
            <Dropdown
              name="logicalOperator"
              label="Logical Operator"
              options={logicalOperators}
            ></Dropdown>
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
          fetchData={fetch}
          onRowClick={(row) => navigate(`/content/${row.id}`)}
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
