import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useProducts } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, FormPage, GridTable, IconButton, IProductModel, Row } from 'tno-core';

import { columns } from './constants';
import { ProductListFilter } from './ProductListFilter';
import * as styled from './styled';

export const ProductList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
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
        <GridTable
          columns={columns}
          header={ProductListFilter}
          paging={{ pageSizeOptions: { fromLocalStorage: true } }}
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.ProductList>
  );
};
