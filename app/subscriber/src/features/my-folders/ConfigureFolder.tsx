import { Button } from 'components/button';
import * as React from 'react';
import { FaGear, FaTrash } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useFolders } from 'store/hooks';
import {
  Checkbox,
  FieldSize,
  getDistinct,
  IContentModel,
  IFilterModel,
  IFolderContentModel,
  IFolderModel,
  IFolderScheduleModel,
  IOptionItem,
  Modal,
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
  const [{ myFilters }, { findMyFilters }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();
  const [{ myFolders }, { findMyFolders, getFolder, updateFolder, deleteFolder }] = useFolders();
  const { id } = useParams();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();

  const [activeFilter, setActiveFilter] = React.useState<IFilterModel>();
  const [actionName, setActionName] = React.useState<'delete' | 'empty'>();
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getFilterOptions(myFilters, activeFilter?.id ?? 0),
  );
  const { folder: currentFolder, setFolder: setCurrentFolder } = useFolderContext();
  const [init, setInit] = React.useState(false);

  React.useEffect(() => {
    if (myFolders.length) setInit(true);
  }, [myFolders.length]);

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
    currentFolder &&
      !currentFolder.events.length &&
      setCurrentFolder({
        ...currentFolder,
        events: [createSchedule(currentFolder.name, currentFolder.description)],
      });
    // only run when currentFolder changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentFolder]);

  React.useEffect(() => {
    if (init && ((!currentFolder && id) || currentFolder?.id !== Number(id))) {
      setInit(false);
      getFolder(Number(id), false)
        .then((folder) => {
          setCurrentFolder(folder);
          if (folder.filter) setActiveFilter(folder.filter);
          else {
            setActiveFilter(undefined);
          }
        })
        .catch(() => {});
    }
    // do not want to run with setCurrentFolder
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentFolder, id, init, getFolder, myFolders]);

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
          await updateFolder({ ...currentFolder, content }, true);
          await findMyFolders();
          toast.success(`Filter found and added ${results.hits.hits.length} content items.`);
        } else {
          toast.warning('No content found for this filter.');
        }
      } catch {}
    },
    [findContentWithElasticsearch, currentFolder, updateFolder, findMyFolders],
  );

  const handleSaveFolder = React.useCallback(
    async (values: IFolderModel) => {
      try {
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
          onChange={(e) =>
            setCurrentFolder({ ...currentFolder, name: e.target.value } as IFolderModel)
          }
        />
        <h2>Folder automation settings</h2>
        <Row>
          <FaGear className="small-gear" /> <h3>Content collection</h3>
        </Row>
        <div className="add-filter">
          <p>
            A folder can be set up here to populate stories automatically based on one of your Saved
            Searches. Choose your Saved Search first and then setup your preferred scheduling
            options.
          </p>
          <Checkbox
            name="auto-pop"
            label="Auto-populate this folder"
            checked={currentFolder?.settings.autoPopulate}
            onChange={(e) => {
              setCurrentFolder({
                ...currentFolder,
                settings: {
                  autoPopulate: e.target.checked,
                  keepAgeLimit: currentFolder?.settings.keepAgeLimit,
                },
              } as IFolderModel);
            }}
          />
          <label>Choose one of your Saved Searches to apply to this folder</label>
          <Row className="choose-filter">
            <Select
              options={filterOptions}
              name="filters"
              isClearable
              className="filter-select"
              value={filterOptions.find((option) => option.value === currentFolder?.filterId ?? '')}
              onChange={(newValue) => {
                if (!newValue) {
                  setActiveFilter(undefined);
                  setCurrentFolder({ ...currentFolder, filterId: undefined } as IFolderModel);
                  return;
                }
                const option = newValue as IOptionItem;
                const targetFilter = myFilters.find((f) => f.id === option.value);
                setActiveFilter(targetFilter);
                setCurrentFolder({ ...currentFolder, filterId: targetFilter?.id } as IFolderModel);
              }}
            />
            <Button
              className="run"
              disabled={!activeFilter}
              onClick={() => !!activeFilter && handleRun(activeFilter)}
            >
              Run Filter
            </Button>
          </Row>
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
                  setActionName('empty');
                  toggle();
                }}
              >
                Empty folder
              </Button>
              <Button
                className="danger"
                onClick={() => {
                  setActionName('delete');
                  toggle();
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
        headerText="Confirm Removal"
        body={`Are you sure you wish to ${actionName} this folder?`}
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={() => {
          try {
            if (actionName === 'empty' && currentFolder) {
              updateFolder({ ...currentFolder, content: [] }, true)
                .then((data) => {
                  // need to clear state managed content as well
                  currentFolder && setCurrentFolder(data);
                  toast.success(`${currentFolder.name} updated successfully`);
                })
                .catch(() => {});
            } else if (actionName === 'delete' && !!currentFolder) {
              deleteFolder(currentFolder)
                .then(() => {
                  toast.success(`${currentFolder.name} deleted successfully`);
                })
                .catch(() => {});
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.ConfigureFolder>
  );
};
