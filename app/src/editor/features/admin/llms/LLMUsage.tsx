import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';

import { useLLMs } from '@/editor/store/hooks/admin';
import {
  Button,
  FieldSize,
  FlexboxTable,
  Modal,
  OptionItem,
  Row,
  Select,
  Show,
  useModal,
  type ILLMModel,
} from '@/shared';

import { getUsageColumns } from './constants';
import {
  type ILLMMigrationCheckModel,
  type ILLMUsageItemModel,
  type ILLMUsageModel,
} from './interfaces';

export interface ILLMUsageProps {
  /** The LLM connection. */
  llmId: number;
}

const getRowId = (item?: ILLMUsageItemModel) => `${item?.type}-${item?.sectionId ?? item?.id ?? 0}`;

const getItemPath = (item: ILLMUsageItemModel) => {
  switch (item.type) {
    case 'ReportSection':
      return `/admin/reports/${item.id}`;
    case 'AutomationProfile':
      return `/admin/automations/${item.id}`;
    case 'ContentAnalysis':
      return '/admin/content-analysis';
    case 'Search':
      return '/admin/filters';
  }
};

/**
 * Lists what uses an LLM connection, and moves all of it to another connection.
 */
export const LLMUsage: React.FC<ILLMUsageProps> = ({ llmId }) => {
  const [, api] = useLLMs();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const [usage, setUsage] = React.useState<ILLMUsageModel>();
  const [llms, setLLMs] = React.useState<ILLMModel[]>([]);
  const [targetId, setTargetId] = React.useState<number>();
  const [check, setCheck] = React.useState<ILLMMigrationCheckModel>();

  const columns = React.useMemo(() => getUsageColumns(), []);
  const items = React.useMemo(() => usage?.items ?? [], [usage]);
  const options = React.useMemo(
    () =>
      llms
        .filter((llm) => llm.id !== llmId && llm.isEnabled)
        .map((llm) => new OptionItem(llm.name, llm.id)),
    [llmId, llms],
  );
  const target = llms.find((llm) => llm.id === targetId);

  React.useEffect(() => {
    api
      .getLLMUsage(llmId)
      .then(setUsage)
      .catch(() => {});
    api
      .findAllLLMs()
      .then(setLLMs)
      .catch(() => {});
    // findAllLLMs updates the admin store, which gives useLLMs a new controller. Depending on it
    // would repeat these requests forever.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [llmId]);

  React.useEffect(() => {
    setCheck(undefined);
    if (!targetId) return;
    let isCurrent = true;
    api
      .checkLLMMigration(llmId, targetId)
      .then((result) => {
        if (isCurrent) setCheck(result);
      })
      .catch(() => {});
    return () => {
      isCurrent = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [llmId, targetId]);

  const handleMigrate = async () => {
    if (!targetId) return;
    try {
      const result = await api.migrateLLM(llmId, targetId);
      setUsage(result.usage);
      setTargetId(undefined);
      toast.success(`${result.migrated} item(s) moved to ${target?.name}.`);
    } catch {
    } finally {
      toggle();
    }
  };

  return (
    <div className="llm-usage">
      <Show visible={!!usage && items.length === 0}>
        <p>Nothing uses this connection.</p>
      </Show>
      <Show visible={items.length > 0}>
        <Row gap="1rem" alignItems="flex-end" className="field-row">
          <Select
            name="targetId"
            label="Move everything to"
            width={FieldSize.Large}
            options={options}
            value={options.find((o) => o.value === targetId) ?? ''}
            onChange={(o) => {
              setTargetId((o as OptionItem)?.value as number | undefined);
            }}
          />
          <Button disabled={!check || check.errors.length > 0} onClick={toggle}>
            Migrate {items.length} item(s)
          </Button>
        </Row>
        <Show visible={!!check?.errors.length}>
          <ul className="llm-usage-messages errors">
            {check?.errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </Show>
        <Show visible={!!check?.warnings.length}>
          <ul className="llm-usage-messages warnings">
            {check?.warnings.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </Show>
        <FlexboxTable
          rowId={getRowId}
          data={items}
          columns={columns}
          showSort={true}
          pagingEnabled={false}
          onRowClick={(row) => {
            navigate(getItemPath(row.original));
          }}
        />
      </Show>
      <Show visible={!!usage?.contentAnalysisResultCount}>
        <p>
          {usage?.contentAnalysisResultCount} past AI Analysis result(s) came from this connection.
          They record the model that produced them, so a migration does not change them.
        </p>
      </Show>
      <Modal
        headerText="Confirm Migration"
        component={
          <div>
            <p>
              Move {items.length} item(s) to {target?.name}? Report sections keep their prompts.
            </p>
            {check?.warnings.map((message) => (
              <p key={message}>{message}</p>
            ))}
          </div>
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Migrate"
        onConfirm={handleMigrate}
      />
    </div>
  );
};
