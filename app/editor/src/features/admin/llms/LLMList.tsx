import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useLLMs } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, type ILLMModel, Row } from 'tno-core';

import { columns } from './constants';
import { LLMFilter } from './LLMFilter';
import * as styled from './styled';

const LLMList: React.FC = () => {
  const navigate = useNavigate();
  const [{ llms }, api] = useLLMs();

  const [items, setItems] = React.useState<ILLMModel[]>([]);

  React.useEffect(() => {
    if (llms.length === 0) {
      api.findAllLLMs().then((data) => {
        setItems(data);
      });
    } else {
      setItems(llms);
    }
  }, [api, llms]);

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
      </FormPage>
    </styled.LLMList>
  );
};

export default LLMList;
