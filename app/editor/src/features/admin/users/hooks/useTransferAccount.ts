import { useFormikContext } from 'formik';
import React from 'react';
import {
  useFilters,
  useFolders,
  useNotifications,
  useProducts,
  useReports,
} from 'store/hooks/admin';
import {
  IFilterModel,
  IFolderModel,
  INotificationModel,
  IProductModel,
  IReportModel,
  IReportSectionModel,
  ISortableModel,
  ITransferAccount,
  ITransferObject,
  ITransferReport,
  ITransferReportSection,
  IUserModel,
} from 'tno-core';

import { defaultTransferAccount } from '../constants';

interface IUserSubscriptions {
  notifications: INotificationModel[];
  reports: IReportModel[];
  products: IProductModel[];
}

interface IUserObjects extends IUserSubscriptions {
  filters: IFilterModel[];
  folders: IFolderModel[];
}

interface IUserObject<T extends number> extends ISortableModel<T> {
  ownerId?: number;
}

/**
 * Provides a hook to manage a transfer account for the current user.
 * @returns Objects and functions to manage a transfer account.
 */
export const useTransferAccount = () => {
  const { values } = useFormikContext<IUserModel>();
  const [, { findNotifications }] = useNotifications();
  const [, { findFilters }] = useFilters();
  const [, { findFolders }] = useFolders();
  const [, { findReports }] = useReports();
  const [, { findProducts }] = useProducts();

  const [userObjects, setUserObjects] = React.useState<IUserObjects>({
    filters: [],
    folders: [],
    reports: [],
    notifications: [],
    products: [],
  });
  const [userSubscriptions, setUserSubscriptions] = React.useState<IUserSubscriptions>({
    notifications: [],
    reports: [],
    products: [],
  });
  const [account, setAccount] = React.useState<ITransferAccount>({
    ...defaultTransferAccount,
    toAccountId: values.id,
  });

  React.useEffect(() => {
    if (values.id) {
      setAccount({
        ...defaultTransferAccount,
        toAccountId: values.id,
      });
    }
  }, [values.id]);

  /** Fetch all the objects that belong to the specified user. */
  const getUserObjects = React.useCallback(
    async (userId: number): Promise<IUserObjects> => {
      try {
        const notifications = await findNotifications({
          ownerId: userId,
        });
        const filters = await findFilters({ ownerId: userId });
        const folders = await findFolders({ ownerId: userId });
        const reports = await findReports({ ownerId: userId });
        return { notifications, filters, reports, folders, products: [] };
      } catch {
        return { notifications: [], filters: [], reports: [], folders: [], products: [] };
      }
    },
    [findFilters, findFolders, findNotifications, findReports],
  );

  /** Fetch all the objects the specified user is subscribed to. */
  const getUserSubscriptions = React.useCallback(
    async (userId: number): Promise<IUserSubscriptions> => {
      try {
        const notifications = await findNotifications({
          subscriberUserId: userId,
        });
        const reports = await findReports({ subscriberUserId: userId });
        const products = await findProducts({ subscriberUserId: userId });
        return { notifications, reports, products };
      } catch {
        return { notifications: [], reports: [], products: [] };
      }
    },
    [findNotifications, findProducts, findReports],
  );

  const initUser = React.useCallback(
    async (userId: number) => {
      try {
        // Get all the objects the specified user owns to ensure duplicate name conflicts do not occur.
        const userObjects = await getUserObjects(userId);
        const userSubscriptions = await getUserSubscriptions(userId);

        // Only keep the subscriptions that are to objects the specified user will not own.
        userSubscriptions.notifications = userSubscriptions.notifications.filter(
          (n) => !userObjects.notifications.some((un) => un.id === n.id),
        );
        userSubscriptions.reports = userSubscriptions.reports.filter(
          (r) => !userObjects.reports.some((ur) => ur.id === r.id),
        );
        setUserObjects(userObjects);
        setUserSubscriptions(userSubscriptions);
      } catch {}
    },
    [getUserObjects, getUserSubscriptions],
  );

  React.useEffect(() => {
    if (values.id) initUser(values.id);
    // Only init user if the actual user changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.id]);

  /** Convert an account object to a transfer object. */
  const createTransferObject = React.useCallback(
    <RT extends ITransferObject, IT extends IUserObject<number>>(
      item: IT,
      subscribeOnly: boolean = false,
      apply: (item: IT, transfer: ITransferObject) => RT = (_, t) => t as RT,
    ) => {
      const transfer: ITransferObject = {
        checked: true,
        originalId: item.id,
        originalName: item.name,
        newId: item.id,
        subscribeOnly,
      };
      return apply(item, transfer);
    },
    [],
  );

  /** Convert account objects to transfer objects. */
  const createTransferObjects = React.useCallback(
    <RT extends ITransferObject, IT extends IUserObject<number>>(
      items: IT[],
      subscribeOnly: boolean = false,
      apply?: (item: IT, transfer: ITransferObject) => RT,
    ): RT[] => {
      return items.map<RT>((item) => {
        return createTransferObject(item, subscribeOnly, apply);
      });
    },
    [createTransferObject],
  );

  const checkIfReady = React.useCallback(
    (account: ITransferAccount) => {
      const notifications = userObjects.notifications?.map((n) => n.name) ?? [];
      const notificationsReady = account.notifications.every((n) => {
        const showNewName = n.checked && notifications.some((name) => name === n.originalName);
        const mustChangeName =
          showNewName &&
          (!n.newName ||
            n.originalName === n.newName ||
            notifications.some((name) => name === n.newName));
        return !mustChangeName;
      });

      const reports = userObjects.reports?.map((n) => n.name) ?? [];
      const reportsReady = account.reports.every((n) => {
        const showNewName = n.checked && reports.some((name) => name === n.originalName);
        const mustChangeName =
          showNewName && n.newName && reports.some((name) => name === n.newName);
        return !mustChangeName;
      });

      const folders = userObjects.folders?.map((n) => n.name) ?? [];
      const foldersReady = account.folders.every((n) => {
        const showNewName = n.checked && folders.some((name) => name === n.originalName);
        const mustChangeName =
          showNewName && n.newName && folders.some((name) => name === n.newName);
        return !mustChangeName;
      });

      const filters = userObjects.filters?.map((n) => n.name) ?? [];
      const filtersReady = account.filters.every((n) => {
        const showNewName = n.checked && filters.some((name) => name === n.originalName);
        const mustChangeName =
          showNewName && n.newName && filters.some((name) => name === n.newName);
        return !mustChangeName;
      });

      return notificationsReady && reportsReady && foldersReady && filtersReady;
    },
    [userObjects.filters, userObjects.folders, userObjects.notifications, userObjects.reports],
  );

  /** Prepare an account transfer */
  const createTransferAccount = React.useCallback(
    async (
      fromAccountId: number,
      toAccountId: number,
      transferOwnership: boolean,
      includeHistory: boolean,
    ) => {
      try {
        // Get all the objects from the source account that will be transferred or copied to the specified user.
        const accountObjects = await getUserObjects(fromAccountId);
        const accountSubscriptions = await getUserSubscriptions(fromAccountId);

        // Only keep the subscriptions that are to objects the specified user will not own, and where they are not already subscribed.
        accountSubscriptions.notifications = accountSubscriptions.notifications.filter(
          (n) =>
            !accountObjects.notifications.some((an) => an.id === n.id) &&
            !userSubscriptions.notifications.some((un) => un.id === n.id),
        );
        accountSubscriptions.reports = accountSubscriptions.reports.filter(
          (r) =>
            !accountObjects.reports.some((ar) => ar.id === r.id) &&
            !userSubscriptions.reports.some((ur) => ur.id === r.id),
        );
        accountSubscriptions.products = accountSubscriptions.products.filter(
          (p) => !userSubscriptions.products.some((up) => up.id === p.id),
        );

        const filters = createTransferObjects(accountObjects.filters);
        const folders = createTransferObjects(accountObjects.folders, false, (item, transfer) => {
          return { ...transfer, filterId: item.filterId }; // TODO: A copy will require remapping this filter.
        });
        const notifications = createTransferObjects(accountObjects.notifications);
        const reports = createTransferObjects<ITransferReport, IReportModel>(
          accountObjects.reports,
          false,
          (item, transfer) => {
            const sections = createTransferObjects<ITransferReportSection, IReportSectionModel>(
              item.sections,
              false,
              (item, transfer) => {
                return {
                  ...transfer,
                  filterId: item.filterId, // TODO: A copy will require remapping this filter.
                  filter: item.filter ? createTransferObject(item.filter) : undefined,
                  folderId: item.folderId, // TODO: A copy will require remapping this folder.
                  folder: item.folder ? createTransferObject(item.folder) : undefined,
                  linkedReportId: item.linkedReportId, // TODO: A copy will require remapping this linked report.
                };
              },
            );
            return { ...transfer, sections };
          },
        );

        const notificationSubscriptions = createTransferObjects(
          accountSubscriptions.notifications,
          true,
        );
        const reportSubscriptions = createTransferObjects<ITransferReport, IReportModel>(
          accountSubscriptions.reports,
          true,
        );
        const productSubscriptions = createTransferObjects(accountSubscriptions.products, true);

        const updatedAccount = {
          ...defaultTransferAccount,
          fromAccountId,
          toAccountId,
          transferOwnership,
          includeHistory,
          isReady: false,
          filters,
          folders,
          notifications: [...notifications, ...notificationSubscriptions],
          reports: [...reports, ...reportSubscriptions],
          products: productSubscriptions,
        };
        updatedAccount.isReady = checkIfReady(updatedAccount);
        setAccount(updatedAccount);
      } catch {}
    },
    [
      checkIfReady,
      createTransferObject,
      createTransferObjects,
      getUserObjects,
      getUserSubscriptions,
      userSubscriptions,
    ],
  );

  return {
    userObjects,
    userSubscriptions,
    account,
    setAccount,
    createTransferAccount,
    checkIfReady,
  };
};
