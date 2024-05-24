import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useFilters } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Col, FlexboxTable, IconButton, IFilterModel, Row } from 'tno-core';

import { filterColumns } from './constants';
import { ListFilter } from './ListFilter';
import * as styled from './styled';

export const FilterList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, filters }, { findAllFilters }] = useFilters();
  const [{ filterFilter }] = useAdminStore();

  const [items, setItems] = React.useState<IFilterModel[]>(filters);

  React.useEffect(() => {
    if (!initialized) {
      findAllFilters().catch(() => {});
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
        <FlexboxTable
          rowId="id"
          data={items}
          columns={filterColumns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.FilterList>
  );
};

export default FilterList;
