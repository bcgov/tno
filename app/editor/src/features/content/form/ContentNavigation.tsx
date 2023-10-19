import React from 'react';
import { FaChevronLeft, FaChevronRight, FaSpinner } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  ButtonVariant,
  getFromLocalStorage,
  IContentModel,
  Row,
  Show,
  useCombinedView,
} from 'tno-core';

import { IContentForm } from './interfaces/IContentForm';
import { getContentPath } from './utils';

export interface IContentNavigationProps {
  /** The current content id. */
  values: IContentForm;
  /** Function to fetch content. */
  fetchContent: (id: number) => void;
  /** Root path for combined view. */
  combinedPath?: string;
}

/**
 * Provides a component to navigate between content items and the list view page.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentNavigation: React.FC<IContentNavigationProps> = ({
  values,
  fetchContent,
  combinedPath,
}) => {
  const navigate = useNavigate();
  const { combined } = useCombinedView(values.contentType);

  // TODO: This won't update with the latest search results.
  const [items] = React.useState(getFromLocalStorage<IContentModel[]>('content', []));

  const indexPosition = !!values.id ? items.findIndex((c) => c.id === +values.id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (items.length ?? 0) - 1;

  return (
    <Row gap="0.15rem">
      <Show visible={!!values.id}>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Previous"
          onClick={() => {
            const id = items[indexPosition - 1]?.id;
            if (!!id) {
              navigate(getContentPath(combined, id, values.contentType, combinedPath));
            }
          }}
          disabled={!enablePrev}
        >
          <FaChevronLeft />
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Next"
          onClick={() => {
            const id = items[indexPosition + 1]?.id;
            navigate(getContentPath(combined, id, values.contentType, combinedPath));
          }}
          disabled={!enableNext}
        >
          <FaChevronRight />
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Reload"
          onClick={() => {
            fetchContent(values.id);
          }}
        >
          <FaSpinner />
        </Button>
      </Show>
    </Row>
  );
};
