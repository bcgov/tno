// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import { getDefaultCommentaryExpiryValue } from 'features/content/form/utils';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useContentStore } from 'store/slices';
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
  const [{ content }, { updateContentList }] = useContent();
  const [, { removeContent }] = useContentStore();
  const [{ holidays }] = useLookup();

  const [commentary] = React.useState(`${getDefaultCommentaryExpiryValue(new Date(), holidays)}`);

  const handleAction = React.useCallback(
    async (action: ContentListActionName, name?: string, value?: string) => {
      try {
        setLoading(true);
        const items = await updateContentList({
          action,
          actionName: name,
          actionValue: value,
          contentIds: selected.length
            ? selected.map((s) => s.id)
            : content?.items.map((c) => c.id) ?? [],
        });
        if (value === 'false' || !value) {
          removeContent(items);
        }
        toast.success(`${items.length} item${items.length > 1 ? 's' : ''} updated.`);
      } catch (ex: any | AxiosError) {
        toast.error(ex.message);
      } finally {
        setLoading(false);
      }
    },
    [setLoading, updateContentList, selected, content?.items, removeContent],
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
        onClick={() =>
          handleAction(
            ContentListActionName.Action,
            ActionName.TopStory,
            filter.topStory ? 'false' : 'true',
          )
        }
      >
        {filter.topStory ? `Remove from` : `Add to`} Top Story
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() =>
          handleAction(
            ContentListActionName.Action,
            ActionName.Homepage,
            filter.homepage ? 'false' : 'true',
          )
        }
      >
        {filter.homepage ? `Remove from` : `Add to`} Homepage
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() =>
          handleAction(
            ContentListActionName.Action,
            ActionName.Commentary,
            filter.commentary ? `` : commentary,
          )
        }
      >
        {filter.commentary ? `Remove from` : `Add to`} Commentary
      </Button>
      <Row flex="1 1 0" justifyContent="flex-end">
        <Button
          variant={ButtonVariant.success}
          disabled={!selected.length}
          onClick={() => handleAction(ContentListActionName.Publish)}
        >
          Publish Selected
        </Button>
        <Show visible={filter.commentary || filter.topStory || filter.homepage}>
          <Button
            variant={ButtonVariant.success}
            onClick={() => handleAction(ContentListActionName.Publish)}
          >
            Publish All
          </Button>
        </Show>
      </Row>
    </styled.ReportActions>
  );
};
