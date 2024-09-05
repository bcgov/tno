import { useFormikContext } from 'formik';
import React from 'react';
import { Col, IProductModel, ProductRequestStatusName, Row, Show } from 'tno-core';

import * as styled from './styled';
import { UserApproveDeny } from './UserApproveDeny';

export const ProductSubRequests: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IProductModel>();

  // split it into two groups subscription requests and unsubscribe requests
  return (
    <styled.ProductSubRequests>
      <h3>Subscription Requests</h3>
      {values.subscribers.map((user, index) => {
        if (user.status !== ProductRequestStatusName.RequestSubscription) return null;
        return (
          <UserApproveDeny
            key={`user-request-subscription-${user.userId}`}
            user={user}
            onChange={(approve) =>
              setFieldValue(`subscribers.${index}`, {
                ...user,
                status: ProductRequestStatusName.NA,
                isSubscribed: approve,
              })
            }
          />
        );
      })}
      <Show
        visible={
          !values.subscribers.some((u) => u.status === ProductRequestStatusName.RequestSubscription)
        }
      >
        <div className="user-row">No subscribe requests to review</div>
      </Show>
      <h3 className="unsub">Unsubscribe Requests</h3>
      {values.subscribers.map((user, index) => {
        if (user.status !== ProductRequestStatusName.RequestUnsubscribe) return null;
        return (
          <Row key={`user-request-cancellation-${user.userId}`} gap="1rem">
            <Col flex="1">
              <UserApproveDeny
                user={user}
                onChange={(approve) =>
                  setFieldValue(`subscribers.${index}`, {
                    ...user,
                    status: ProductRequestStatusName.NA,
                    isSubscribed: !approve,
                  })
                }
              />
            </Col>
            <Col flex="2">
              <p>
                If this user is currently subscribed through a distribution list, you will need to
                first remove them from the list and then approve this request.
              </p>
            </Col>
          </Row>
        );
      })}
      <Show
        visible={
          !values.subscribers.some((u) => u.status === ProductRequestStatusName.RequestUnsubscribe)
        }
      >
        <div className="user-row">No unsubscribe requests to review</div>
      </Show>
    </styled.ProductSubRequests>
  );
};
