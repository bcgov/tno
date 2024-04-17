import * as React from 'react';
import { FaGear } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useFolders } from 'store/hooks';
import {
  Button,
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
import { Schedule } from './Schedule';
import * as styled from './styled';
import { createSchedule } from './utils';

export interface IConfigureFolderProps {
  active?: IFolderModel;
}

export const ConfigureFolder: React.FC<IConfigureFolderProps> = ({ active }) => {
  const [{ myFilters }, { findMyFilters }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();
  const [{ myFolders }, { findMyFolders, getFolder, updateFolder, deleteFolder }] = useFolders();
  const { id } = useParams();
  const { toggle, isShowing } = useModal();
  const navigate = useNavigate();

  const [activeFilter, setActiveFilter] = React.useState<IFilterModel | null>();
  const [actionName, setActionName] = React.useState<'delete' | 'empty'>();
  const [currentFolder, setCurrentFolder] = React.useState<IFolderModel>();
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getFilterOptions(myFilters, activeFilter?.id ?? 0),
  );

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
  }, [currentFolder]);

  React.useEffect(() => {
    if ((myFolders.length && !currentFolder && id) || currentFolder?.id !== Number(id)) {
      getFolder(Number(id), false)
        .then((data) => {
          setCurrentFolder(data);
          if (data.filter) setActiveFilter(data.filter);
          else {
            setActiveFilter(null);
          }
        })
        .catch(() => {
          toast.error('Failed to load folder.');
        });
    }
  }, [myFolders, currentFolder, id, getFolder]);

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
          await updateFolder({ ...currentFolder, content });
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
        const result = await updateFolder(values);
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
          value={currentFolder?.name}
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
          <Checkbox name="auto-pop" label="Auto-populate this folder" defaultChecked />
          <label>Choose one of your Saved Searches to apply to this folder</label>
          <Row className="choose-filter">
            <Select
              options={filterOptions}
              name="filters"
              isClearable
              className="filter-select"
              value={filterOptions.find((option) => option.value === activeFilter?.id ?? null)}
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
            <Button className="run" onClick={() => !!activeFilter && handleRun(activeFilter)}>
              Apply
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
          <Button className="cancel" onClick={() => navigate(`/folders`)}>
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
            </Button>
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
            if (actionName === 'empty' && !!active) {
              updateFolder({ ...active, content: [] }).then((data) => {
                // need to clear state managed content as well
                currentFolder && setCurrentFolder(data);
                toast.success(`${active.name} updated successfully`);
              });
            } else if (actionName === 'delete' && !!active) {
              deleteFolder(active).then(() => {
                toast.success(`${active.name} deleted successfully`);
              });
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.ConfigureFolder>
  );
};
