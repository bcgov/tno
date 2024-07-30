import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSystemMessages } from 'store/hooks/admin';
import {
  CellCheckbox,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  IGridHeaderColumnProps,
  ISystemMessageModel,
  Link,
  Row,
  SortDirection,
} from 'tno-core';

import * as styled from './styled';
import { SystemMessageFilter } from './SystemMessageFilter';

export const SystemMessageList: React.FC = () => {
  const navigate = useNavigate();
  const [, { findSystemMessages }] = useSystemMessages();

  const [filter, setFilter] = React.useState('');
  const [messages, setMessages] = React.useState<ISystemMessageModel[]>([]);

  const fetch = React.useCallback(async () => {
    try {
      const messages = await findSystemMessages();
      return setMessages(messages);
    } catch {}
  }, [findSystemMessages]);

  React.useEffect(() => {
    fetch();
    // Only load on first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSortChange = React.useCallback(
    (column: IGridHeaderColumnProps, direction: SortDirection) => {
      if (column.name) {
      }
    },
    [],
  );

  return (
    <styled.SystemMessageList>
      <Row className="add-media" justifyContent="space-between" gap="1rem">
        <Col>
          <p>
            System messages provide a way to inform users of site outages, maintenance windows, and
            important information.
          </p>
          <p>Only the first enabled message will be displayed on the login page.</p>
        </Col>
        <IconButton
          iconType="plus"
          label="System Message"
          onClick={() => navigate('/admin/system-messages/0')}
        />
      </Row>
      <SystemMessageFilter onSearch={(keywords) => setFilter(keywords)} />
      <Grid
        items={messages.filter((m) => !filter || m.name.includes(filter))}
        onSortChange={async (column, direction) => {
          handleSortChange(column, direction);
        }}
        renderHeader={() => [
          {
            name: 'name',
            label: 'Name',
            sortable: true,
          },
          { name: 'description', label: 'Description', size: '2fr' },
          { name: 'isEnabled', label: 'Enabled', size: '120px', sortable: true },
        ]}
        renderColumns={(row: ISystemMessageModel, rowIndex) => {
          return [
            {
              column: (
                <div key="1" className="clickable">
                  <CellEllipsis>
                    <Link to={`${row.id}`}>{row.name}</Link>
                  </CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="2" className="clickable">
                  <CellEllipsis>{row.description}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="3" className="clickable">
                  <CellCheckbox checked={row.isEnabled} />
                </div>
              ),
            },
          ];
        }}
      />
    </styled.SystemMessageList>
  );
};

export default SystemMessageList;
