import React from 'react';
import { FaChevronLeft, FaChevronRight, FaSpinner } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useLocalStorage } from 'store/hooks';
import { Button, ButtonVariant, Row, Show, Spinner } from 'tno-core';

import { useContentForm } from './hooks';
import { IContentForm } from './interfaces/IContentForm';
import { getContentPath } from './utils';

export interface IContentNavigationProps {
  /** The current content id. */
  values: IContentForm;
  /** Function to fetch content. */
  fetchContent: (id: number) => void;
  /** Whether to show the refresh button */
  showRefresh?: boolean;
}

/**
 * Provides a component to navigate between content items and the list view page.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentNavigation: React.FC<IContentNavigationProps> = ({
  values,
  fetchContent,
  showRefresh = true,
}) => {
  const navigate = useNavigate();
  const { isProcessing } = useContentForm(values);

  const [currentItems] = useLocalStorage<number[] | null>('currentContent', null);
  const [, setCurrentItemId] = useLocalStorage('currentContentItemId', -1);

  const [indexPosition, setIndexPosition] = React.useState(0);
  const [enablePrev, setEnablePrev] = React.useState(false);
  const [enableNext, setEnableNext] = React.useState(false);

  React.useEffect(() => {
    if (currentItems != null) {
      let index = !!values.id ? currentItems.findIndex((c) => c === +values.id) ?? -1 : -1;
      setIndexPosition(index);
      setEnablePrev(index > 0);
      setEnableNext(index < (currentItems.length ?? 0) - 1);
    }
  }, [currentItems, values.id]);

  const handleNavigate = (offset: number) => {
    if (currentItems != null) {
      const targetId = currentItems[indexPosition + offset];
      if (!!targetId) {
        setCurrentItemId(targetId);
        navigate(getContentPath(targetId, values.contentType));
      }
    }
  };

  return (
    <Row gap="0.15rem">
      <Show visible={!!values.id}>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Previous"
          onClick={() => handleNavigate(-1)}
          disabled={!enablePrev}
        >
          <FaChevronLeft />
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Next"
          onClick={() => handleNavigate(+1)}
          disabled={!enableNext}
        >
          <FaChevronRight />
        </Button>
        {showRefresh && (
          <Button
            variant={ButtonVariant.secondary}
            tooltip="Reload"
            onClick={() => {
              fetchContent(values.id);
            }}
          >
            {isProcessing ? <Spinner size="10px" /> : <FaSpinner />}
          </Button>
        )}
      </Show>
    </Row>
  );
};
