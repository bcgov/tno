import * as React from 'react';
import { FaArrowLeft } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useFolders } from 'store/hooks';
import {
  Button,
  Col,
  FieldSize,
  getDistinct,
  IContentModel,
  IFilterModel,
  IFolderContentModel,
  IFolderModel,
  IOptionItem,
  Row,
  Select,
  Show,
  sortObject,
} from 'tno-core';

import { getFilterOptions } from './constants';
import { Schedule } from './Schedule';
import * as styled from './styled';
import { createSchedule } from './utils';

export const ConfigureFolder: React.FC = () => {
  const [{ myFilters }, { findMyFilters }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();
  const [, { getFolder, updateFolder }] = useFolders();
  const { id } = useParams();
  const navigate = useNavigate();

  const [active, setActive] = React.useState<IFilterModel>();
  const [currentFolder, setCurrentFolder] = React.useState<IFolderModel>();
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getFilterOptions(myFilters, active?.id ?? 0),
  );

  React.useEffect(() => {
    if (!myFilters.length) {
      findMyFilters().then((data) => {
        setFilterOptions(getFilterOptions(data, active?.id ?? 0));
      });
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (!currentFolder && id) {
      getFolder(Number(id)).then((data) => {
        setCurrentFolder(data);
      });
    }
  }, [currentFolder, getFolder, id]);

  const handleRun = React.useCallback(
    async (filter: IFilterModel) => {
      if (!currentFolder) return;
      try {
        const results: any = await findContentWithElasticsearch(
          filter.query,
          filter.settings.searchUnpublished,
        );
        if (results.hits.hits.length) {
          const existing = [...currentFolder.content].sort(sortObject((c) => c.sortOrder));
          const maxSortOrder = existing.length ? existing[existing.length - 1].sortOrder : 0;
          const content: IFolderContentModel[] = getDistinct(
            [
              ...existing,
              ...results.hits.hits.map((h: { _source: IContentModel }, index: number) => ({
                contentId: h._source.id,
                sortOrder: maxSortOrder + index,
                content: h._source,
                selected: false,
              })),
            ],
            (item) => item.content.id,
          ).map((item, index) => ({ ...item, sortOrder: index }));
          updateFolder({ ...currentFolder, content });
          toast.success(`Filter found and added ${results.hits.hits.length} content items.`);
        } else {
          toast.warning('No content found for this filter.');
        }
      } catch {}
    },
    [findContentWithElasticsearch, currentFolder, updateFolder],
  );

  return (
    <styled.ConfigureFolder
      ignoreLastChildGap
      header={
        <Row width={FieldSize.Stretch}>
          <span className="back-to-folders" onClick={() => navigate(-1)}>
            <FaArrowLeft /> Back to folders
          </span>
          <span className="name">Configuring folder: "{currentFolder?.name}"</span>
        </Row>
      }
    >
      <div className="main-container">
        <div className="add-filter">
          <div className="choose-text">Choose a filter to apply to this folder.</div>
          <Row className="choose-filter">
            <Select
              options={filterOptions}
              name="filters"
              isClearable
              className="filter-select"
              value={filterOptions.find((option) => option.value === active?.id)}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                setActive(myFilters.find((f) => f.id === option.value));
              }}
            />
            <Button className="run" onClick={() => !!active && handleRun(active)}>
              Run
            </Button>
          </Row>
        </div>
        <Col className="add-schedule">
          <Row>
            <span className="schedule-text">
              Configure when you would like to have content removed automatically.
            </span>
            <Button
              className="add-schedule-btn"
              onClick={() =>
                currentFolder &&
                setCurrentFolder({
                  ...currentFolder,
                  events: [createSchedule(currentFolder.name, currentFolder.description)],
                })
              }
            >
              Add Clearance Schedule
            </Button>
          </Row>
          <Schedule setCurrentFolder={setCurrentFolder} folderEvents={currentFolder?.events} />
          <Show visible={!!currentFolder?.events?.length}>
            <Button
              onClick={() => {
                updateFolder(currentFolder as IFolderModel);
              }}
            >
              Save
            </Button>
          </Show>
        </Col>
      </div>
    </styled.ConfigureFolder>
  );
};
