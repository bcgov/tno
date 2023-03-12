// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { AxiosError } from 'axios';
import {
  ContentListActionName,
  IContentListModel,
  IContentModel,
  useApiMorningReports,
} from 'hooks';
import * as React from 'react';
import { toast } from 'react-toastify';
import { useContentStore } from 'store/slices';
import { Button, ButtonVariant, FieldSize, Text } from 'tno-core';

import * as styled from './styled';

interface IReportActionProps {
  setLoading: (loading: boolean) => void;
  selected: IContentModel[];
}

export const ReportActions: React.FunctionComponent<IReportActionProps> = ({
  setLoading,
  selected,
}) => {
  const morningReports = useApiMorningReports();
  const [, { updateContent, removeContent }] = useContentStore();
  const [commentary, setCommentary] = React.useState('1');
  const handleAction = async (action: ContentListActionName, name?: string, value?: string) => {
    try {
      setLoading(true);
      const model: IContentListModel = {
        action,
        actionName: name,
        actionValue: value,
        contentIds: selected.map((s) => s.id),
      };
      const res = await morningReports.updateContent(model);
      switch (action) {
        case ContentListActionName.Publish:
        case ContentListActionName.Unpublish:
        case ContentListActionName.Action:
          updateContent(
            res.data.map((i) => {
              i.isSelected = true;
              return i;
            }),
          );
          break;
        case ContentListActionName.Remove:
          removeContent(res.data);
          break;
      }
    } catch (ex: any | AxiosError) {
      toast.error(ex.message);
    } finally {
      setLoading(false);
    }
  };
  return (
    <styled.ReportActions>
      <Button
        variant={ButtonVariant.warning}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Remove)}
      >
        Remove
      </Button>
      <Button
        variant={ButtonVariant.primary}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Action, 'Front Page', 'true')}
      >
        Front Page
      </Button>
      <Button
        variant={ButtonVariant.primary}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Action, 'Top Story', 'true')}
      >
        Top Story
      </Button>
      <Text
        name="commentary"
        width={FieldSize.Tiny}
        type="number"
        value={commentary}
        onChange={(e) => setCommentary(e.currentTarget.value)}
      >
        <Button
          variant={ButtonVariant.primary}
          disabled={!selected.length}
          onClick={() => handleAction(ContentListActionName.Action, 'Commentary', commentary)}
        >
          Commentary
        </Button>
      </Text>
      <Button
        variant={ButtonVariant.success}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Publish)}
      >
        Publish
      </Button>
      <Button
        variant={ButtonVariant.secondary}
        disabled={!selected.length}
        onClick={() => handleAction(ContentListActionName.Unpublish)}
      >
        Unpublish
      </Button>
    </styled.ReportActions>
  );
};
