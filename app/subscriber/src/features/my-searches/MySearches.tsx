import { SubscriberTableContainer } from 'components/table';
import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { Col, FlexboxTable, IFilterModel, Modal, useModal } from 'tno-core';

import { columns } from './constants/columns';
import * as styled from './styled';
import { settingsToFilter } from './utils';

/** contains a list of the user's filters, allows for edit and viewing */
export const MySearches = () => {
  const [, { findMyFilters, updateFilter, deleteFilter }] = useFilters();
  const { toggle, isShowing } = useModal();
  const [{ actions }] = useLookup();
  const navigate = useNavigate();
  const [, { storeSearchFilter: storeFilter }] = useContent();
  const [searchParams] = useSearchParams();

  const [myFilters, setMyFilters] = React.useState<IFilterModel[]>([]);
  const [active, setActive] = React.useState<IFilterModel>();
  const [viewing, setViewing] = React.useState<IFilterModel>();
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

  // action in columns set viewing object, when this is set user is viewing a filter and wants to see results
  React.useEffect(() => {
    if (!!viewing) {
      storeFilter(settingsToFilter(viewing, viewing.id, topStoryId, actions));
      navigate(`/search?viewing=${viewing.name}`);
    }
    // only fire when viewing has value
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [viewing]);

  React.useEffect(() => {
    if (!!active) storeFilter(settingsToFilter(active, active.id, topStoryId, actions));
    // only fire when active has value
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [active]);

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
      <SubscriberTableContainer>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns(setActive, editable, handleSave, handleDelete, setViewing, active)}
          rowId={(e) => e?.id.toString() ?? '0'}
          onRowClick={(e) => {
            setActive(e.original);
            storeFilter(settingsToFilter(e.original, e.original.id, topStoryId, actions));
            navigate('/search');
          }}
          data={myFilters}
          // TODO: Highlight currently viewed search - chat with Bobbi
          activeRowId={searchParams.get('viewing') ?? ''}
          showActive={false}
        />
        <TooltipMenu
          clickable
          openOnClick
          place="right"
          id="modify"
          variant="light"
          className="modify"
        >
          <Col className="filter-container">
            {/* TODO: Upcoming ticket will change this to allow users to modify selected search */}
            <div className="option" onClick={() => setEditable(active?.name ?? '')}>
              Edit filter name
            </div>
            <div
              className="option"
              onClick={() => navigate(`/search?modify=${active?.id}&name=${active?.name}`)}
            >
              Modify this search
            </div>
          </Col>
        </TooltipMenu>
        <Tooltip place="top" id="binocs" variant="dark">
          View filter
        </Tooltip>
      </SubscriberTableContainer>
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
