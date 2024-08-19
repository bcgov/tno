import { useFormikContext } from 'formik';
import React from 'react';
import { useUsers } from 'store/hooks/admin';
import {
  IProductModel,
  IUserFilter,
  IUserProductModel,
  ProductRequestStatusName,
  ProductTypeName,
  Show,
} from 'tno-core';

import { ProductEveningOverviewSubscribersForm } from './ProductEveningOverviewSubscribersForm';
import { ProductFilter } from './ProductFilter';
import { ProductNotificationSubscribersForm } from './ProductNotificationSubscribersForm';
import { ProductReportSubscribersForm } from './ProductReportSubscribersForm';
import * as styled from './styled';

export const ProductSubscribersForm = () => {
  const { values } = useFormikContext<IProductModel>();
  const [{ users }, { findUsers }] = useUsers();

  const [filter, setFilter] = React.useState<IUserFilter>({ page: 1, quantity: 100, sort: [] });
  const [subscribers, setSubscribers] = React.useState<IUserProductModel[]>([]);

  const fetch = React.useCallback(
    async (filter: IUserFilter) => {
      try {
        await findUsers(filter);
      } catch {}
    },
    [findUsers],
  );

  React.useEffect(() => {
    fetch(filter).catch();
    // The fetch method will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  React.useEffect(() => {
    findUsers({}).catch(() => {
      // Handled already.
    });
    // Fetch users on initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    // Merge users with subscribers to this product.
    setSubscribers(
      users.items.map<IUserProductModel>((user) => {
        var subscription = values.subscribers.find((s) => s.userId === user.id);
        var userSubscription: IUserProductModel = {
          username: user.username,
          email: user.email,
          preferredEmail: user.preferredEmail,
          displayName: user.displayName,
          firstName: user.firstName,
          lastName: user.lastName,
          accountType: user.accountType,
          emailVerified: user.emailVerified,
          isEnabled: user.isEnabled,

          userId: user.id,
          productId: values.id,
          status: ProductRequestStatusName.NA,
          isSubscribed: false,
          ...subscription,
        };
        return userSubscription;
      }),
    );
  }, [users, values.id, values.subscribers]);

  return (
    <styled.ProductSubscribersForm>
      <ProductFilter
        filter={filter}
        productId={values.id}
        onSearch={async (filter: IUserFilter) => setFilter({ ...filter, page: 1 })}
      />
      <Show visible={values.productType === ProductTypeName.Report}>
        <ProductReportSubscribersForm
          users={subscribers}
          page={users.page}
          quantity={users.quantity}
          total={users.total}
          setFilter={setFilter}
        />
      </Show>
      <Show visible={values.productType === ProductTypeName.Notification}>
        <ProductNotificationSubscribersForm
          users={subscribers}
          page={users.page}
          quantity={users.quantity}
          total={users.total}
          setFilter={setFilter}
        />
      </Show>
      <Show visible={values.productType === ProductTypeName.EveningOverview}>
        <ProductEveningOverviewSubscribersForm
          users={subscribers}
          page={users.page}
          quantity={users.quantity}
          total={users.total}
          setFilter={setFilter}
        />
      </Show>
    </styled.ProductSubscribersForm>
  );
};
