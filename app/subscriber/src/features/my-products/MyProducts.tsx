import React from 'react';
import { toast } from 'react-toastify';
import { useProducts } from 'store/hooks';
import { useAppStore } from 'store/slices';
// import { Col, FlexboxTable, IProductSubscriberModel, Row } from 'tno-core';
import { Col, FlexboxTable, IProductSubscriberModel, Modal, Row, useModal } from 'tno-core';

import { useColumns } from './hooks';
import * as styled from './styled';

export const MyProducts: React.FC = () => {
  const [{ getProducts, toggleSubscription }] = useProducts();
  const { toggle, isShowing } = useModal();
  const [{ requests }] = useAppStore();

  const [products, setProducts] = React.useState<IProductSubscriberModel[]>([]);
  const [product, setProduct] = React.useState<IProductSubscriberModel>();

  React.useEffect(() => {
    getProducts().then((data) => {
      setProducts(data);
    });
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleToggleSubscription = React.useCallback(
    (product: IProductSubscriberModel) => {
      if (!!product) {
        toggleSubscription(product)
          .then((data) => {
            toast.success(`Successfully change subscription status for product '${data.name}'.`);
            // TODO: Why doesnt the checkbox value get updated?
            setProducts(
              products.map((ds) => {
                if (ds.id === data.id) return data;
                return ds;
              }),
            );
          })
          .catch(() => {});
      }
    },
    [toggleSubscription, products],
  );

  const columns = useColumns(async (product) => {
    try {
      setProduct(product);
      toggle();
    } catch {}
  });

  return (
    <styled.MyProducts>
      <Col className="my-reports">
        <Col className="info">
          <Row>Media Monitoring products</Row>
          <Row>
            The following products are available for subscription. Subscribed products delivered by
            email.
          </Row>
        </Col>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns}
          rowId={'id'}
          data={products}
          showActive={false}
          isLoading={requests.some((r) => r.url.includes('get-products'))}
        />
      </Col>
      <Modal
        headerText="Confirm Subscription Status Change"
        body={`Are you sure you wish to ${
          product?.isSubscribed ? 'Unsubscribe' : 'Subscribe'
        } to the "${product?.name}" report?`}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText={`Yes, ${product?.isSubscribed ? 'Unsubscribe' : 'Subscribe'}`}
        onConfirm={() => {
          if (product) handleToggleSubscription(product);
          toggle();
        }}
      />
    </styled.MyProducts>
  );
};
