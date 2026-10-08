import React from 'react';
import { useUsers } from 'store/hooks/admin';
import { UserAccountTypeName } from 'tno-core';

/**
 * A subscriber of a report or notification, which may be a distribution list.
 */
export interface IDistributionListSubscriber {
  /** The user ID of the subscriber. */
  userId: number;
  /** Whether the subscriber is subscribed. */
  isSubscribed: boolean;
  /** The type of account, a distribution list is a type of account. */
  accountType?: UserAccountTypeName;
  /** The username of the subscriber. */
  username: string;
  /** The display name of the subscriber. */
  displayName?: string;
}

/**
 * Provides the subscribed distribution lists each user is a member of.
 * A member of a subscribed distribution list receives the report or notification through it,
 * even when the member is not subscribed directly.
 * @param subscribers The subscribers of a report or notification.
 * @returns The names of the subscribed distribution lists each user is a member of, keyed by user ID.
 */
export const useDistributionListMembership = (subscribers: IDistributionListSubscriber[]) => {
  const [, { getDistributionListById }] = useUsers();
  const [membership, setMembership] = React.useState<Record<number, string[]>>({});

  const lists = subscribers.filter(
    (s) => s.isSubscribed && s.accountType === UserAccountTypeName.Distribution,
  );
  // Only fetch the members again when the subscribed distribution lists change.
  const listIds = lists
    .map((list) => list.userId)
    .sort((a, b) => a - b)
    .join(',');

  React.useEffect(() => {
    let isCurrent = true;
    Promise.all(
      lists.map(async (list) => ({ list, members: await getDistributionListById(list.userId) })),
    )
      .then((results) => {
        if (!isCurrent) return;
        const result: Record<number, string[]> = {};
        results.forEach(({ list, members }) =>
          members.forEach((member) => {
            result[member.id] = [...(result[member.id] ?? []), list.displayName || list.username];
          }),
        );
        setMembership(result);
      })
      .catch(() => {});
    return () => {
      isCurrent = false;
    };
    // The lists are only fetched when their IDs change.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [getDistributionListById, listIds]);

  return membership;
};
