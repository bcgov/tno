import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLLMs } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, type ILLMModel, Modal, Row, useModal } from 'tno-core';

import { getColumns } from './constants';
import { LLMFilter } from './LLMFilter';
import * as styled from './styled';

const LLMList: React.FC = () => {
  const navigate = useNavigate();
  const [{ llms }, api] = useLLMs();

  const { toggle, isShowing } = useModal();

  const [items, setItems] = React.useState<ILLMModel[]>([]);
  const [removing, setRemoving] = React.useState<ILLMModel>();
  const [isBusy, setIsBusy] = React.useState(false);

  React.useEffect(() => {
    if (llms.length === 0) {
      api.findAllLLMs().then((data) => {
        setItems(data);
      });
    } else {
      setItems(llms);
    }
  }, [api, llms]);

  /** Names are unique, so the copy takes the first free '(Copy)' suffix. */
  const getCopyName = React.useCallback(
    (name: string) => {
      const names = new Set(llms.map((l) => l.name.toLocaleLowerCase()));
      let copy = `${name} (Copy)`;
      for (let i = 2; names.has(copy.toLocaleLowerCase()); i++) copy = `${name} (Copy ${i})`;
      return copy;
    },
    [llms],
  );

  const handleDuplicate = React.useCallback(
    async (llm: ILLMModel) => {
      try {
        setIsBusy(true);
        // Copy the latest saved LLM, including its API key and endpoint.
        const source = await api.getLLM(llm.id);
        const result = await api.addLLM({
          ...source,
          id: 0,
          version: 0,
          name: getCopyName(source.name),
        });
        toast.success(`${result.name} has successfully been created.`);
      } catch {
        // Ignore error as it's handled globally.
      } finally {
        setIsBusy(false);
      }
    },
    [api, getCopyName],
  );

  const columns = getColumns(
    handleDuplicate,
    (llm) => {
      setRemoving(llm);
      toggle();
    },
    isBusy,
  );

  return (
    <styled.LLMList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Configure AI language models available for report summary generation.
          </Col>
          <IconButton
            iconType="plus"
            label="Add new LLM"
            onClick={() => {
              navigate('/admin/llms/0');
            }}
          />
        </Row>
        <LLMFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                llms.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value) ||
                    i.deploymentName.toLocaleLowerCase().includes(value) ||
                    (i.agentName ?? '').toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(llms);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => {
            navigate(`${row.original.id}`);
          }}
          pagingEnabled={false}
        />
        <Modal
          headerText="Confirm Removal"
          body={`Are you sure you wish to remove the LLM '${removing?.name ?? ''}'?`}
          isShowing={isShowing}
          hide={toggle}
          type="delete"
          confirmText="Yes, Remove It"
          onConfirm={async () => {
            if (!removing) return toggle();
            try {
              setIsBusy(true);
              await api.deleteLLM(removing);
              toast.success(`${removing.name} has successfully been deleted.`);
            } catch {
              // Ignore error as it's handled globally.
            } finally {
              setIsBusy(false);
              setRemoving(undefined);
              toggle();
            }
          }}
        />
      </FormPage>
    </styled.LLMList>
  );
};

export default LLMList;
