import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useProducts } from 'store/hooks/admin';
import { Col, FlexboxTable, FormPage, IconButton, IProductModel, Row } from 'tno-core';

import { columns } from './constants';
import { ProductFilter } from './ProductFilter';
import * as styled from './styled';

export const ProductList: React.FC = () => {
  const navigate = useNavigate();
  const [{ products }, api] = useProducts();

  const [items, setItems] = React.useState<IProductModel[]>([]);

  React.useEffect(() => {
    if (!products.length) {
      api.findAllProducts().then((data) => {
        setItems(data);
      });
    } else {
      setItems(products);
    }
  }, [api, products]);

  return (
    <styled.ProductList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Products provide a way to designate how and where content is displayed to subscribers.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new product`}
            onClick={() => navigate(`/admin/products/0`)}
          />
        </Row>
        <ProductFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
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
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        />
      </FormPage>
    </styled.ProductList>
  );
};
