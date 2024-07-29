import { Action } from 'components/action';
import { Modal } from 'components/modal';
import React from 'react';
import { FaCheck, FaSave } from 'react-icons/fa';
import { FaBookmark, FaGear, FaPen, FaRegClipboard, FaTrash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useFilters } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Grid, IFilterModel, Loading, Row, Show, Text, useModal } from 'tno-core';

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
  const [loading, setLoading] = React.useState(false);
  const [active, setActive] = React.useState<IFilterModel>();
  const [editing, setEditing] = React.useState<IFilterModel>();

  React.useEffect(() => {
    if (userInfo && !myFilters.length) {
      setLoading(true);
      findMyFilters()
        .then((result) => {})
        .catch((error) => {})
        .finally(() => {
          setLoading(false);
        });
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
      <Show visible={loading}>
        <Loading />
      </Show>
      <Grid
        items={sortedMyFilters}
        renderHeader={() => [
          { name: 'name', label: '', size: '2fr' },
          { name: 'actions', label: '', size: '1fr' },
        ]}
        renderColumns={(row: IFilterModel, rowIndex) => {
          const keywords = row.settings?.search ? row.settings.search : '';
          const truncatedKeywords = truncateTeaser(keywords, 20);

          return [
            <Row key="1" flex="1" gap="1rem">
              <Action icon={<FaBookmark />} onClick={() => handleClick(row)}>
                <Col flex="1" className="link">
                  {editing?.id === row.id ? (
                    <Text
                      name={`filters.${rowIndex}.name`}
                      value={editing?.name ?? ''}
                      onChange={(e) => setEditing({ ...editing, name: e.target.value })}
                    >
                      <Col className="txt-button">
                        <Action icon={<FaSave />} onClick={() => handleSave(editing)} />
                      </Col>
                    </Text>
                  ) : (
                    row.name
                  )}
                </Col>
              </Action>
            </Row>,
            <Row key="2" justifyContent="flex-end" flex="1" gap="1rem">
              <Col flex="1">
                {truncatedKeywords ? (
                  <Row className="keywords-row">
                    {truncatedKeywords}
                    <Action
                      icon={<FaRegClipboard />}
                      title="Copy Keywords"
                      className="copy-icon"
                      onClick={() => handleCopyKeywords(row.settings?.search)}
                    />
                  </Row>
                ) : null}
              </Col>
              {editing?.id === row.id ? (
                <Action icon={<FaCheck />} onClick={() => setEditing(undefined)} />
              ) : (
                <Action
                  icon={<FaPen />}
                  onClick={() => {
                    setEditing(row);
                  }}
                />
              )}
              <Action icon={<FaGear />} onClick={() => handleClick(row, true)} />
              <Action
                icon={<FaTrash />}
                onClick={() => {
                  setActive(row);
                  toggle();
                }}
              />
            </Row>,
          ];
        }}
      />
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
