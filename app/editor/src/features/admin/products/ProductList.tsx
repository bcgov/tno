import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useProducts } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IProductModel, Row } from 'tno-core';

import { columns } from './constants';
import { ProductFilter } from './ProductFilter';
import * as styled from './styled';

const ProductList: React.FC = () => {
  const navigate = useNavigate();
  const [{ products }, api] = useProducts();

  const [items, setItems] = React.useState<IProductModel[]>([]);
  const [isReady, setIsReady] = React.useState(false);

  React.useEffect(() => {
    if (!products.length && !isReady) {
      setIsReady(true);
      api
        .findProducts({})
        .then((data) => {
          setItems(data);
        })
        .catch(() => {});
    } else {
      setItems(products);
    }
  }, [api, isReady, products]);

  return (
    <styled.ProductList>
      <FormPage>
        <Row className="add-product" justifyContent="flex-end">
          <Col flex="1 1 0">
            Products provide a way for subscribers to access conten via subscription.
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
          pagingEnabled={false}
        />
      </FormPage>
    </styled.ProductList>
  );
};

export default ProductList;
