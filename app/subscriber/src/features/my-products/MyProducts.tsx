import { Modal } from 'components/modal';
import { PageSection } from 'components/section';
import React from 'react';
import { FaEnvelope, FaUserPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useProducts } from 'store/hooks';
import { IProductSubscriberModel, Row, Show, useModal } from 'tno-core';

import { ProductCard } from './ProductCard';
import * as styled from './styled';

export const MyProducts: React.FC = () => {
  const [{ getProducts, toggleSubscription }] = useProducts();
  const { toggle, isShowing } = useModal();
  const [{ userInfo }] = useApp();

  const [products, setProducts] = React.useState<IProductSubscriberModel[]>([]);
  const [product, setProduct] = React.useState<IProductSubscriberModel>();

  React.useEffect(() => {
    if (userInfo && !products.length) {
      getProducts().then((data) => {
        setProducts(data);
      });
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo]);

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
      <PageSection header="MMI Products" includeHeaderIcon>
        <div>
          <p>
            Access to all products is managed by the MMI Admin team. You may request to subscribe or
            unsubscribe by clicking on the relevant action next to the product. If you wish to
            cancel your request, you can click on the cancel action.
          </p>
          <Show
            visible={products.some(
              (p) =>
                // products which the user *IS* subscribed to
                p.isSubscribed ||
                // OR products which the user has a request to subscribed
                (p.requestedIsSubscribedStatus !== undefined && p.requestedIsSubscribedStatus),
            )}
          >
            <Row className="page-section-title">
              <FaEnvelope className="page-section-icon" /> Subscribed
            </Row>
            <p>
              You are currently subscribed, or are awaiting approval for subscription to the
              following products.
            </p>
            <div>
              {products
                .filter(
                  (product) =>
                    // products which the user *IS* subscribed to
                    (product.isSubscribed && product.requestedIsSubscribedStatus === undefined) ||
                    // OR products which the user has a request to subscribed
                    (product.requestedIsSubscribedStatus !== undefined &&
                      product.requestedIsSubscribedStatus),
                )
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
              .filter(
                (product) =>
                  // products which the user *IS NOT* unsubscribed to
                  (!product.isSubscribed && product.requestedIsSubscribedStatus === undefined) ||
                  // *OR products which the user has a request to unsubscribe from
                  (product.requestedIsSubscribedStatus !== undefined &&
                    !product.requestedIsSubscribedStatus),
              )
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
        headerText={`Confirm change`}
        body={
          product?.requestedIsSubscribedStatus === undefined
            ? `Are you sure you wish to ${
                product?.isSubscribed ? 'unsubscribe from' : 'subscribe to'
              } "${product?.name}"?`
            : `Are you sure you wish to cancel your pending request to ${
                product?.requestedIsSubscribedStatus ? 'unsubscribe from' : 'subscribe to'
              } "${product?.name}"?`
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText={
          product?.requestedIsSubscribedStatus === undefined
            ? `Yes, ${product?.isSubscribed ? 'request to unsubscribe' : 'request to subscribe'}`
            : `Yes, cancel my pending request to ${
                product?.requestedIsSubscribedStatus ? 'unsubscribe' : 'subscribe'
              }`
        }
        onConfirm={() => {
          if (product) handleToggleSubscription(product);
          toggle();
        }}
      />
    </styled.MyProducts>
  );
};
