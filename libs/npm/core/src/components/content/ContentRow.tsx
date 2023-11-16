import moment from 'moment';
import React from 'react';
import { FaGripVertical } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';

import { Checkbox, Link, Sentiment, Show, Text } from '..';
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
        <div>
          <FaGripVertical />
        </div>
      </Show>
      <Show visible={showCheckbox}>
        <div>
          <Checkbox
            name={`chk-row-${row.content.id}`}
            checked={row.selected}
            onChange={(e) => {
              onSelected?.({ ...row, selected: e.target.checked });
            }}
          />
        </div>
      </Show>
      {to ? <Link to={to}>{row.content.headline}</Link> : <div>{row.content.headline}</div>}
      <div>{row.content.byline ? row.content.byline : row.content.contributor?.name}</div>
      <div>
        {row.content.section}
        {row.content.page ? `:${row.content.page}` : ''}
      </div>
      <div>{row.content.otherSource}</div>
      <div>
        {row.content.publishedOn ? moment(row.content.publishedOn).format('yyyy-MM-DD') : ''}
      </div>
      <div>
        <Sentiment
          value={row.content.tonePools?.[0]?.value}
          title={`${row.content.tonePools?.[0]?.value ?? ''}`}
        />
      </div>
      <Show visible={showSortOrder}>
        <div>
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
        <div>{actions}</div>
      </Show>
      <Show visible={!!onRemove && !!row.content}>
        <div>
          <FaX className="btn btn-link red" onClick={() => onRemove?.(row)} title="Remove" />
        </div>
      </Show>
    </styled.ContentRow>
  );
};
