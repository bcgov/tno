import { Action } from 'components/action';
import { Sentiment } from 'components/sentiment';
import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import moment from 'moment';
import React from 'react';
import { FaCheck, FaGripLines, FaPen, FaX } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Col, Row } from 'tno-core';

import { ContentForm, UserContentForm } from '.';

export interface IReportContentSectionRowProps {
  /** Whether to show the form to edit content. */
  show?: 'all' | 'summary' | 'none';
  /** Event fires when removing content. */
  onRemove?: (index: number, row: IReportInstanceContentForm) => void;
  /** index of content item in section */
  index: number;
  /** The content item. */
  row: IReportInstanceContentForm;
  /** Whether the form is disabled. */
  disabled?: boolean;
}

export const ReportContentSectionRow: React.FC<IReportContentSectionRowProps> = ({
  show: initShow = 'none',
  row,
  disabled,
  index,
  onRemove,
}) => {
  const [{ userInfo }] = useApp();

  const [show, setShow] = React.useState(initShow);

  const userId = userInfo?.id ?? 0;

  if (!row.content) return <></>;

  const headline = row.content.versions?.[userId]?.headline
    ? row.content.versions[userId].headline ?? ''
    : row.content.headline;
  const sentiment = row.content.tonePools?.length ? row.content.tonePools[0].value : undefined;

  return (
    <Col>
      <Row className="content-row" flex="1">
        <Col>{!disabled && <FaGripLines />}</Col>
        {/* <Col>
          <FormikCheckbox
            name={`sections.${index}.content.${contentInSectionIndex}.selected`}
          />
        </Col> */}
        <Col>
          <Sentiment value={sentiment} />
        </Col>
        <Col>
          {row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}
        </Col>
        <Col flex="1">
          <Link to={`/view/${row.contentId}`} target="blank">
            {headline}
          </Link>
        </Col>
        <Col>{row.content.page ? `(P.${row.content.page})` : ''}</Col>
        <Row gap="1rem">
          {!disabled && (
            <>
              <Action
                icon={show === 'none' ? <FaPen /> : <FaCheck />}
                title="edit"
                onClick={() => {
                  setShow(show === 'none' ? 'all' : 'none');
                }}
                disabled={disabled}
              />
              <Action
                icon={<FaX />}
                title="remove"
                onClick={() => onRemove?.(row.originalIndex, row)}
                disabled={disabled}
              />
            </>
          )}
        </Row>
      </Row>
      {row.content.ownerId === userId && row.content.isPrivate ? (
        <UserContentForm index={row.originalIndex} show={show} />
      ) : (
        <ContentForm index={row.originalIndex} show={show} />
      )}
    </Col>
  );
};
