import { Modal } from 'components/modal';
import { PageSection } from 'components/section';
import React from 'react';
import { FaEnvelope, FaUserPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useProducts } from 'store/hooks';
import {
  IProductModel,
  IUserProductModel,
  Loading,
  ProductRequestStatusName,
  Show,
  useModal,
} from 'tno-core';

import { ProductCard } from './ProductCard';
import * as styled from './styled';

interface ISelectedProduct {
  product: IProductModel;
  userProduct: IUserProductModel;
}

export const MyProducts: React.FC = () => {
  const [{ getProducts, toggleSubscription }] = useProducts();
  const { toggle, isShowing } = useModal();
  const [{ userInfo }] = useApp();

  const [products, setProducts] = React.useState<IProductModel[]>([]);
  const [active, setActive] = React.useState<ISelectedProduct>();
  const [isLoading, setIsLoading] = React.useState(true);

  React.useEffect(() => {
    if (userInfo && !products.length) {
      setIsLoading(true);
      getProducts().then((data) => {
        setProducts(data);
        setIsLoading(false);
      });
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo]);

  const selectProduct = React.useCallback(
    (product: IProductModel, userProduct: IUserProductModel) => {
      setActive({
        product,
        userProduct,
      });
    },
    [],
  );

  const handleToggleSubscription = React.useCallback(
    (product: IProductModel, userProduct: IUserProductModel) => {
      if (!!userProduct) {
        toggleSubscription(userProduct)
          .then((data) => {
            toast.success(`Successfully change subscription status for product '${product.name}'.`);
            setProducts((products) =>
              products.map((product) => {
                if (product.id === data.productId)
                  return { ...product, subscribers: [userProduct] };
                return product;
              }),
            );
          })
          .catch(() => {});
      }
    },
    [toggleSubscription],
  );

  return (
    <styled.MyProducts>
      <PageSection header="Media Monitoring Products" includeHeaderIcon>
        <div className="product-container">
          <p>
            Access to all products is managed by the MMI Admin team. You may request to subscribe or
            unsubscribe by clicking on the relevant action next to the product. If you wish to
            cancel your request, you can click on the cancel action.
          </p>
          <Show
            visible={products.some((product) =>
              // products which the user *IS* subscribed to
              // OR products which the user has a request to subscribed
              product.subscribers.some(
                (s) => s.isSubscribed || s.status === ProductRequestStatusName.RequestSubscription,
              ),
            )}
          >
            <h2 className="product-section-title">
              <FaEnvelope /> <span>Subscribed</span>
            </h2>
            <p className="info-text">
              You are currently subscribed, or are awaiting approval for subscription to the
              following products.
            </p>
            <div>
              {products
                .filter((product) =>
                  // products which the user *IS* subscribed to
                  // OR products which the user has a request to subscribed
                  product.subscribers.some(
                    (s) =>
                      s.isSubscribed || s.status === ProductRequestStatusName.RequestSubscription,
                  ),
                )
                .map((product) => {
                  return (
                    <ProductCard
                      key={product.id}
                      userId={userInfo?.id}
                      product={product}
                      onToggleSubscription={(userProduct) => {
                        selectProduct(product, userProduct);
                        toggle();
                      }}
                    />
                  );
                })}
            </div>
          </Show>
          <h2 className="product-section-title">
            <FaUserPlus /> <span>Available products</span>
          </h2>
          <p className="info-text">
            You may request subscription to the following automated products. Subscribed products
            are sent by email on a scheduled basis.
          </p>
          <div>
            <Show visible={isLoading}>
              <Loading />
            </Show>
            {products
              .filter((product) =>
                // products which the user *IS NOT* unsubscribed to
                product.subscribers.some(
                  (s) =>
                    !s.isSubscribed && s.status !== ProductRequestStatusName.RequestSubscription,
                ),
              )
              .map((product) => {
                return (
                  <ProductCard
                    key={product.id}
                    userId={userInfo?.id}
                    product={product}
                    onToggleSubscription={(userProduct) => {
                      selectProduct(product, userProduct);
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
          (active?.userProduct.status === ProductRequestStatusName.RequestSubscription
            ? `Are you sure you wish to ${
                active.userProduct.isSubscribed ? 'unsubscribe from' : 'subscribe to'
              }`
            : `Are you sure you wish to cancel your pending request to ${
                active?.userProduct.isSubscribed ? 'unsubscribe from' : 'subscribe to'
              }`) + `"${active?.product.name}"?`
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText={
          active?.userProduct.status === ProductRequestStatusName.RequestSubscription
            ? `Yes, ${
                active.userProduct.isSubscribed ? 'request to unsubscribe' : 'request to subscribe'
              }`
            : `Yes, cancel my pending request to ${
                active?.userProduct.isSubscribed ? 'unsubscribe' : 'subscribe'
              }`
        }
        onConfirm={() => {
          if (active) handleToggleSubscription(active.product, active.userProduct);
          toggle();
        }}
      />
    </styled.MyProducts>
  );
};
