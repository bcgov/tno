import React from 'react';
import { toast } from 'react-toastify';
import { useProducts } from 'store/hooks/admin';
import {
  IProductModel,
  IUserProductModel,
  ReportDistributionFormatName,
  Row,
  ToggleGroup,
} from 'tno-core';

export interface IUserApproveDenyProps {
  user: IUserProductModel;
  product?: IProductModel;
  setProduct: React.Dispatch<React.SetStateAction<IProductModel | undefined>>;
}

export const UserApproveDeny: React.FC<IUserApproveDenyProps> = ({ user, product, setProduct }) => {
  const [, { updateProduct }] = useProducts();
  const handleApprove = async (user: IUserProductModel) => {
    if (!product) return;
    // update the user's subscription status
    const updatedUser = {
      ...user,
      isSubscribed: !user.isSubscribed,
      format: ReportDistributionFormatName.FullText,
      requestedIsSubscribedStatus: false,
    };
    // update the product's subscribers list
    await updateProduct({
      ...product,
      subscribers: product.subscribers.map((u) => (u.id === user.id ? updatedUser : u)),
    }).then((result) => {
      setProduct(result);
      toast.success(`Subscription request approved for ${user.firstName} ${user.lastName}`);
    });
  };

  const handleReject = async (user: IUserProductModel) => {
    if (!product) return;
    // update the user's subscription status
    const updatedUser = {
      ...user,
      requestedIsSubscribedStatus: false,
    };
    // update the product's subscribers list
    await updateProduct({
      ...product,
      subscribers: product.subscribers.map((u) => (u.id === user.id ? updatedUser : u)),
    }).then((result) => {
      setProduct(result);
      toast.success(`Subscription request rejected for ${user.firstName} ${user.lastName}`);
    });
  };
  return (
    <Row className="user-row" key={`${user.id}-${user.isSubscribed}`}>
      <div className="user-name">{`${user.firstName} ${user.lastName} `}</div>
      <ToggleGroup
        options={[
          {
            label: 'Approve',
            onClick: () => handleApprove(user),
          },
          { label: 'Reject', onClick: () => handleReject(user) },
        ]}
        className="toggle-group"
      />
    </Row>
  );
};
