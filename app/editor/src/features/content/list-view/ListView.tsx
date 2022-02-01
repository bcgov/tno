import {
  Button,
  ButtonVariant,
  CheckboxGroup,
  Dropdown,
  GridTable,
  RadioGroup,
  Text,
} from 'components';
import { mockContents } from 'hooks/api-editor/mocks';

import { columns, timeFrames } from './constants';
import * as styled from './ListViewStyled';

export const ListView: React.FC = () => {
  const mediaTypes = ['All Media', 'Syndication'];
  const users = ['user 1', 'user 2'];
  const filters = ['Lois', 'Included', 'Ticker', 'Commentary', 'Top Stories'];
  const fieldTypes = ['Headline'];
  const logicalOperators = ['Contains'];
  const data = mockContents;

  return (
    <styled.ListView>
      <div className="filter">
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
      <div className="list-view">
        <GridTable columns={columns} data={data}></GridTable>
      </div>
    </styled.ListView>
  );
};
