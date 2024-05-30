import { Action } from 'components/action';
import { Sentiment } from 'components/sentiment';
import { ReportContentInlineText } from 'features/my-reports/components';
import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import React from 'react';
import { FaGripVertical, FaX } from 'react-icons/fa6';
import { useApp } from 'store/hooks';
import { Checkbox, Col, IOptionItem, Row, Select, Text } from 'tno-core';
export interface IReportContentSectionRowProps {
  /** Whether to show the form to edit content. */
  show?: 'all' | 'summary' | 'none';
  /** index of content item in section */
  contentIndex: number;
  /** The content item. */
  row: IReportInstanceContentForm;
  /** Whether the form is disabled. */
  disabled?: boolean;
  /** Event fires when removing content. */
  onRemove?: (index: number, row: IReportInstanceContentForm) => void;
  /** Whether to show the checkbox to select the row. */
  showCheckbox?: boolean;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content: IReportInstanceContentForm) => void;
  /** Event fires when selecting the checkbox */
  onSelect?: (checked: boolean, row: IReportInstanceContentForm) => void;
  /** Whether to show the select section */
  showSelectSection?: boolean;
  /** Event fires when changing the section content belongs in */
  onChangeSection?: (sectionName: string, row: IReportInstanceContentForm) => void;
  /** An array of section options. */
  sectionOptions?: IOptionItem[];
  /** Whether to show the sort order column */
  showSortOrder?: boolean;
  /** Event fires when sort order blur occurs. */
  onBlurSortOrder?: (row: IReportInstanceContentForm) => void;
}

export const ReportContentSectionRow: React.FC<IReportContentSectionRowProps> = ({
  show: initShow = 'none',
  row,
  disabled,
  contentIndex,
  onRemove,
  showCheckbox,
  onContentClick,
  onSelect,
  showSelectSection,
  onChangeSection,
  sectionOptions,
  showSortOrder,
  onBlurSortOrder,
}) => {
  const [{ userInfo }] = useApp();

  const [sortOrder, setSortOrder] = React.useState(row.sortOrder);

  const userId = userInfo?.id ?? 0;

  React.useEffect(() => {
    setSortOrder(row.sortOrder);
  }, [row.sortOrder]);

  if (!row.content) return <></>;

  const headline = row.content.versions?.[userId]?.headline
    ? row.content.versions[userId].headline ?? ''
    : row.content.headline;
  const sentiment = row.content.tonePools?.length ? row.content.tonePools[0].value : undefined;

  return (
    <Col>
      <Row className="content-row" flex="1">
        <Col>{!disabled && <FaGripVertical className="grip-bar" />}</Col>
        {showCheckbox && (
          <Col>
            <Checkbox
              name={`chk-${row.sectionName}-${row.contentId}`}
              checked={row.selected ?? false}
              onChange={(e) => onSelect?.(e.target.checked, row)}
            />
          </Col>
        )}

        <Col className="story-sentiment">
          <Sentiment value={sentiment} showValue showIcon={false} />
        </Col>
        <Col flex="1" className="story-headline">
          <span className="link" onClick={() => onContentClick?.(row)}>
            {headline}
          </span>
        </Col>
        <Col className="story-details">
          <ReportContentInlineText row={row} />
        </Col>
        {!disabled && showSortOrder && (
          <Col className="story-sortOrder">
            <Text
              name={`instances.0.content.${row.originalIndex}.sortOrder`}
              value={sortOrder}
              width="4ch"
              className="align-right"
              onChange={(e) => setSortOrder(+e.target.value)}
              onBlur={() => onBlurSortOrder?.({ ...row, sortOrder })}
              maxLength={3}
            />
          </Col>
        )}
        {!disabled && showSelectSection && sectionOptions?.length && (
          <Col className="story-section">
            <Select
              name={`sel-${row.sectionName}-${row.contentId}`}
              placeholder="move to section"
              options={sectionOptions}
              onChange={(e) => {
                var option = e as IOptionItem;
                if (option) onChangeSection?.(`${option.value}`, row);
              }}
              isClearable
              width="25ch"
            />
          </Col>
        )}
        <Col>
          {!disabled && (
            <Action
              icon={<FaX />}
              title="remove"
              onClick={() => onRemove?.(row.originalIndex, row)}
              disabled={disabled}
              className="remove-link"
            />
          )}
        </Col>
      </Row>
    </Col>
  );
};
