import { Action } from 'components/action';
import { Section } from 'components/section';
import React from 'react';
import { FaFileLines, FaRotateLeft, FaUserMinus, FaUserPlus } from 'react-icons/fa6';
import { Col, IProductSubscriberModel, Show } from 'tno-core';

export interface IProductCardProps {
  /** The product to display on this card. */
  product: IProductSubscriberModel;
  /** Event fires when user requests to delete product. This event does not delete the product itself. */
  onToggleSubscription?: (product: IProductSubscriberModel) => void;
}

/**
 * A card to display product information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ProductCard: React.FC<IProductCardProps> = ({ product, onToggleSubscription }) => {
  return (
    <Section
      showOpen={false}
      open={true}
      key={product.id}
      icon={<FaFileLines />}
      label={product.name}
      actions={
        <>
          <Show visible={product.requestedIsSubscribedStatus !== undefined}>
            <Action
              className={`action-cancel-request`}
              icon={<FaRotateLeft />}
              label={`Cancel pending request`}
              onClick={() => {
                onToggleSubscription?.(product);
              }}
            />
          </Show>
          <Show visible={product.requestedIsSubscribedStatus === undefined}>
            <Action
              className={product.isSubscribed ? `action-unsubscribe` : `action-subscribe`}
              icon={product.isSubscribed ? <FaUserMinus /> : <FaUserPlus />}
              label={product.isSubscribed ? `Unsubscribe` : `Subscribe`}
              onClick={() => {
                onToggleSubscription?.(product);
              }}
            />
          </Show>
        </>
      }
      onChange={(open) => open}
    >
      <Col gap="1rem">{product.description && <div>{product.description}</div>}</Col>
    </Section>
  );
};
