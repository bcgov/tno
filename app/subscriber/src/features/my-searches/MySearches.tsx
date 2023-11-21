import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { Col, FlexboxTable, IFilterModel, Modal, Row, toQueryString, useModal } from 'tno-core';

import { columns } from './constants/columns';
import * as styled from './styled';
import { settingsToFilter } from './utils';

/** contains a list of the user's filters, allows for edit and viewing */
export const MySearches = () => {
  const [, { findMyFilters, updateFilter, deleteFilter }] = useFilters();
  const { toggle, isShowing } = useModal();
  const [{ actions }] = useLookup();
  const navigate = useNavigate();
  const [{ filter }, { storeFilter }] = useContent();
  const [myFilters, setMyFilters] = React.useState<IFilterModel[]>([]);
  const [active, setActive] = React.useState<IFilterModel>();
  const [editable, setEditable] = React.useState<string>('');
  const [actionName, setActionName] = React.useState<'delete'>('delete');
  const topStoryId = actions.find((action) => action.name === 'Top Story')?.id ?? 0;

  React.useEffect(() => {
    findMyFilters().then((data) => {
      setMyFilters(data);
    });
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = () => {
    setActionName('delete');
    toggle();
  };

  const handleSave = () => {
    if (!!active) {
      updateFilter(active).then((data) => {
        toast.success(`${data.name} updated successfully`);
        setMyFilters([...myFilters.filter((filter) => filter.id !== data.id), data]);
        setEditable('');
      });
    }
  };
  return (
    <styled.MySearches>
      <Row>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns(setActive, editable, handleSave, handleDelete, active)}
          rowId={'id'}
          onRowClick={(e) => {
            setActive(e.original);
            storeFilter(settingsToFilter(e.original, e.original.id, topStoryId));
            navigate('/search');
          }}
          data={myFilters}
          showActive={false}
        />
        <Tooltip
          clickable
          openOnClick
          place="right"
          id="edit-name"
          variant="light"
          className="options"
        >
          <Col className="filter-container">
            <div className="option" onClick={() => setEditable(active?.name ?? '')}>
              Edit filter name
            </div>
          </Col>
        </Tooltip>
      </Row>
      <Modal
        headerText="Confirm Removal"
        body={`Are you sure you wish to ${actionName} this filter?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          try {
            if (!!active) {
              if (actionName === 'delete') {
                deleteFilter(active).then(() => {
                  toast.success(`${active.name} deleted successfully`);
                  setMyFilters(myFilters.filter((filter) => filter.id !== active.id));
                });
              }
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.MySearches>
  );
};
