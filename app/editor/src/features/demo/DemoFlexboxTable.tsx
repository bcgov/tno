import React from 'react';
import { Button, FlexboxTable, ITableHookColumn, Row, Text } from 'tno-core';

export class DemoData {
  id: number;
  name: string;
  value: any;
  section?: string;
  code: string;
  cost: number;

  constructor(id: number, name: string, section: string | undefined = undefined) {
    this.id = id;
    this.name = name;
    this.value = undefined;
    this.section = section;
    this.code = String.fromCharCode(id + 50);
    this.cost = 0.0;
  }
}

const columns: ITableHookColumn<DemoData>[] = [
  {
    name: 'id',
    label: 'Id',
    showSort: true,
  },
  {
    name: 'name',
    label: 'Name',
    cell: (cell) => cell.original.name,
    showSort: true,
  },
  {
    name: 'value',
    label: 'Value',
    showSort: true,
  },
  {
    name: 'section',
    label: 'Section',
    showSort: true,
  },
  {
    name: 'cost',
    label: 'Cost',
    showSort: true,
    hAlign: 'right',
  },
];

const generate = (count: number) => {
  const items = [];
  var section = 1;
  for (var i = 0; i < count; i++) {
    const value = new DemoData(i, `name-${i}`, `${section}`);
    value.value = i % 2 ? 'even' : 'odd';
    items.push(value);
    section = (i + 1) % 10 === 0 ? section + 1 : section;
  }
  return items;
};

export const DemoFlexboxTable = () => {
  const [data, setData] = React.useState<DemoData[]>(generate(153));
  const [item, setItem] = React.useState<DemoData>();
  const [showPaging, setShowPaging] = React.useState(true);
  const [pageIndex, setPageIndex] = React.useState(0);
  const [filter, setFilter] = React.useState('');

  return (
    <div style={{ width: '80%' }}>
      <form>
        <Text
          name="name"
          label="Name"
          value={item?.name ?? ''}
          onChange={(e) => {
            if (!!item) setItem({ ...item, name: e.target.value });
          }}
        />
        <Text
          name="value"
          label="Value"
          value={item?.value ?? ''}
          onChange={(e) => {
            if (!!item) setItem({ ...item, value: e.target.value });
          }}
        />
        <Button
          onClick={() => {
            setData(data.map((row) => (row.id === item?.id ? item : row)));
          }}
        >
          Save
        </Button>
        <Button
          onClick={() => {
            setShowPaging(!showPaging);
          }}
        >
          Toggle Paging
        </Button>
        <Row>
          <Text
            name="pageIndex"
            label="Page Index"
            value={pageIndex}
            width="4em"
            onChange={(e) => {
              setPageIndex(Number(e.currentTarget.value));
            }}
          />
          <Text
            name="filter"
            label="Search"
            value={filter}
            onChange={(e) => {
              setFilter(e.currentTarget.value);
            }}
          />
        </Row>
      </form>
      <FlexboxTable
        rowId="id"
        columns={columns}
        data={data}
        showSort={true}
        pageIndex={pageIndex}
        pageButtons={5}
        showPaging={showPaging}
        // pageSize={1}
        isMulti={true}
        // manualPaging={true}
        search={filter}
        // showFilter={true}
        // scrollSize="200px"
        // showHeader={false}
        // groupBy={(item) => item.original.section ?? 'any'}
        groupBy="section"
        onRowClick={(row) => {
          setItem(row.original);
        }}
        onColumnClick={(col) => {}}
        onCellClick={(cell) => {}}
        onSelectedChanged={(row) => {}}
      ></FlexboxTable>
    </div>
  );
};
