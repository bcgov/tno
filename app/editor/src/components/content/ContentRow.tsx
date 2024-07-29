import moment from 'moment';
import React from 'react';
import { FaGripVertical } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { Checkbox, Link, Sentiment, Show, Text } from 'tno-core';

import { IContentRowModel } from './interfaces';
import * as styled from './styled';

export interface IContentRowProps {
  /** Route to navigate when headline clicked. */
  to?: string;
  /** Content to be displayed. */
  row: IContentRowModel;
  /** Custom actions displayed in column. */
  actions?: React.ReactNode;
  /** Whether to show the grip column. */
  showGrip?: boolean;
  /** Whether to show the checkbox for selecting rows. */
  showCheckbox?: boolean;
  /** Whether to show the sort order, which allows manual editing. */
  showSortOrder?: boolean;
  /** Remove content item event. */
  onRemove?: (content: IContentRowModel) => void;
  /** Row data has been changed. */
  onChange?: (content: IContentRowModel) => void;
  /** Event fires when a row selection changes. */
  onSelected?: (content: IContentRowModel) => void;
}

/**
 * Component provides a row display content information.
 * Provides a way to click and navigate on the headline.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentRow: React.FC<IContentRowProps> = ({
  showGrip = false,
  showCheckbox = false,
  showSortOrder = false,
  actions,
  row,
  to,
  onRemove,
  onChange,
  onSelected,
}) => {
  const [sortOrder, setSortOrder] = React.useState(row.sortOrder);

  React.useEffect(() => {
    setSortOrder(row.sortOrder);
  }, [row.sortOrder]);

  return (
    <styled.ContentRow className="content-row">
      <Show visible={showGrip}>
        <div className="row-grip">
          <FaGripVertical />
        </div>
      </Show>
      <Show visible={showCheckbox}>
        <div className="row-selector">
          <Checkbox
            name={`chk-row-${row.content.id}`}
            checked={row.selected}
            onChange={(e) => {
              onSelected?.({ ...row, selected: e.target.checked });
            }}
          />
        </div>
      </Show>
      {to ? (
        <Link className="headline-link" to={to}>
          {row.content.headline}
        </Link>
      ) : (
        <div className="headline">{row.content.headline}</div>
      )}
      <div className="byline">
        {row.content.byline ? row.content.byline : row.content.contributor?.name}
      </div>
      <div className="section-page">
        {row.content.section}
        {row.content.page ? `:${row.content.page}` : ''}
      </div>
      <div className="other-source">{row.content.otherSource}</div>
      <div className="published-on">
        {row.content.publishedOn ? moment(row.content.publishedOn).format('yyyy-MM-DD') : ''}
      </div>
      <Sentiment
        value={row.content.tonePools?.[0]?.value}
        title={`${row.content.tonePools?.[0]?.value ?? ''}`}
      />
      <Show visible={showSortOrder}>
        <div className="sort-order">
          <Text
            className="frm-in number"
            name={`sortOrder-${row.content.id}`}
            value={sortOrder}
            width="5ch"
            onClick={(e) => e.currentTarget.select()}
            onChange={(e) => {
              const value = Number(e.target.value);
              const sortOrder = isNaN(value) ? row.sortOrder : value;
              setSortOrder(sortOrder);
            }}
            onBlur={(e) => {
              onChange?.({ ...row, sortOrder });
            }}
          />
        </div>
      </Show>
      <Show visible={!!actions}>
        <div className="actions">{actions}</div>
      </Show>
      <Show visible={!!onRemove && !!row.content}>
        <div className="delete">
          <FaX className="button button-link red" onClick={() => onRemove?.(row)} title="Remove" />
        </div>
      </Show>
    </styled.ContentRow>
  );
};
