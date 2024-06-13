// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import { getDefaultCommentaryExpiryValue } from 'features/content/form/utils';
import { IContentListFilter } from 'features/content/interfaces';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useContent, useLookup, useSettings } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  ContentListActionName,
  ContentStatusName,
  IContentModel,
  IPage,
  Row,
  Show,
} from 'tno-core';

import * as styled from './styled';

interface IReportActionProps {
  /** The current filter. */
  filter: IContentListFilter;
  /** Whether content is loading. */
  setLoading: (loading: boolean) => void;
  onContentHidden: (contentToHide: IContentModel[]) => void;
  /** An array of selected content. */
  selected: IContentSearchResult[];
  searchResults: IPage<IContentSearchResult>;
}

/**
 * Provides available actions that can be performed on selected content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportActions: React.FunctionComponent<IReportActionProps> = ({
  filter,
  setLoading,
  onContentHidden,
  selected,
  searchResults,
}) => {
  const [, { updateContentList }] = useContent();

  const [{ holidays }] = useLookup();
  const { commentaryActionId, featuredStoryActionId, topStoryActionId } = useSettings();

  const [commentary] = React.useState(`${getDefaultCommentaryExpiryValue(new Date(), holidays)}`);

  const handleAction = React.useCallback(
    async (action: ContentListActionName, actionId?: number, value?: string) => {
      try {
        setLoading(true);
        const items = await updateContentList({
          action,
          actionId: actionId,
          actionValue: value,
          contentIds: selected.length
            ? selected.map((s) => s.id)
            : searchResults?.items.map((c) => c.id) ?? [],
        });
        if (action === ContentListActionName.Hide && (value === 'false' || !value)) {
          onContentHidden(items);
        }
        toast.success(`${items.length} item${items.length > 1 ? 's' : ''} updated.`);
      } catch (ex: any | AxiosError) {
        toast.error(ex.message);
      } finally {
        setLoading(false);
      }
    },
    [setLoading, updateContentList, selected, searchResults?.items, onContentHidden],
  );

  const handleToggleAction = React.useCallback(
    (action: string) => {
      let actionId;
      switch (action) {
        case 'Commentary':
          actionId = commentaryActionId;
          break;
        case 'Top Story':
          actionId = topStoryActionId;
          break;
        case 'Featured Story':
          actionId = featuredStoryActionId;
          break;
        case 'Published Selected':
          break;
        default:
          return;
      }

      if (actionId) {
        handleAction(
          ContentListActionName.Action,
          actionId,
          actionId === commentaryActionId ? '3' : 'true',
        );
      } else {
        handleAction(ContentListActionName.Publish);
      }
    },
    [commentaryActionId, featuredStoryActionId, handleAction, topStoryActionId],
  );

  const handleKeyDown = React.useCallback(
    (e: KeyboardEvent) => {
      if (e.ctrlKey) {
        e.preventDefault();
        switch (e.code) {
          case 'KeyC':
            handleToggleAction('Commentary');
            break;
          case 'KeyO':
            handleToggleAction('Top Story');
            break;
          case 'KeyF':
            handleToggleAction('Featured Story');
            break;
          case 'KeyP':
            handleToggleAction('Published Selected');
            break;
          default:
            break;
        }
      }
    },
    [handleToggleAction],
  );

  React.useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  return (
    <styled.ReportActions>
      <Show visible={!filter.isHidden}>
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
      <Show visible={!!filter.isHidden}>
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
            topStoryActionId,
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
            featuredStoryActionId,
            filter.featuredStory ? 'false' : 'true',
          )
        }
      >
        {filter.featuredStory ? `Remove from` : `Add to`} Featured Stories
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() =>
          handleAction(
            ContentListActionName.Action,
            commentaryActionId,
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
        <Show visible={filter.commentary || filter.topStory || filter.featuredStory}>
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
