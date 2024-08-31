import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useIngests } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import {
  CellCheckbox,
  CellDate,
  CellEllipsis,
  Col,
  Grid,
  IconButton,
  IIngestModel,
  Row,
} from 'tno-core';

import { IngestFilter } from './IngestFilter';
import * as styled from './styled';
import { getStatus } from './utils';
import { sortData } from './utils/sortData';

interface IIngestListProps {}

const IngestList: React.FC<IIngestListProps> = (props) => {
  const navigate = useNavigate();
  const [{ ingests }, { findAllIngests, setIngestEnabledStatus }] = useIngests();
  const [{ ingestFilter }] = useAdminStore();
  const [isReady, setIsReady] = React.useState(false);
  const [items, setItems] = React.useState<IIngestModel[]>([]);

  React.useEffect(() => {
    if (!ingests.length && !isReady) {
      setIsReady(true);
      findAllIngests()
        .then((data) => {
          setItems(
            data.filter(
              (i) =>
                i.name.toLocaleLowerCase().includes(ingestFilter) ||
                i.source?.code.toLocaleLowerCase().includes(ingestFilter) ||
                i.description.toLocaleLowerCase().includes(ingestFilter) ||
                i.ingestType?.name.toLocaleLowerCase().includes(ingestFilter) ||
                getStatus(i).toLocaleLowerCase().includes(ingestFilter),
            ),
          );
        })
        .catch(() => {});
    } else {
      setItems(
        ingests.filter(
          (i) =>
            i.name.toLocaleLowerCase().includes(ingestFilter) ||
            i.source?.code.toLocaleLowerCase().includes(ingestFilter) ||
            i.description.toLocaleLowerCase().includes(ingestFilter) ||
            i.ingestType?.name.toLocaleLowerCase().includes(ingestFilter) ||
            getStatus(i).toLocaleLowerCase().includes(ingestFilter),
        ),
      );
    }
  }, [findAllIngests, isReady, ingests, ingestFilter]);

  const handleOnSorting = (column: any, direction: any) => {
    if (direction && column.name) {
      setItems((items) => {
        switch (column.name) {
          case 'source.code':
            return [...items].sort((a: any, b: any) =>
              sortData(a['source']['code'], b['source']['code'], direction),
            );
          case 'ingestType.name':
            return [...items].sort((a: any, b: any) =>
              sortData(a['ingestType']['name'], b['ingestType']['name'], direction),
            );
          case 'status':
            return [...items].sort((a: any, b: any) =>
              sortData(getStatus(a), getStatus(b), direction),
            );
          case 'isEnabled':
            return [...items].sort((a: any, b: any) =>
              sortData(a[column.name!] as boolean, b[column.name!] as boolean, direction),
            );
          default:
            return [...items].sort((a: any, b: any) =>
              sortData(a[column.name!] ?? '', b[column.name!] ?? '', direction),
            );
        }
      });
    } else {
      setItems(ingests);
    }
  };

  const handleEnabledClicked = (event: any, row: any) => {
    setIngestEnabledStatus(row.id, !row.isEnabled).catch(() => {});
    toast.success(`Ingest [${row.name}] is now ` + (row.isEnabled ? `disabled` : `enabled`));
  };

  return (
    <styled.IngestList>
      <Row justifyContent="flex-end">
        <Col flex="1 1 0">
          Manage all ingest services. These services run in the background and upload content from
          external data sources.
        </Col>
        <IconButton
          iconType="plus"
          label="Add New Ingest"
          onClick={() => navigate('/admin/ingests/0')}
        />
      </Row>
      <IngestFilter />
      <Grid
        items={items}
        showPaging={false}
        onSortChange={(column, direction) => handleOnSorting(column, direction)}
        renderHeader={() => [
          { name: 'name', label: 'Name', size: '23%', sortable: true },
          { name: 'source.code', label: 'Source', size: '10%', sortable: true },
          { name: 'description', label: 'Description', size: '25%', sortable: true },
          { name: 'ingestType.name', label: 'Type', size: '10%', sortable: true },
          { name: 'status', label: 'Status', size: '8%', sortable: true },
          { name: 'lastRanOn', label: 'Last Run', size: '18%', sortable: true },
          { name: 'isEnabled', label: 'Enabled', sortable: true },
        ]}
        renderColumns={(row: IIngestModel, rowIndex) => {
          return [
            {
              column: (
                <div key="1" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.name}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="2" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.source?.code}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="3" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.description}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="4" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{row.ingestType?.name}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="5" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellEllipsis key="">{getStatus(row)}</CellEllipsis>
                </div>
              ),
            },
            {
              column: (
                <div key="6" className="clickable" onClick={() => navigate(`${row.id}`)}>
                  <CellDate value={row.lastRanOn} />
                </div>
              ),
            },
            {
              column: (
                <div
                  key="7"
                  className="clickable"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleEnabledClicked(e, row);
                  }}
                >
                  <CellCheckbox key="8" checked={row.isEnabled} />
                </div>
              ),
            },
          ];
        }}
      />
    </styled.IngestList>
  );
};

export default IngestList;
