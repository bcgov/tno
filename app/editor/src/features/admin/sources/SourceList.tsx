import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSources } from 'store/hooks/admin';
import {
  CellCheckbox,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  ISourceModel,
  Link,
  Row,
} from 'tno-core';

import { sortData } from '../ingests/utils/sortData';
import { SourceFilter } from './SourceFilter';
import * as styled from './styled';

interface ISourceListProps {}

/**
 * Creates a new array and sorts based on the predicate.
 * @param items Array of items.
 * @param predicate Predicate function or property name(s) that will be used for sorting.
 * @returns A new array sorted.
 */
const sortArray = <T,>(items: T[], predicate: keyof T | (keyof T)[] | ((item: T) => any)): T[] => {
  return [...items].sort((a, b) => {
    let valueA: any;
    let valueB: any;
    if (typeof predicate === 'function') {
      valueA = predicate(a);
      valueB = predicate(b);
    } else if (Array.isArray(predicate)) {
      var sort: number = 0;
      for (let i = 0; i < predicate.length; i++) {
        valueA = a[predicate[i]];
        valueB = b[predicate[i]];
        sort = valueA < valueB ? -1 : valueA > valueB ? 1 : 0;
        if (sort !== 0) break;
      }
      return sort;
    } else {
      valueA = a[predicate];
      valueB = b[predicate];
    }
    return valueA < valueB ? -1 : valueA > valueB ? 1 : 0;
  });
};

const SourceList: React.FC<ISourceListProps> = (props) => {
  const navigate = useNavigate();
  const [{ sources }, api] = useSources();
  const [items, setItems] = React.useState<ISourceModel[]>([]);

  React.useEffect(() => {
    if (sources.length) {
      setItems(sortArray(sources, ['sortOrder', 'name', 'code']));
    } else {
      api.findAllSources().then((data) => {
        setItems(sortArray(data, ['sortOrder', 'name', 'code']));
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleOnSorting = (column: any, direction: any) => {
    if (direction && column.name) {
      setItems((items) => {
        switch (column.name) {
          case 'mediaType':
            return [...items].sort((a: any, b: any) =>
              sortData(
                a['mediaType'] ? a['mediaType']['name'] : '',
                b['mediaType'] ? b['mediaType']['name'] : '',
                direction,
              ),
            );
          case 'configuration.isDailyPaper':
            return [...items].sort((a: any, b: any) =>
              sortData(
                (a['configuration']
                  ? a['configuration']['isDailyPaper'] ?? false
                  : false) as boolean,
                (b['configuration']
                  ? b['configuration']['isDailyPaper'] ?? false
                  : false) as boolean,
                direction,
              ),
            );
          case 'useInTopics':
            return [...items].sort((a: any, b: any) =>
              sortData(a[column.name!] as boolean, b[column.name!] as boolean, direction),
            );
          case 'isEnabled':
            return [...items].sort((a: any, b: any) =>
              sortData(a[column.name!] as boolean, b[column.name!] as boolean, direction),
            );
          default:
            return [...items].sort((a: any, b: any) =>
              sortData(a[column.name!] ?? '', b[column.name!] ?? '', direction),
            );
        }
      });
    } else {
      setItems(sources);
    }
  };

  return (
    <styled.SourceList>
      <Row justifyContent="flex-end">
        <Col flex="1 1 0">
          Sources provide a way to identify the source of the content. Generally this would be the
          publisher, or channel.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Source"
          onClick={() => navigate('/admin/sources/0')}
        />
      </Row>
      <SourceFilter
        onFilterChange={(filter) => {
          if (filter && filter.length) {
            const value = filter.toLocaleLowerCase();
            setItems(
              sources.filter(
                (i) =>
                  i.name.toLocaleLowerCase().includes(value) ||
                  i.description.toLocaleLowerCase().includes(value) ||
                  i.code.toLocaleLowerCase().includes(value),
              ),
            );
          } else {
            setItems(sources);
          }
        }}
      />
      <Grid
        items={items}
        showPaging={false}
        onSortChange={(column, direction) => handleOnSorting(column, direction)}
        renderHeader={() => [
          { name: 'name', label: 'Name', sortable: true },
          { name: 'code', label: 'Code', size: '15%', sortable: true },
          { name: 'mediaType', label: 'Media Type', size: '15%', sortable: true },
          { name: 'sortOrder', label: 'Order', size: '10%', sortable: true },
          { name: 'useInTopics', label: 'Topics', size: '10%', sortable: true },
          { name: 'configuration.isDailyPaper', label: 'Paper', size: '10%', sortable: true },
          { name: 'isEnabled', label: 'Enabled', size: '10%', sortable: true },
        ]}
        renderColumns={(row: ISourceModel, rowIndex) => {
          return [
            {
              column: (
                <div key="1">
                  <Link to={`${row.id}`}>{row.name}</Link>
                </div>
              ),
            },
            {
              column: (
                <div key="2" className="clickable">
                  <CellEllipsis key="">{row.code}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="3" className="clickable">
                  <CellEllipsis key="">{row.mediaType?.name}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="4" className="clickable">
                  <CellEllipsis key="">{row.sortOrder}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="5" className="clickable">
                  <CellCheckbox key="" checked={row.useInTopics} />
                </div>
              ),
            },
            {
              column: (
                <div key="6" className="clickable">
                  <CellCheckbox key="" checked={row.configuration.isDailyPaper} />
                </div>
              ),
            },
            {
              column: (
                <div key="7" className="clickable">
                  <CellCheckbox key="" checked={row.isEnabled} />
                </div>
              ),
            },
          ];
        }}
      />
    </styled.SourceList>
  );
};

export default SourceList;
