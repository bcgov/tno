// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import { getDefaultCommentaryExpiryValue } from 'features/content/form/utils';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import {
  ActionName,
  Button,
  ButtonVariant,
  Col,
  ContentListActionName,
  ContentStatusName,
  IContentModel,
  Row,
  Show,
} from 'tno-core';

import { IPaperFilter } from '../interfaces';
import * as styled from './styled';

interface IReportActionProps {
  /** The current filter. */
  filter: IPaperFilter;
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
  const [{ holidays }] = useLookup();
  const [commentary] = React.useState(`${getDefaultCommentaryExpiryValue(new Date(), holidays)}`);

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
        toast.success(
          `${selected.length} item${selected.length > 1 ? 's' : ''} marked with "${name}".`,
        );
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
        onClick={() => handleAction(ContentListActionName.Action, ActionName.TopStory, 'true')}
      >
        Top Story
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Action, ActionName.Homepage, 'true')}
      >
        Homepage
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() =>
          handleAction(ContentListActionName.Action, ActionName.Commentary, commentary)
        }
      >
        Commentary
      </Button>
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
