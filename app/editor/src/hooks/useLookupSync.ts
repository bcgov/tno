import _ from 'lodash';
import React from 'react';
import { useSelector } from 'react-redux';
import { useLookupStore } from 'store/slices';

/**
 * Hook that syncs the lookups between the admin store and lookup store. Lookup store will need to be occasionally updated when an admin updates the admin store.
 */
export const useLookupSync = () => {
  const adminProducts = useSelector((state: any) => state.admin.products);
  const lookupProducts = useSelector((state: any) => state.lookup.products);

  const [, store] = useLookupStore();

  React.useEffect(() => {
    // the following line indicates that the admin store has been updated and the lookups need to be synced
    if (!_.isEqual(adminProducts, lookupProducts)) store.storeProducts(adminProducts);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [store]);
};
