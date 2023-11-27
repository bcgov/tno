import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { FaEnvelope, FaUserPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useProducts } from 'store/hooks';
import { IProductSubscriberModel, Modal, Row, Show, useModal } from 'tno-core';

import { ProductCard } from './ProductCard';
import * as styled from './styled';

export const MyProducts: React.FC = () => {
  const [{ getProducts, toggleSubscription }] = useProducts();
  const { toggle, isShowing } = useModal();

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

  return (
    <styled.MyProducts>
      <Header />
      <PageSection header="MMI Products">
        <div>
          <Show visible={products.some((p) => p.isSubscribed)}>
            <Row className="page-section-title">
              <FaEnvelope className="page-section-icon" /> Subscribed
            </Row>
            <p>
              You are currently subscribed to the following products. Clicking on the Unsubscribe
              action action will unsubscribe you from that product.
            </p>
            <div>
              {products
                .filter((product) => product.isSubscribed)
                .map((product) => {
                  return (
                    <ProductCard
                      key={product.id}
                      product={product}
                      onToggleSubscription={(product) => {
                        setProduct(product);
                        toggle();
                      }}
                    />
                  );
                })}
            </div>
          </Show>
          <Row className="page-section-title">
            <FaUserPlus className="page-section-icon" /> Available products
          </Row>
          <p>
            You may request subscription to the following automated products. Susbscribed products
            are sent by email on a scheduled basis.
          </p>
          <div>
            {products
              .filter((product) => !product.isSubscribed)
              .map((product) => {
                return (
                  <ProductCard
                    key={product.id}
                    product={product}
                    onToggleSubscription={(product) => {
                      setProduct(product);
                      toggle();
                    }}
                  />
                );
              })}
          </div>
        </div>
      </PageSection>
      <Modal
        headerText={`Confirm ${product?.isSubscribed ? 'Unsubscribe' : 'Subscribe'}`}
        body={`Are you sure you wish to ${
          product?.isSubscribed ? 'Unsubscribe from' : 'Subscribe to'
        } "${product?.name}"?`}
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
