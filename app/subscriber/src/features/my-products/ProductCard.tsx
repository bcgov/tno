import { Action } from 'components/action';
import React from 'react';
import { FaFileLines, FaRotateLeft, FaUserMinus, FaUserPlus } from 'react-icons/fa6';
import { useApp } from 'store/hooks';
import {
  Col,
  IProductModel,
  IUserProductModel,
  ProductRequestStatusName,
  Row,
  Show,
  UserAccountTypeName,
} from 'tno-core';

export interface IProductCardProps {
  /** The user ID */
  userId?: number;
  /** The product to display on this card. */
  product: IProductModel;
  /** Event fires when user requests to delete product. This event does not delete the product itself. */
  onToggleSubscription?: (userProduct: IUserProductModel) => void;
}

/**
 * A card to display product information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ProductCard: React.FC<IProductCardProps> = ({
  userId,
  product,
  onToggleSubscription,
}) => {
  const [{ userInfo }] = useApp();

  let userProduct: IUserProductModel | undefined = product.subscribers.find(
    (s) => s.userId === userId,
  ) ?? {
    userId: userId ?? 0,
    productId: product.id,
    status: ProductRequestStatusName.NA,
    isSubscribed: false,
    username: userInfo?.username ?? '',
    email: userInfo?.email ?? '',
    preferredEmail: userInfo?.preferredEmail ?? '',
    emailVerified: false,
    displayName: userInfo?.displayName ?? '',
    firstName: userInfo?.firstName ?? '',
    lastName: userInfo?.lastName ?? '',
    isEnabled: userInfo?.isEnabled ?? true,
    accountType: UserAccountTypeName.Direct,
  };
  // Check a distribution list to see if user is subscribed this way.
  if (product.subscribers.length > 1 && !userProduct?.isSubscribed) {
    const distribution = product.subscribers.find((s) => s.isSubscribed);
    // Switch the subscription to the user from the distribution list.
    if (distribution) userProduct = { ...distribution, userId: userId ?? 0 };
  }
  const isSubscribed = userProduct.isSubscribed;
  const isRequesting =
    !userProduct.isSubscribed &&
    userProduct.status === ProductRequestStatusName.RequestSubscription;
  const isCancelling =
    userProduct.isSubscribed && userProduct.status === ProductRequestStatusName.RequestUnsubscribe;

  return (
    <Col className="product-card">
      <Row className="product-row">
        <FaFileLines />
        <span className="product-name">{product.name}</span>
        <Show visible={isRequesting || isCancelling}>
          <Action
            className={`action-cancel-request`}
            icon={<FaRotateLeft />}
            label={`Cancel pending request`}
            onClick={() => {
              userProduct &&
                onToggleSubscription?.({ ...userProduct, status: ProductRequestStatusName.NA });
            }}
          />
        </Show>
        <Show visible={!isRequesting && !isCancelling}>
          <Action
            className={isSubscribed ? `action-unsubscribe` : `action-subscribe`}
            icon={isSubscribed ? <FaUserMinus /> : <FaUserPlus />}
            label={isSubscribed ? `Unsubscribe` : `Subscribe`}
            onClick={() => {
              userProduct &&
                onToggleSubscription?.({
                  ...userProduct,
                  status: !isSubscribed
                    ? ProductRequestStatusName.RequestSubscription
                    : ProductRequestStatusName.RequestUnsubscribe,
                });
            }}
          />
        </Show>
      </Row>
      {product.description && <div className="product-description">{product.description}</div>}
    </Col>
  );
};
