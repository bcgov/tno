import { FormPage } from 'components/formpage';
import _ from 'lodash';
import moment from 'moment';
import React from 'react';
import { FaEdit } from 'react-icons/fa';
import { FaBugSlash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useApiHub, useLookupOptions } from 'store/hooks';
import { useIngests, useIngestTypes } from 'store/hooks/admin';
import {
  Checkbox,
  Col,
  IIngestMessageModel,
  IIngestModel,
  MessageTargetName,
  Row,
  Section,
  Select,
} from 'tno-core';

import { getStatus } from '../ingests/utils';
import { useIngestIcon } from './hooks';
import * as styled from './styled';
import { getClassName } from './utils';

const groupIngests = (ingests: IIngestModel[], typeId?: number) => {
  return _.groupBy(
    _.sortBy(
      ingests.filter((i) => !typeId || i.ingestTypeId === typeId),
      (i) => i.name,
    ),
    (i) => i.ingestTypeId,
  );
};

export const Dashboard: React.FC = () => {
  const [
    { ingests },
    { findAllIngests, storeIngest, setIngestEnabledStatus, resetIngestFailures },
  ] = useIngests();
  const [, { findAllIngestTypes }] = useIngestTypes();
  const [{ ingestTypeOptions }] = useLookupOptions();
  const navigate = useNavigate();
  const getIngestIcon = useIngestIcon();
  const hub = useApiHub();

  const [showAll, setShowAll] = React.useState(false);
  const [type, setType] = React.useState('');
  const [groups, setGroups] = React.useState(groupIngests(ingests));

  React.useEffect(() => {
    if (!ingests.length) findAllIngests().catch(() => {});
    if (!ingestTypeOptions.length) findAllIngestTypes().catch(() => {});
    // Only on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setGroups(groupIngests(ingests, +type));
  }, [ingests, type]);

  React.useEffect(() => {
    // Join the room for editor messages.
    hub.send('joinRoom', ['editor']);
    return () => {
      hub.send('leaveRoom', ['editor']);
    };
    // Only do this when state changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hub.state]);

  React.useEffect(() => {
    // Refresh the status of each ingest.
    const intervalId = setInterval(() => {
      setGroups(groupIngests(ingests, +type));
    }, 30000);
    return () => {
      clearInterval(intervalId);
    };
  }, [ingests, type]);

  hub.useHubEffect(MessageTargetName.IngestUpdated, (message: IIngestMessageModel) => {
    // Update the ingest status.
    storeIngest(
      ingests.map((i) =>
        i.id === message.id
          ? { ...i, lastRanOn: message.lastRanOn, isEnabled: message.isEnabled }
          : i,
      ),
    );
  });

  const handleStatus = React.useCallback(
    async (id: number, status: boolean) => {
      try {
        await setIngestEnabledStatus(id, status);
      } catch {}
    },
    [setIngestEnabledStatus],
  );

  const handleReset = React.useCallback(
    async (id: number) => {
      try {
        await resetIngestFailures(id);
      } catch {}
    },
    [resetIngestFailures],
  );

  return (
    <styled.Dashboard>
      <FormPage>
        <h1>Dashboard</h1>
        <Row className="filter">
          <h2>Ingest Services</h2>
          <Select
            name="ingestType"
            options={ingestTypeOptions}
            width="20ch"
            placeholder="Select Type"
            value={ingestTypeOptions.find((t) => t.value === type) ?? ''}
            onChange={(newValue: any) => {
              setType(newValue?.value ?? '');
            }}
          />
          <Checkbox
            name="showAll"
            label="Show All"
            checked={showAll}
            onChange={(e) => setShowAll(e.target.checked)}
          />
        </Row>
        <Row>
          {Object.entries(groups)
            .filter((g) => showAll || g[1].some((ingest) => ingest.isEnabled))
            .map((g) => (
              <React.Fragment key={g[0]}>
                {g[1]
                  .filter((ingest) => showAll || ingest.isEnabled)
                  .map((ingest) => (
                    <Section
                      key={ingest.id}
                      className={`ingest ${getClassName(getStatus(ingest))}`}
                    >
                      <Row nowrap>
                        {getIngestIcon(ingest)}
                        <h3>{ingest.name}</h3>
                        <div>
                          <FaEdit
                            title="Edit"
                            className="btn-link"
                            onClick={() => navigate(`/admin/ingests/${ingest.id}`)}
                          />
                        </div>
                        <div>
                          <FaBugSlash
                            title="Reset"
                            className="btn-link"
                            onClick={() => handleReset(ingest.id)}
                          />
                        </div>
                      </Row>
                      <Col className="lastRan">
                        {ingest.lastRanOn
                          ? moment(ingest.lastRanOn).format('YYYY-MM-DD hh:mm:ss')
                          : ''}
                      </Col>
                      <Row nowrap alignItems="center">
                        <Col>{getStatus(ingest)}</Col>
                        <Col>
                          <Checkbox
                            name={`chkIsEnabled-${ingest.id}`}
                            label="Enabled"
                            checked={ingest.isEnabled}
                            onChange={(e) => handleStatus(ingest.id, e.target.checked)}
                          />
                        </Col>
                      </Row>
                    </Section>
                  ))}
              </React.Fragment>
            ))}
        </Row>
      </FormPage>
    </styled.Dashboard>
  );
};

export default Dashboard;
