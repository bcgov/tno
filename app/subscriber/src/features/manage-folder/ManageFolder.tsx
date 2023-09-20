import { SearchWithLogout } from 'components/search-with-logout';
import { DetermineToneIcon } from 'features/home';
import parse from 'html-react-parser';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { FaArrowLeft, FaFolderMinus, FaGripLines } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useFolders } from 'store/hooks/subscriber/useFolders';
import { Checkbox, Col, IContentModel, IFolderModel, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component that renders the manage folder page, allowing user to view/manage content inside a folder
 * @returns ManageFolder component
 */
export const ManageFolder: React.FC = () => {
  const { id } = useParams();
  const [, { getFolder, updateFolder }] = useFolders();
  const navigate = useNavigate();
  const [, { findContent }] = useContent();
  const [folder, setFolder] = React.useState<IFolderModel>();
  const [items, setItems] = React.useState<any>([]);
  const [sortedFlag, setSortedFlag] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  /** TODO: Folder content only contains contentId and sortOrder so we have to make an additional call based off of the contentIds to get the headline/summary etc..
   * assuming we want this to differ eventually.
   */
  React.useEffect(() => {
    getFolder(Number(id)).then((data) => {
      // have to use temp not folder state as folder state will not update right away
      const temp = data;
      setFolder(data);
      findContent({
        contentTypes: [],
        contentIds: data.content.map((c) => c.contentId),
      }).then((data) => {
        const tempSort = data.items.map((item) => ({
          ...item,
          sortOrder: temp.content.find((c) => c.contentId === item.id)?.sortOrder ?? 0,
        }));
        tempSort.sort((a, b) => a.sortOrder - b.sortOrder);
        setItems(tempSort);
      });
    });
    // Only on initialize, or when sort order changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sortedFlag]);

  /** function that runs after a user drops an item in the list */
  const handleDrop = (droppedItem: any) => {
    if (!droppedItem.destination) {
      return;
    }
    var updatedList = [...items];
    // Remove dragged item
    const [reorderedItem] = updatedList.splice(droppedItem.source.index, 1);
    // Add dropped item
    updatedList.splice(droppedItem.destination.index, 0, reorderedItem);
    // Update State
    //  has what we want still sorted
    setItems(updatedList);
    // Update Folder
    !!folder &&
      updateFolder({
        ...folder,
        content: updatedList.map((item, index) => ({
          ...item,
          contentId: item.id,
          sortOrder: index,
        })),
      }).then((data) => {
        setFolder(data);
      });
    // setSortedFlag(!sortedFlag);
  };

  /** function that will remove items from the folder when the button is clicked */
  const removeItems = () => {
    const updatedList = items.filter((item: any) => !selected.includes(item));
    setItems(updatedList);
    !!folder &&
      updateFolder({
        ...folder,
        content: updatedList.map((item: any, index: any) => ({
          ...item,
          contentId: item.id,
          sortOrder: index,
        })),
      }).then((data) => {
        setFolder(data);
        setSortedFlag(!sortedFlag);
      });
    setSelected([]);
  };

  /** determines whether to show body or summary text */
  const determinePreview = (item: IContentModel) => {
    if (!!item.body && !item.summary) {
      return parse(item.body);
    }
    if (!!item.summary && !item.body) {
      return parse(item.summary);
    }
  };

  return (
    <styled.ManageFolder>
      <SearchWithLogout />
      <Row className="header">
        <FaArrowLeft className="back-arrow" onClick={() => navigate(-1)} />
        <FaFolderMinus className="remove-icon" onClick={() => removeItems()} />
        <div className="title">{`Manage Folder: ${folder?.name}`}</div>
      </Row>
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
                              <DetermineToneIcon
                                tone={item.tonePools?.length ? item.tonePools[0].value : 0}
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
                        <div className="item-preview">{determinePreview(item)}</div>
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
    </styled.ManageFolder>
  );
};
