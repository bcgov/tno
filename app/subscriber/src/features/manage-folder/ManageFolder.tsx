import { PageSection } from 'components/section';
import { Sentiment } from 'components/sentiment';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult, determinePreview } from 'features/utils';
import parse from 'html-react-parser';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { FaGripLines } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Checkbox, Col, generateQuery, IContentModel, IFolderModel, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component that renders the manage folder page, allowing user to view/manage content inside a folder
 * @returns ManageFolder component
 */
export const ManageFolder: React.FC = () => {
  const { id } = useParams();
  const [, { getFolder, updateFolder }] = useFolders();
  const navigate = useNavigate();
  const [, { findContentWithElasticsearch }] = useContent();
  const [folder, setFolder] = React.useState<IFolderModel>();
  const [items, setItems] = React.useState<any>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  /** TODO: Folder content only contains contentId and sortOrder so we have to make an additional call based off of the contentIds to get the headline/summary etc..
   * assuming we want this to differ eventually.
   *
   */
  React.useEffect(() => {
    getFolder(Number(id)).then((folder) => {
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
        false,
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
    async (droppedItem: any) => {
      if (!droppedItem.destination) {
        return;
      }
      var updatedList = [...items];
      // hold response
      let res: IFolderModel | undefined;
      // Remove dragged item
      const [reorderedItem] = updatedList.splice(droppedItem.source.index, 1);
      // Add dropped item
      updatedList.splice(droppedItem.destination.index, 0, reorderedItem);
      // Update State
      setItems(updatedList);
      // Update Folder
      if (!!folder) {
        try {
          res = await updateFolder({
            ...folder,
            content: updatedList.map((item, index) => ({
              ...item,
              contentId: item.id,
              sortOrder: index,
            })),
          });
        } catch (error) {}
      }
      setFolder(res);
    },
    [folder, items, updateFolder],
  );

  /** function that will remove items from the folder when the button is clicked */
  const removeItems = React.useCallback(async () => {
    const updatedList = items.filter((item: any) => !selected.includes(item));
    setItems(updatedList);
    let res: IFolderModel | undefined;
    if (!!folder) {
      try {
        res = await updateFolder({
          ...folder,
          content: updatedList.map((item: any, index: any) => ({
            ...item,
            contentId: item.id,
            sortOrder: index,
          })),
        });
      } catch (error) {}
    }
    setFolder(res);
    setSelected([]);
  }, [folder, items, selected, updateFolder]);

  return (
    <styled.ManageFolder>
      <PageSection header={`Manage Folder: ${folder?.name}`}>
        <div className="main-manage">
          <ContentListActionBar
            content={selected}
            onSelectAll={(e) => (e.target.checked ? setSelected(items) : setSelected([]))}
            removeFolderItem={removeItems}
          />
          <DragDropContext onDragEnd={handleDrop}>
            <Droppable droppableId="droppable">
              {(provided) => (
                <div {...provided.droppableProps} ref={provided.innerRef}>
                  {items.map((item: any, index: number) => (
                    <Draggable key={item.id} draggableId={String(item.id)} index={index}>
                      {(provided) => (
                        <div
                          ref={provided.innerRef}
                          {...provided.draggableProps}
                          {...provided.dragHandleProps}
                          className="full-draggable"
                        >
                          <Col className="item-draggable">
                            <Row>
                              <Col>
                                <Checkbox
                                  className="checkbox"
                                  onClick={() => {
                                    if (selected.includes(item)) {
                                      setSelected(selected.filter((i) => i !== item));
                                    } else {
                                      setSelected([...selected, item]);
                                    }
                                  }}
                                />
                              </Col>
                              <Col className="tone-date">
                                <Row>
                                  <Sentiment
                                    value={item.tonePools?.length ? item.tonePools[0].value : 0}
                                  />
                                  <p className="date text-content">
                                    {new Date(item.publishedOn).toDateString()}
                                  </p>
                                </Row>
                              </Col>
                            </Row>
                            <Row>
                              <div
                                onClick={() => navigate(`/view/${item.id}`)}
                                className="item-headline"
                              >
                                {item.headline}
                              </div>
                              <FaGripLines className="grip-lines" />
                            </Row>
                            <div className="item-preview">{parse(determinePreview(item))}</div>
                          </Col>
                        </div>
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </DragDropContext>
        </div>
      </PageSection>
    </styled.ManageFolder>
  );
};
