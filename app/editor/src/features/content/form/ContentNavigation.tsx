import React from 'react';
import { FaChevronLeft, FaChevronRight, FaSpinner } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useApiHub, useLocalStorage } from 'store/hooks';
import { IContentSearchResult } from 'store/slices';
import {
  Button,
  ButtonVariant,
  IWorkOrderMessageModel,
  MessageTargetName,
  Row,
  Show,
  Spinner,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

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
  const hub = useApiHub();

  const [currentItems] = useLocalStorage('currentContent', null);
  const [, setCurrentItemId] = useLocalStorage('currentContentItemId', -1);

  const [isProcessing, setIsProcessing] = React.useState(
    values.workOrders.some((wo) => wo.status === WorkOrderStatusName.InProgress),
  );
  const [indexPosition, setIndexPosition] = React.useState(0);
  const [enablePrev, setEnablePrev] = React.useState(false);
  const [enableNext, setEnableNext] = React.useState(false);

  React.useEffect(() => {
    if (currentItems != null) {
      let index = !!values.id
        ? (currentItems as IContentSearchResult[]).findIndex((c) => c.id === +values.id) ?? -1
        : -1;
      setIndexPosition(index);
      setEnablePrev(index > 0);
      setEnableNext(index < ((currentItems as IContentSearchResult[]).length ?? 0) - 1);
    }
  }, [currentItems, values.id]);

  const handleNavigate = (offset: number) => {
    if (currentItems != null) {
      const targetId = (currentItems as IContentSearchResult[])[indexPosition + offset]?.id;
      if (!!targetId) {
        setCurrentItemId(targetId);
        navigate(getContentPath(targetId, values.contentType));
      }
    }
  };

  const onWorkOrder = React.useCallback(async (workOrder: IWorkOrderMessageModel) => {
    if ([WorkOrderTypeName.FFmpeg].includes(workOrder.workType)) {
      setIsProcessing(workOrder.status === WorkOrderStatusName.InProgress);
    }
  }, []);

  hub.useHubEffect(MessageTargetName.WorkOrder, onWorkOrder);

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
