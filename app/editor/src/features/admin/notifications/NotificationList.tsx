import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useNotifications } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, INotificationModel, Row } from 'tno-core';

import { columns } from './constants';
import { NotificationFilter } from './NotificationFilter';
import * as styled from './styled';

const NotificationList: React.FC = () => {
  const navigate = useNavigate();
  const [{ notifications }, api] = useNotifications();

  const [items, setItems] = React.useState<INotificationModel[]>([]);

  React.useEffect(() => {
    if (!notifications.length) {
      api.findAllNotifications().then((data) => {
        setItems(data);
      });
    } else {
      setItems(notifications);
    }
  }, [api, notifications]);

  return (
    <styled.NotificationList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Notifications provide a way to generate output (i.e. email) based on a filter or
            manually selecting content to be included.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new Notification`}
            onClick={() => navigate(`/admin/Notifications/0`)}
          />
        </Row>
        <NotificationFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                notifications.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(notifications);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.NotificationList>
  );
};

export default NotificationList;
