import React from 'react';
import { useParams } from 'react-router-dom';
import { useProducts } from 'store/hooks/admin';
import { IProductModel, IUserProductModel, Show } from 'tno-core';

import * as styled from './styled';
import { UserApproveDeny } from './UserApproveDeny';

export const ProductSubRequests: React.FC = () => {
  const { id } = useParams();
  const [, { getProduct }] = useProducts();

  const [product, setProduct] = React.useState<IProductModel>();
  // users that are flagged to have a subscription request change reviewed
  const [flaggedUsers, setFlaggedUsers] = React.useState<IUserProductModel[]>([]);

  React.useEffect(() => {
    if (Number(id) && !product) {
      getProduct(Number(id)).then((result) => {
        setProduct(result);
        setFlaggedUsers(result.subscribers.filter((user) => user.requestedIsSubscribedStatus));
      });
    }
  }, [id, getProduct, product]);

  const handleUserUpdate = (updatedUser: IUserProductModel) => {
    setFlaggedUsers((prevUsers) => prevUsers.filter((user) => user.id !== updatedUser.id));
    setProduct((prevProduct) => {
      if (!prevProduct) return prevProduct;
      return {
        ...prevProduct,
        subscribers: prevProduct.subscribers.map((user) =>
          user.id === updatedUser.id ? updatedUser : user,
        ),
      };
    });
  };

  return (
    <styled.ProductSubRequests>
      <h3>Subscription Requests</h3>
      {flaggedUsers
        ?.filter((user) => !user.isSubscribed)
        .map((user) => (
          <UserApproveDeny
            key={user.id}
            user={user}
            product={product}
            setProduct={setProduct}
            onUserUpdate={handleUserUpdate}
          />
        ))}
      <Show visible={flaggedUsers.filter((u) => !u.isSubscribed).length === 0}>
        <div className="user-row">No subscribe requests to review</div>
      </Show>
      <h3 className="unsub">Unsubscribe Requests</h3>
      {flaggedUsers
        ?.filter((user) => user.isSubscribed)
        .map((user) => (
          <UserApproveDeny
            key={user.id}
            user={user}
            product={product}
            setProduct={setProduct}
            onUserUpdate={handleUserUpdate}
          />
        ))}
      <Show visible={flaggedUsers.filter((u) => u.isSubscribed).length === 0}>
        <div className="user-row">No unsubscribe requests to review</div>
      </Show>
    </styled.ProductSubRequests>
  );
};
