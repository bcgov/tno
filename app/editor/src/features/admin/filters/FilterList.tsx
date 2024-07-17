import { FormPage } from 'components/formpage';
import React from 'react';
import { FaRegClipboard } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useFilters } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  CellCheckbox,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  IFilterModel,
  Link,
  Row,
  SortDirection,
} from 'tno-core';

import { ListFilter } from './ListFilter';
import * as styled from './styled';
import { handleCopyKeyWords } from './utils/handleCopyKeyWords';
import { truncateString } from './utils/truncateString';

export const FilterList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, filters }, { findFilters }] = useFilters();
  const [{ filterFilter }] = useAdminStore();

  const [items, setItems] = React.useState<IFilterModel[]>(filters);
  const [sort, setSort] = React.useState('name');

  React.useEffect(() => {
    if (!initialized) {
      findFilters({ sort: [sort] }).catch(() => {});
    }
    // The api will cause a double render because findAllFilters(...) updates the store.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

  React.useEffect(() => {
    if (filterFilter && filterFilter.length) {
      const value = filterFilter.toLocaleLowerCase();
      setItems(
        filters.filter(
          (i) =>
            i.name.toLocaleLowerCase().includes(value) ||
            i.description.toLocaleLowerCase().includes(value) ||
            i.owner?.username.toLocaleLowerCase().includes(value) ||
            i.owner?.displayName.toLocaleLowerCase().includes(value) ||
            i.owner?.firstName.toLocaleLowerCase().includes(value) ||
            i.owner?.lastName.toLocaleLowerCase().includes(value),
        ),
      );
    } else {
      setItems(filters);
    }
  }, [filters, filterFilter]);

  React.useEffect(() => {
    setItems((items) => {
      if (!sort) return items;
      const parts = sort.split(' ');
      const desc = parts.length === 2 ? parts[1] === SortDirection.Descending : false;
      return [...items].sort((a, b) => {
        if (!sort) return 0;
        const aValue = (a as any)[parts[0]];
        const bValue = (b as any)[parts[0]];
        if (aValue < bValue) return desc ? 1 : -1;
        if (aValue > bValue) return desc ? -1 : 1;
        return 0;
      });
    });
  }, [sort]);

  return (
    <styled.FilterList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">A filter provides a way to save an Elasticsearch query.</Col>
          <IconButton
            iconType="plus"
            label={`Add new filter`}
            onClick={() => navigate(`/admin/filters/0`)}
          />
        </Row>
        <ListFilter onFilterChange={(filter) => {}} />
        <Grid
          items={items}
          onSortChange={async (column, direction) => {
            if (direction === SortDirection.None) setSort('name');
            else setSort(`${column.name} ${direction}`);
          }}
          renderHeader={() => [
            { name: 'name', label: 'Name', sortable: true },
            { name: 'description', label: 'Description', sortable: true },
            { name: 'ownerId', label: 'Owner', sortable: true },
            { name: 'keywords', label: 'Keywords', sortable: false },
            { name: 'isEnabled', label: 'Enabled', size: '120px', sortable: true },
          ]}
          renderColumns={(row: IFilterModel, rowIndex) => [
            <CellEllipsis key="1">
              <Link to={`/admin/filters/${row.id}`}>{row.name}</Link>
            </CellEllipsis>,
            <CellEllipsis key="2">{row.description}</CellEllipsis>,
            <CellEllipsis key="3">{row.owner?.username}</CellEllipsis>,
            <div key="4" className="keyword-cell">
              <CellEllipsis>{truncateString(row.settings?.search)}</CellEllipsis>
              {row.settings?.search ? (
                <FaRegClipboard
                  className="clipboard-icon"
                  title="Copy keywords to clipboard"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleCopyKeyWords(e, row.settings.search);
                  }}
                />
              ) : null}
            </div>,
            <CellCheckbox key="5" checked={row.isEnabled} />,
          ]}
        />
      </FormPage>
    </styled.FilterList>
  );
};

export default FilterList;
