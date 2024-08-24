import { Action } from 'components/action';
import React from 'react';
import { FaFileLines, FaRotateLeft, FaUserMinus, FaUserPlus } from 'react-icons/fa6';
import {
  Col,
  IProductModel,
  IUserProductModel,
  ProductRequestStatusName,
  Row,
  Show,
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
  const userProduct = product.subscribers.find((s) => s.userId === userId);
  const isSubscribed = userProduct?.isSubscribed ?? false;
  const isRequesting =
    userProduct &&
    !userProduct.isSubscribed &&
    userProduct.status === ProductRequestStatusName.RequestSubscription;
  const isCancelling =
    userProduct &&
    userProduct.isSubscribed &&
    userProduct.status === ProductRequestStatusName.RequestUnsubscribe;

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
                  status: !userProduct.isSubscribed
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
