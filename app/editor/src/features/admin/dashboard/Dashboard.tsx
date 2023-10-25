import _ from 'lodash';
import moment from 'moment';
import React from 'react';
import { FaEdit } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useLookupOptions } from 'store/hooks';
import { useIngests, useIngestTypes } from 'store/hooks/admin';
import { Checkbox, Col, FormPage, IIngestModel, Row, Section, Select } from 'tno-core';

import { getStatus, isRunning } from '../ingests/utils';
import { useIngestIcon } from './hooks';
import * as styled from './styled';

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
  const [{ ingests }, { findAllIngests }] = useIngests();
  const [, { findAllIngestTypes }] = useIngestTypes();
  const [{ ingestTypeOptions }] = useLookupOptions();
  const navigate = useNavigate();
  const getIngestIcon = useIngestIcon();

  const [showAll, setShowAll] = React.useState(false);
  const [type, setType] = React.useState('');
  const [groups, setGroups] = React.useState(groupIngests(ingests));

  React.useEffect(() => {
    findAllIngests().catch(() => {});
    findAllIngestTypes().catch(() => {});
    // Only on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setGroups(groupIngests(ingests, +type));
  }, [ingests, type]);

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
            .filter((g) => showAll || g[1].some((i) => i.isEnabled))
            .map((g) => (
              <React.Fragment key={g[0]}>
                {g[1]
                  .filter((i) => showAll || i.isEnabled)
                  .map((i) => (
                    <Section
                      key={i.id}
                      className={`ingest${!i.isEnabled ? ' disabled' : ''}${
                        isRunning(i) ? ' running' : ' warning'
                      }${i.retryLimit <= i.failedAttempts ? ' error' : ''}`}
                    >
                      <Row nowrap>
                        {getIngestIcon(i)}
                        <h3>{i.name}</h3>
                        <div>
                          <FaEdit
                            title="Edit"
                            className="btn-link"
                            onClick={() => navigate(`/admin/ingests/${i.id}`)}
                          />
                        </div>
                      </Row>
                      <Col className="lastRan">
                        {i.lastRanOn ? moment(i.lastRanOn).format('YYYY-MM-DD hh:mm:ss') : ''}
                      </Col>
                      <div>{getStatus(i)}</div>
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
