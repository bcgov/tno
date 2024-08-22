import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSources } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ISourceModel, Row } from 'tno-core';

import { columns } from './constants';
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
  const [isReady, setIsReady] = React.useState(false);
  const [items, setItems] = React.useState<ISourceModel[]>([]);

  React.useEffect(() => {
    setIsReady(true);
    if (sources.length) {
      setItems(sortArray(sources, ['sortOrder', 'name', 'code']));
      setIsReady(false);
    } else {
      api.findAllSources().then((data) => {
        setItems(sortArray(data, ['sortOrder', 'name', 'code']));
      });
      setIsReady(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
      <FlexboxTable
        rowId="id"
        data={items}
        columns={columns}
        showSort={true}
        pagingEnabled={false}
      />
    </styled.SourceList>
  );
};

export default SourceList;
