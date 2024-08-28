import moment from 'moment';
import React from 'react';
import { FaArrowAltCircleLeft, FaArrowAltCircleRight } from 'react-icons/fa';
import { useApp } from 'store/hooks';
import { useNotifications } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  IconButton,
  IDashboardFilter,
  INotificationInstanceModel,
  IOptionItem,
  Loader,
  NotificationStatusName,
  OptionItem,
  Row,
  Select,
  SelectDate,
  Text,
} from 'tno-core';

import { NotificationCard } from './NotificationCard';
import * as styled from './styled';
import { getNotificationOptions } from './utils';

export const NotificationsDashboard: React.FC = () => {
  const [, { getDashboard, findNotifications }] = useNotifications();
  const [{ requests }] = useApp();

  const startDate = new Date().setHours(0, 0, 0);
  const endDate = new Date().setHours(23, 59, 59);

  const [search, setSearch] = React.useState('');
  const [filter, setFilter] = React.useState<IDashboardFilter>({
    page: 1,
    quantity: 10,
    isEnabled: true,
    notificationStatus: [NotificationStatusName.Failed],
    startDate: moment(startDate).format('YYYY-MM-DDTHH:mm:ss'),
    endDate: moment(endDate).format('YYYY-MM-DDTHH:mm:ss'),
  });
  const [notificationInstances, setNotificationInstances] = React.useState<
    INotificationInstanceModel[]
  >([]);
  const [notificationOptions, setNotificationOptions] = React.useState<IOptionItem[]>([]);

  const isLoading = requests.some((r: any) => r.url === 'get-dashboard');

  const fetchNotifications = React.useCallback(
    async (filter: IDashboardFilter) => {
      try {
        const results = await getDashboard(filter);
        setNotificationInstances(results);
      } catch {}
    },
    [getDashboard],
  );

  React.useEffect(() => {
    fetchNotifications(filter).catch(() => {});
    // Only when filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  React.useEffect(() => {
    findNotifications().then((data) => {
      setNotificationOptions(
        getNotificationOptions(data, filter.notificationId ? filter.notificationId : 0),
      );
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.NotificationsDashboard>
      <h1>Notifications Dashboard</h1>
      <Row justifyContent="center" gap="1rem;">
        <div className="filters">
          <div>
            <Select
              label="Notification"
              name="notification"
              options={notificationOptions}
              value={notificationOptions.find((o) => o.value === filter.notificationId)}
              isClearable={true}
              onChange={(newValue) => {
                const value =
                  newValue instanceof OptionItem
                    ? newValue.toInterface()
                    : (newValue as IOptionItem);
                if (value) {
                  setFilter((filter) => ({
                    ...filter,
                    notificationId: value.value,
                  }));
                } else {
                  setFilter((filter) => ({
                    ...filter,
                    notificationId: undefined,
                  }));
                }
              }}
            />
          </div>
          <div className="keyword-filter">
            <SelectDate
              label="Start Date:"
              name="startDate"
              placeholderText="mm/dd/yyyy"
              selected={filter.startDate ? new Date(filter.startDate) : new Date()}
              onChange={(date) => {
                const value = moment(date?.setHours(0, 0, 0));
                setFilter((filter) => ({
                  ...filter,
                  startDate: value.format('YYYY-MM-DDTHH:mm:ss'),
                }));
              }}
            />
          </div>
          <div className="keyword-filter">
            <SelectDate
              label="End Date:"
              name="endDate"
              placeholderText="mm/dd/yyyy"
              selected={filter.endDate ? new Date(filter.endDate) : new Date()}
              onChange={(date) => {
                const value = moment(date?.setHours(23, 59, 59));
                setFilter((filter) => ({
                  ...filter,
                  endDate: value.format('YYYY-MM-DDTHH:mm:ss'),
                }));
              }}
            />
          </div>
          <div>
            <Checkbox
              name="failed"
              label="Show failed only"
              className="failed-filter"
              checked={filter.notificationStatus?.includes(NotificationStatusName.Failed)}
              onChange={(e) =>
                setFilter((filter) => ({
                  ...filter,
                  notificationStatus: e.target.checked ? [NotificationStatusName.Failed] : [],
                }))
              }
            />
          </div>
          <div>
            <Text
              className="keyword-filter"
              name="keyword"
              value={search ?? ''}
              onChange={(e) => setSearch(e.target.value)}
            >
              <IconButton
                iconType="search"
                onClick={(e) => setFilter((filter) => ({ ...filter, page: 1, keyword: search }))}
              />
            </Text>
          </div>
        </div>
      </Row>
      <div className="header">
        <div>Name</div>
        <div>Subject</div>
        <div>Owner</div>
        <div>Status</div>
        <div>Last Sent</div>
        <div></div>
      </div>
      <div className="notifications">
        <Loader visible={isLoading} />
        <div className="notification-cards">
          {notificationInstances.map((i) => (
            <NotificationCard key={i.id} instance={i} />
          ))}
        </div>
      </div>
      <Row justifyContent="center">
        {filter.page && filter.page > 1 && (
          <Button
            variant={ButtonVariant.link}
            title="Previous"
            onClick={() => setFilter((filter) => ({ ...filter, page: filter.page! - 1 }))}
          >
            <FaArrowAltCircleLeft />
          </Button>
        )}
        <Text
          name="quantity"
          type="number"
          width="8ch"
          value={filter.quantity ?? ''}
          onChange={(e) => {
            const value = e.target.value;
            const quantity = +value;
            setFilter((filter) => ({ ...filter, page: 1, quantity }));
          }}
        />
        {filter.quantity && filter.page && filter.quantity <= notificationInstances.length && (
          <Button
            variant={ButtonVariant.link}
            title="Next"
            onClick={() => setFilter((filter) => ({ ...filter, page: filter.page! + 1 }))}
          >
            <FaArrowAltCircleRight />
          </Button>
        )}
      </Row>
    </styled.NotificationsDashboard>
  );
};

export default NotificationsDashboard;
