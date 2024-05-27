import { Button } from 'components/button';
import { Modal } from 'components/modal';
import * as React from 'react';
import { FaArrowAltCircleRight, FaInfoCircle } from 'react-icons/fa';
import { FaGear, FaTrash } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useContent, useFilters, useFolders } from 'store/hooks';
import {
  Checkbox,
  Col,
  FieldSize,
  getDistinct,
  IContentModel,
  IFilterModel,
  IFolderContentModel,
  IFolderModel,
  IFolderScheduleModel,
  IOptionItem,
  Row,
  Select,
  sortObject,
  Text,
  useModal,
} from 'tno-core';

import { getFilterOptions } from './constants';
import { useFolderContext } from './FolderContext';
import { Schedule } from './Schedule';
import * as styled from './styled';
import { createSchedule } from './utils';

export interface IConfigureFolderProps {}

export const ConfigureFolder: React.FC<IConfigureFolderProps> = () => {
  const [, { findContentWithElasticsearch }] = useContent();
  const [{ myFilters }, { findMyFilters }] = useFilters();
  const [{ myFolders }, { findMyFolders, getFolder, updateFolder, deleteFolder }] = useFolders();
  const { id } = useParams();
  const { toggle: toggleEmpty, isShowing: isShowingEmpty } = useModal();
  const { toggle: toggleDelete, isShowing: isShowingDelete } = useModal();
  const navigate = useNavigate();
  const { folder: currentFolder, setFolder: setCurrentFolder } = useFolderContext();

  const [activeFilter, setActiveFilter] = React.useState<IFilterModel>();
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getFilterOptions(myFilters, activeFilter?.id ?? 0),
  );
  const [init, setInit] = React.useState(true);

  React.useEffect(() => {
    if (!myFilters.length) {
      findMyFilters().then((data) => {
        setFilterOptions(getFilterOptions(data, activeFilter?.id ?? 0));
      });
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setInit(true);
  }, [id]);

  React.useEffect(() => {
    if (init && myFolders.length && currentFolder?.id !== Number(id)) {
      setInit(false);
      getFolder(Number(id), true)
        .then((folder) => {
          setCurrentFolder(folder);
          if (folder.filterId) {
            const selectedFilter = myFilters.find((f) => f.id === folder.filterId);
            setActiveFilter(selectedFilter);
          } else {
            setActiveFilter(undefined);
          }
        })
        .catch(() => {})
        .finally(() => setInit(true));
    } else {
      if (currentFolder && currentFolder.filterId) {
        const selectedFilter = myFilters.find((f) => f.id === currentFolder.filterId);
        setActiveFilter(selectedFilter);
      } else {
        setActiveFilter(undefined);
      }
    }
    // do not want to run with setCurrentFolder
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentFolder, id, getFolder, myFolders, init]);

  React.useEffect(() => {
    if (currentFolder && !currentFolder.events.length) {
      setCurrentFolder({
        ...currentFolder,
        events: [createSchedule(currentFolder.name, currentFolder.description)],
      });
      if (currentFolder.filterId) {
        const selectedFilter = myFilters.find((f) => f.id === currentFolder.filterId);
        setActiveFilter(selectedFilter);
      }
    } else if (currentFolder && currentFolder.events.length > 1) {
      // Remove extra scheduled events that might have been created due to a bug.
      setCurrentFolder({
        ...currentFolder,
        events: [currentFolder.events[0]],
      });
    }
    // only run when currentFolder changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentFolder]);

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
            (item) => item.contentId,
          ).map((item, index) => ({ ...item, sortOrder: index }));

          // Only update the folder if a change in content has occurred.
          if (currentFolder.content.length !== content.length) {
            const result = await updateFolder({ ...currentFolder, content }, true);
            setCurrentFolder(result);
            await findMyFolders();
            toast.success(`Filter found and added ${results.hits.hits.length} content items.`);
          } else {
            toast.info(`Filter found ${results.hits.hits.length} content items.`);
          }
        } else {
          toast.warning('No content found for this filter.');
        }
      } catch {}
    },
    [currentFolder, findContentWithElasticsearch, updateFolder, setCurrentFolder, findMyFolders],
  );

  const handleSaveFolder = React.useCallback(
    async (values: IFolderModel) => {
      try {
        if (values.settings.autoPopulate && values.filterId === undefined) {
          toast.error('Filter required when Auto-populate is checked.');
          return;
        }
        const result = await updateFolder(values, false);
        setCurrentFolder(result);
        toast.success('Folder schedule saved.');
      } catch {
        toast.error('Failed to save folder schedule.');
      }
    },
    [updateFolder, setCurrentFolder],
  );

  return (
    <styled.ConfigureFolder
      ignoreLastChildGap
      header={
        <Row width={FieldSize.Stretch}>
          <FaGear className="gear" /> {currentFolder?.name}
        </Row>
      }
    >
      <div className="main-container">
        <Text
          name="name"
          label="Folder name:"
          value={currentFolder?.name ?? ''}
          onChange={(e) => {
            setCurrentFolder({ ...currentFolder, name: e.target.value } as IFolderModel);
          }}
        />
        <h2>Folder automation settings</h2>
        <Row>
          <FaGear className="small-gear" /> <h3>Content collection</h3>
        </Row>
        <div className="add-filter">
          <p>
            A folder can be set up here to populate stories automatically based on one of your Saved
            Searches. If you select a saved search any content that matches will be added to your
            folder.
          </p>
          <Checkbox
            name="auto-pop"
            label="Auto-populate this folder"
            checked={currentFolder?.settings.autoPopulate ?? false}
            onChange={(e) => {
              if (!e.target.checked) {
                const newFolder = {
                  ...currentFolder,
                  settings: {
                    autoPopulate: e.target.checked,
                    keepAgeLimit: currentFolder?.settings.keepAgeLimit,
                  },
                };
                delete newFolder?.filter;
                delete newFolder?.filterId;
                setCurrentFolder(newFolder as IFolderModel);
              } else {
                setCurrentFolder({
                  ...currentFolder,
                  settings: {
                    autoPopulate: e.target.checked,
                    keepAgeLimit: currentFolder?.settings.keepAgeLimit,
                  },
                } as IFolderModel);
              }
            }}
          />
          <div hidden={!currentFolder?.settings.autoPopulate ?? true}>
            <label>Choose one of your Saved Searches to apply to this folder</label>
            <Row className="choose-filter" nowrap gap="0.5rem">
              <Col flex="1">
                <Select
                  options={filterOptions}
                  name="filters"
                  isClearable={false}
                  className="filter-select"
                  value={filterOptions.find(
                    (option) => option.value === currentFolder?.filterId ?? '',
                  )}
                  onChange={(newValue) => {
                    if (!newValue) {
                      setActiveFilter(undefined);
                      setCurrentFolder({ ...currentFolder, filterId: undefined } as IFolderModel);
                    } else {
                      const option = newValue as IOptionItem;
                      const targetFilter = myFilters.find((f) => f.id === option.value);
                      setActiveFilter(targetFilter);
                      setCurrentFolder({
                        ...currentFolder,
                        filterId: targetFilter?.id,
                      } as IFolderModel);
                    }
                  }}
                />
              </Col>
              <Button
                disabled={!activeFilter}
                title="Populate the folder by running the filter now"
                onClick={() => !!activeFilter && handleRun(activeFilter)}
              >
                Run
              </Button>
              <Button
                disabled={!activeFilter}
                variant="secondary"
                title="View Filter"
                onClick={() => window.open(`/search/advanced/${activeFilter?.id}`, '_blank')}
              >
                <FaArrowAltCircleRight />
              </Button>
            </Row>
          </div>
        </div>
        <Schedule
          folderSchedule={currentFolder?.events[0] ?? undefined}
          onScheduleChange={async (value: IFolderScheduleModel) => {
            setCurrentFolder({
              ...(currentFolder as IFolderModel),
              events: [value],
            });
          }}
        />
        <Row className="keep-stories">
          Keep stories for
          <Text
            width={FieldSize.Small}
            name="days"
            type="number"
            min="0"
            max="365"
            value={currentFolder?.settings.keepAgeLimit ?? 0}
            onChange={(e) => {
              const value = parseInt(e.currentTarget.value);
              setCurrentFolder({
                ...(currentFolder as IFolderModel),
                settings: {
                  ...currentFolder?.settings,
                  keepAgeLimit: value,
                  autoPopulate: currentFolder?.settings.autoPopulate ?? false,
                },
              });
            }}
          />
          days.
          <FaInfoCircle data-tooltip-id="keep-age" className="info" />
        </Row>
        <Tooltip id="keep-age" variant="info">
          Remove content older than specified amount of days. Use '0' if you would like to remove
          all content.
        </Tooltip>
        <Row className="action-buttons">
          <Button variant="secondary" onClick={() => navigate(`/folders`)}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              handleSaveFolder(currentFolder as IFolderModel);
            }}
            className="save"
          >
            Save
          </Button>
        </Row>
        <div className="remove-container">
          <Row>
            <FaGear className="small-gear" />
            <h3>Remove content</h3>
          </Row>
          <div className="remove-action-buttons">
            <p>Proceed with caution as these actions may not be undone.</p>
            <Row>
              <Button
                className="warning"
                onClick={() => {
                  toggleEmpty();
                }}
              >
                Empty folder
              </Button>
              <Button
                className="danger"
                onClick={() => {
                  toggleDelete();
                }}
              >
                Delete folder
                <FaTrash />
              </Button>
            </Row>
          </div>
        </div>
      </div>

      <Modal
        headerText="Confirm Empty"
        body={`Are you sure you wish to empty this folder?`}
        isShowing={isShowingEmpty}
        hide={toggleEmpty}
        type="delete"
        confirmText="Yes, Empty Folder"
        onConfirm={() => {
          try {
            if (currentFolder) {
              updateFolder({ ...currentFolder, content: [] }, true)
                .then((data) => {
                  // need to clear state managed content as well
                  currentFolder && setCurrentFolder(data);
                  toast.success(`${currentFolder.name} updated successfully`);
                })
                .catch(() => {});
            }
          } finally {
            toggleEmpty();
          }
        }}
      />
      <Modal
        headerText="Confirm Removal"
        body={`Are you sure you wish to delete this folder?`}
        isShowing={isShowingDelete}
        hide={toggleDelete}
        type="delete"
        confirmText="Yes, Delete Folder"
        onConfirm={() => {
          try {
            if (currentFolder) {
              deleteFolder(currentFolder)
                .then(() => {
                  toast.success(`${currentFolder.name} deleted successfully`);
                })
                .catch(() => {});
            }
          } finally {
            toggleDelete();
          }
        }}
      />
    </styled.ConfigureFolder>
  );
};
