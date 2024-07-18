import { Action } from 'components/action';
import { ContentList } from 'components/content-list';
import { reorderDragItems } from 'components/content-list/utils';
import { PageSection } from 'components/section';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { DropResult } from 'react-beautiful-dnd';
import { useNavigate, useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { generateQuery, IContentModel, IFolderModel, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component that renders the manage folder page, allowing user to view/manage content inside a folder
 * @returns ManageFolder component
 */
export const ManageFolder: React.FC = () => {
  const { id } = useParams();
  const [, { getFolder, updateFolder }] = useFolders();
  const [, { findContentWithElasticsearch }] = useContent();
  const [folder, setFolder] = React.useState<IFolderModel>();
  const [items, setItems] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();

  /** TODO: Folder content only contains contentId and sortOrder so we have to make an additional call based off of the contentIds to get the headline/summary etc..
   * assuming we want this to differ eventually.
   *
   */
  React.useEffect(() => {
    getFolder(Number(id), true).then((folder) => {
      setFolder(folder);
      if (folder.content.length === 0) return setItems([]);
      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            contentIds: folder.content.map((c) => c.contentId),
            searchUnpublished: false,
            size: 500,
          }),
        ),
        true,
      )
        .then((data) => {
          const items = data.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          });
          const tempSort = items.map((item) => ({
            ...item,
            sortOrder: folder.content.find((c) => c.contentId === item.id)?.sortOrder ?? 0,
          }));
          tempSort.sort((a, b) => a.sortOrder - b.sortOrder);
          setItems(tempSort);
        })
        .catch(() => {});
    });
    // Only on initialize, or when id changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    async (result: DropResult) => {
      const { source, destination } = result;
      if (!destination) return;
      const reorderedItems = reorderDragItems(items, source.index, destination.index);
      setItems(reorderedItems);
      // Update Folder
      let res: IFolderModel | undefined;
      if (!!folder) {
        try {
          res = await updateFolder(
            {
              ...folder,
              content: reorderedItems.map((item, index) => ({
                ...item,
                contentId: item.id,
                sortOrder: index,
              })),
            },
            true,
          );
        } catch (error) {}
      }
      setFolder(res);
    },
    [folder, items, setItems, updateFolder],
  );

  /** function that will remove items from the folder when the button is clicked */
  const removeItems = React.useCallback(
    async (selected: IContentModel[]) => {
      const updatedList = items.filter((item: IContentSearchResult) => !selected.includes(item));
      setItems(updatedList);
      let res: IFolderModel | undefined;
      if (!!folder) {
        try {
          res = await updateFolder(
            {
              ...folder,
              content: updatedList.map((item: any, index: any) => ({
                ...item,
                contentId: item.id,
                sortOrder: index,
              })),
            },
            true,
          );
        } catch (error) {}
      }
      setFolder(res);
      setSelected([]);
    },
    [folder, items, updateFolder],
  );

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  return (
    <styled.ManageFolder>
      <PageSection
        header={
          <Row className="header-row">
            <div className="title">{`Manage Folder: ${folder?.name}`}</div>
            <Action
              variant="close"
              className="close-button"
              title="Revert"
              onClick={() => navigate(`/folders`)}
            />
          </Row>
        }
      >
        <div className="main-manage">
          <ContentListActionBar
            content={selected}
            onSelectAll={(e) => (e.target.checked ? setSelected(items) : setSelected([]))}
            onClear={() => setSelected([])}
            removeFolderItem={() => removeItems(selected)}
            disableAddToFolder={true}
          />
          <ContentList
            handleDrop={handleDrop}
            content={items}
            showDate={true}
            showTime={true}
            showSeries={true}
            selected={selected}
            onContentSelected={handleContentSelected}
            onContentRemove={(item) => {
              removeItems([item]);
            }}
          />
        </div>
      </PageSection>
    </styled.ManageFolder>
  );
};
