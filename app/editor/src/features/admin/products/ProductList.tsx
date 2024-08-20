import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useProducts } from 'store/hooks/admin';
import {
  CellCheckbox,
  Col,
  Grid,
  IconButton,
  IProductModel,
  Link,
  ProductRequestStatusName,
  Row,
  SortDirection,
} from 'tno-core';

import { ProductFilter } from './ProductFilter';
import * as styled from './styled';

const ProductList: React.FC = () => {
  const navigate = useNavigate();
  const [{ products }, { findProducts }] = useProducts();

  const [items, setItems] = React.useState<IProductModel[]>([]);
  const [isReady, setIsReady] = React.useState(false);

  React.useEffect(() => {
    if (!products.length && !isReady) {
      setIsReady(true);
      findProducts({})
        .then((data) => {
          setItems(data);
        })
        .catch(() => {});
    } else {
      setItems(products);
    }
  }, [findProducts, isReady, products]);

  return (
    <styled.ProductList>
      <FormPage>
        <Row className="add-product" justifyContent="flex-end">
          <Col flex="1 1 0">
            Products provide a way manage a list of products users can request to subscribe to.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new product`}
            onClick={() => navigate(`/admin/products/0`)}
          />
        </Row>
        <ProductFilter
          onChange={(filter) => {
            if (filter && filter.keyword?.length) {
              const value = filter.keyword.toLocaleLowerCase();
              setItems(
                products.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(products);
            }
          }}
          onSearch={(filter) => {
            if (filter && filter.keyword?.length) {
              const value = filter.keyword.toLocaleLowerCase();
              setItems(
                products.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(products);
            }
          }}
          filter={{ keyword: '' }}
        />
        <Grid
          items={items}
          showPaging={false}
          onSortChange={async (column, direction) => {
            if (direction && column.name) {
              if (column.name === 'status') {
                setItems((items) => {
                  return [...items].sort((a, b) => {
                    const aValue = a.subscribers.some(
                      (s) => s.status !== ProductRequestStatusName.NA,
                    );
                    const bValue = b.subscribers.some(
                      (s) => s.status !== ProductRequestStatusName.NA,
                    );
                    if (aValue > bValue) return direction === SortDirection.Ascending ? -1 : 1;
                    if (aValue < bValue) return direction === SortDirection.Ascending ? 1 : -1;
                    return 0;
                  });
                });
              } else {
                setItems((items) => {
                  return [...items].sort((a: any, b: any) => {
                    const aValue = a[column.name!];
                    const bValue = b[column.name!];
                    if (aValue < bValue) return direction === SortDirection.Ascending ? -1 : 1;
                    if (aValue > bValue) return direction === SortDirection.Ascending ? 1 : -1;
                    return 0;
                  });
                });
              }
            } else {
              setItems(products);
            }
          }}
          renderHeader={() => [
            { name: 'name', label: 'Name', sortable: true },
            { name: 'status', label: 'Has Requests', size: '160px', sortable: true },
            { name: 'description', label: 'Description', sortable: true },
            { name: 'productType', label: 'Type', size: '150px', sortable: true },
            { name: 'sortOrder', label: 'Order', size: '100px', sortable: true },
            { name: 'isPublic', label: 'Public', size: '100px', sortable: true },
            { name: 'isEnabled', label: 'Enabled', size: '110px', sortable: true },
          ]}
          renderColumns={(row: IProductModel, rowIndex) => {
            return [
              <div key="1">
                <Link to={`${row.id}`}>{row.name}</Link>
              </div>,
              <div key="2">
                {row.subscribers.some((s) => s.status !== ProductRequestStatusName.NA)
                  ? 'Yes'
                  : 'No'}
              </div>,
              <div key="3">{row.description}</div>,
              <div key="4">{row.productType}</div>,
              <div key="5">{row.sortOrder}</div>,
              <div key="6">
                <CellCheckbox checked={row.isPublic} />
              </div>,
              <div key="7">
                <CellCheckbox checked={row.isEnabled} />
              </div>,
            ];
          }}
        />
      </FormPage>
    </styled.ProductList>
  );
};

export default ProductList;
