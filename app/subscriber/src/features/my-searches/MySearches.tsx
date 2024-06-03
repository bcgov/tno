import { Action } from 'components/action';
import { SubscriberTableContainer } from 'components/table';
import React from 'react';
import { FaCheck, FaSave } from 'react-icons/fa';
import { FaBookmark, FaGear, FaPen, FaRegClipboard, FaTrash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useFilters } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, IFilterModel, Modal, Row, Text, useModal } from 'tno-core';

import { truncateTeaser } from '../../components/content-list/utils/truncateTeaser';
import * as styled from './styled';

/** contains a list of the user's filters, allows for edit and viewing */
export const MySearches = () => {
  const [{ myFilters }, { findMyFilters, updateFilter, deleteFilter }] = useFilters();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();
  const [, { storeSearchFilter }] = useContent();
  const [, { storeFilter }] = useProfileStore();
  const [{ userInfo }] = useApp();

  const [active, setActive] = React.useState<IFilterModel>();
  const [editing, setEditing] = React.useState<IFilterModel>();

  React.useEffect(() => {
    if (userInfo && !myFilters.length) {
      findMyFilters().catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo]);

  const sortedMyFilters = React.useMemo(() => {
    return [...myFilters].sort((a, b) => a.name.localeCompare(b.name));
  }, [myFilters]);

  const handleSave = React.useCallback(
    (filter: IFilterModel) => {
      updateFilter(filter)
        .then((data: IFilterModel) => {
          toast.success(`${data.name} updated successfully`);
          setEditing(undefined);
        })
        .catch(() => {});
    },
    [updateFilter],
  );

  const handleClick = React.useCallback(
    (filter: IFilterModel, advanced: boolean = false) => {
      if (editing?.id !== filter.id) {
        storeFilter(filter);
        storeSearchFilter(filter.settings);
        navigate(`/search/${advanced ? `advanced/` : ''}${filter.id}`);
      }
    },
    [editing?.id, navigate, storeFilter, storeSearchFilter],
  );
  const handleCopyKeywords = (keywords: any) => {
    navigator.clipboard.writeText(keywords);
    toast.success('Keywords copied to clipboard');
  };

  return (
    <styled.MySearches>
      <SubscriberTableContainer>
        <Row className="header">
          <span className="label">Search Name</span>
        </Row>
        {sortedMyFilters.map((filter, index) => {
          const keywords = filter.settings?.search ? filter.settings.search : '';
          const truncatedKeywords = truncateTeaser(keywords, 20);
          return (
            <Row key={filter.id} className="row">
              <FaBookmark className="darker-icon link" onClick={() => handleClick(filter)} />
              <Col flex="1" className="link" onClick={() => handleClick(filter)}>
                {editing?.id === filter.id ? (
                  <Text
                    name={`filters.${index}.name`}
                    value={editing?.name ?? ''}
                    onChange={(e) => setEditing({ ...editing, name: e.target.value })}
                  >
                    <Col className="txt-btn">
                      <Action icon={<FaSave />} onClick={() => handleSave(editing)} />
                    </Col>
                  </Text>
                ) : (
                  filter.name
                )}
              </Col>
              <Col flex="1">
                {truncatedKeywords ? (
                  <Row className="keywords-row">
                    {truncatedKeywords}
                    <FaRegClipboard
                      className="copy-icon"
                      onClick={() => handleCopyKeywords(filter.settings?.search)}
                      title="Copy Keywords"
                    />
                  </Row>
                ) : null}
              </Col>
              {editing?.id === filter.id ? (
                <FaCheck onClick={() => setEditing(undefined)} />
              ) : (
                <FaPen
                  onClick={() => {
                    setEditing(filter);
                  }}
                />
              )}
              <FaGear onClick={() => handleClick(filter, true)} />
              <FaTrash
                onClick={() => {
                  setActive(filter);
                  toggle();
                }}
              />
            </Row>
          );
        })}
      </SubscriberTableContainer>
      <Modal
        headerText="Confirm Removal"
        body={`Are you sure you want to delete the "${active?.name}" filter?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          try {
            if (!!active) {
              deleteFilter(active).then(() => {
                toast.success(`${active.name} deleted successfully`);
              });
            }
          } finally {
            setActive(undefined);
            toggle();
          }
        }}
      />
    </styled.MySearches>
  );
};
