// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import * as React from 'react';
import { FaHourglassHalf } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  ContentListActionName,
  ContentStatusName,
  FieldSize,
  IContentModel,
  Row,
  Show,
  Text,
} from 'tno-core';

import { IMorningReportsFilter } from '../interfaces';
import * as styled from './styled';

interface IReportActionProps {
  /** The current filter. */
  filter: IMorningReportsFilter;
  /** Whether content is loading. */
  setLoading: (loading: boolean) => void;
  /** An array of selected content. */
  selected: IContentModel[];
}

/**
 * Provides available actions that can be performed on selected content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportActions: React.FunctionComponent<IReportActionProps> = ({
  filter,
  setLoading,
  selected,
}) => {
  const [, { updateContentList }] = useContent();

  const [commentary, setCommentary] = React.useState('1');

  const handleAction = React.useCallback(
    async (action: ContentListActionName, name?: string, value?: string) => {
      try {
        setLoading(true);
        await updateContentList({
          action,
          actionName: name,
          actionValue: value,
          contentIds: selected.map((s) => s.id),
        });
      } catch (ex: any | AxiosError) {
        toast.error(ex.message);
      } finally {
        setLoading(false);
      }
    },
    [selected, setLoading, updateContentList],
  );

  return (
    <styled.ReportActions>
      <Show visible={!filter.includeHidden}>
        <Button
          variant={ButtonVariant.warning}
          disabled={
            !selected.length || !selected.every((i) => i.status === ContentStatusName.Draft)
          }
          onClick={() => handleAction(ContentListActionName.Hide)}
        >
          Hide
        </Button>
      </Show>
      <Show visible={filter.includeHidden}>
        <Button
          variant={ButtonVariant.warning}
          disabled={!selected.length}
          onClick={() => handleAction(ContentListActionName.Unhide)}
        >
          Unhide
        </Button>
      </Show>
      <Col className="separate-buttons">
        <hr />
      </Col>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Action, 'Top Story', 'true')}
      >
        Mark as Top Story
      </Button>
      <Col className="separate-buttons">
        <hr />
      </Col>
      <Row>
        <Col>
          <FaHourglassHalf className="icon" />
        </Col>
        <Text
          name="commentary"
          width={FieldSize.Tiny}
          type="number"
          value={commentary}
          onChange={(e) => setCommentary(e.currentTarget.value)}
        >
          <Button
            variant={ButtonVariant.secondary}
            disabled={!selected.length}
            onClick={() => handleAction(ContentListActionName.Action, 'Commentary', commentary)}
          >
            Mark as Commentary
          </Button>
        </Text>
      </Row>
      <Row flex="1 1 0" justifyContent="flex-end">
        <Button
          variant={ButtonVariant.success}
          disabled={!selected.length}
          onClick={() => handleAction(ContentListActionName.Publish)}
        >
          Publish
        </Button>
      </Row>
    </styled.ReportActions>
  );
};
