import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAutomationProfiles } from 'store/hooks/admin';
import { Button, Col, FlexboxTable, IconButton, Row } from 'tno-core';

import { columns } from './constants';
import { type IAutomationProfileModel, type IAutomationRunRequestModel } from './interfaces';
import * as styled from './styled';

const defaultRunRequest: IAutomationRunRequestModel = {};

const AutomationProfileList: React.FC = () => {
  const navigate = useNavigate();
  const [, api] = useAutomationProfiles();
  const [items, setItems] = React.useState<IAutomationProfileModel[]>([]);
  const [selected, setSelected] = React.useState<IAutomationProfileModel>();

  React.useEffect(() => {
    api.findProfiles().then((profiles) => {
      // This page only works with v2 profiles; anything older is invisible here.
      setItems(profiles.filter((profile) => profile.schemaVersion >= 2));
    });
  }, [api]);

  return (
    <styled.AutomationProfileList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Configure automation profiles with shared prompts and phased steps: init runs once,
            process runs per item, complete runs after all steps.
          </Col>
          <IconButton
            iconType="plus"
            label={'Add automation profile'}
            onClick={() => {
              navigate('/admin/automations/0');
            }}
          />
        </Row>
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => {
            setSelected(row.original);
            navigate(`${row.original.id}`);
          }}
          pagingEnabled={false}
        />
        <Row className="actions">
          <Button
            disabled={!selected}
            onClick={async () => {
              if (!selected) return;
              const result = await api.runProfile(selected.id, defaultRunRequest);
              toast.success(`Automation run #${result.id} started.`);
            }}
          >
            Run Automation
          </Button>
        </Row>
      </FormPage>
    </styled.AutomationProfileList>
  );
};

export default AutomationProfileList;
