import { AxiosError } from 'axios';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import _ from 'lodash';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useLookup } from 'store/hooks';
import {
  generateQuery,
  IContentModel,
  IFileReferenceModel,
  IFilterSettingsModel,
  Loader,
  Settings,
  Show,
} from 'tno-core';

import { ContentListContext } from './ContentListContext';
import { ContentRow } from './ContentRow';
import * as styled from './styled';
import { groupContent } from './utils';

export interface IContentListProps {
  /** content is an array of content objects to be displayed and manipulated by the content list*/
  content: IContentSearchResult[];
  /** array of terms to be highlighted in body */
  highlighTerms?: string[];
  /** array of selected content */
  selected: IContentModel[];
  /** prop to determine whether to style the content based on user settings */
  styleOnSettings?: boolean;
  /** determine whether to show date next to the sentiment icon */
  showDate?: boolean;
  /** show the time the content was published */
  showTime?: boolean;
  /** used to determine if the content list is draggable and what to do for handling drop */
  handleDrop?: any;
  /** whether or not the content list is scrollable from within the content list container*/
  scrollWithin?: boolean;
  /** determine whether or not to show the series title */
  showSeries?: boolean;
  /** Whether to store the state of the checkboxes in cache */
  cacheCheck?: boolean;
  /** filter settings for contents */
  filter?: IFilterSettingsModel;
  /** determine the selected content based on the checkbox */
  onContentSelected: (content: IContentModel[]) => void;
  /** Event fires when content is removed. */
  onContentRemove?: (content: IContentModel) => void;
  /** Simplified content list view (no attributes, just title and media buttons), used in places like the commentary block */
  simpleView?: boolean;
}

export const ContentList: React.FC<IContentListProps> = ({
  content,
  highlighTerms,
  selected,
  showDate = false,
  handleDrop,
  scrollWithin = false,
  showSeries = false,
  showTime = true,
  cacheCheck = true,
  filter,
  onContentSelected,
  onContentRemove,
  simpleView = false,
}) => {
  const navigate = useNavigate();
  const { groupBy } = React.useContext(ContentListContext);
  const grouped = groupContent(groupBy, [...content]);
  const [, { streamSilent }] = useContent();
  const [{ settings }] = useLookup();
  const [{ requests }] = useApp();
  const [, { findContentWithElasticsearch }] = useContent();

  const [activeStream, setActiveStream] = React.useState<{ source: string; id: number }>({
    id: 0,
    source: '',
  });
  const [activeFileReference, setActiveFileReference] = React.useState<IFileReferenceModel>();

  // just on init we want to see if anything in local storage
  React.useEffect(() => {
    if (!cacheCheck) return;
    const existing = localStorage.getItem('selected');
    // remove selected items when user navigates away from page or refreshes, etc.
    const handleBeforeUnload = () => {
      localStorage.removeItem('selected');
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    if (existing) {
      const selected: number[] = JSON.parse(existing);
      const selectedContent = content.filter((c) => selected.includes(c.id));
      const missingContent = selected.filter((s) => !selectedContent.some((c) => c.id === s));
      if (missingContent.length) {
        // Need to fetch any content currently not in memory.
        findContentWithElasticsearch(
          generateQuery({
            contentIds: missingContent,
            searchUnpublished: true,
            size: missingContent.length,
          }),
          true,
        )
          .then((data) => {
            onContentSelected([
              ...selectedContent,
              ...data.hits.hits.map((r) => {
                const content = r._source as IContentModel;
                return castToSearchResult(content);
              }),
            ]);
          })
          .catch(() => {});
      } else onContentSelected(selectedContent);
    }
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
    // only want to fire once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // this will update the local storage with the selected content
  React.useEffect(() => {
    if (!cacheCheck) return;
    const existing = localStorage.getItem('selected');
    let array: number[];
    if (existing) {
      array = JSON.parse(existing);
    } else {
      array = [];
    }
    if (array.length !== selected?.length) {
      array = _.uniq([...array, ...selected.map((i) => i.id)]);
      localStorage.setItem('selected', JSON.stringify(array));
    } else if (!existing) {
      localStorage.setItem('selected', JSON.stringify(array));
    }
    // only want to fire when selected changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selected]);

  const handleCheckboxChange = React.useCallback(
    (item: IContentModel, isChecked: boolean) => {
      if (isChecked) {
        onContentSelected([...selected, item]);
      } else {
        onContentSelected(selected.filter((selectedItem) => selectedItem.id !== item.id));
      }
    },
    [onContentSelected, selected],
  );

  React.useEffect(() => {
    if (!!activeFileReference) {
      streamSilent(activeFileReference.path)
        .then((response) => {
          setActiveStream({ source: response.data, id: activeFileReference.contentId });
        })
        .catch(async (ex) => {
          if (ex instanceof AxiosError && ex.response && ex.response.data instanceof Blob) {
            const text = await ex.response.data.text();
            const error = JSON.parse(text);
            if (error.type === 'NoContentException')
              toast.error('Sorry file no longer exists, or has been archived.');
          }
          setActiveFileReference(undefined);
          setActiveStream({
            id: 0,
            source: '',
          });
        });
    }
  }, [activeFileReference, streamSilent, setActiveStream]);

  const popOutIds = React.useMemo(() => {
    return settings.find((setting) => setting.name === Settings.SearchPageResultsNewWindow)?.value;
  }, [settings]);

  return (
    <styled.ContentList className="content-list" scrollWithin={scrollWithin}>
      <Loader visible={requests.some((r) => r.url === 'find-contents-with-elasticsearch')} />
      <Show visible={!handleDrop && !simpleView}>
        {Object.keys(grouped).map((group) => (
          <div key={group} className="grouped-content">
            <h2 className="group-title">{group}</h2>
            <div>
              {grouped[group]
                .sort((a, b) => (a.publishedOn < b.publishedOn ? 1 : -1))
                .map((item) => (
                  <ContentRow
                    className={`${
                      selected?.some((selectedItem) => selectedItem.id === item.id) ? 'checked' : ''
                    }`}
                    key={item.id}
                    selected={selected ?? []}
                    popOutIds={popOutIds}
                    showSeries={showSeries}
                    showDate={showDate}
                    showTime={showTime}
                    item={item}
                    highlighTerms={highlighTerms || []}
                    onCheckboxChange={handleCheckboxChange}
                    filter={filter}
                    activeStream={activeStream}
                    setActiveStream={setActiveStream}
                    activeFileReference={activeFileReference}
                    setActiveFileReference={setActiveFileReference}
                  />
                ))}
            </div>
          </div>
        ))}
      </Show>
      <Show visible={!!handleDrop || simpleView}>
        <DragDropContext onDragEnd={handleDrop}>
          <Droppable droppableId="droppable">
            {(provided) => (
              <div {...provided.droppableProps} ref={provided.innerRef}>
                {content.map((item: IContentSearchResult, index: number) => (
                  <Draggable key={item.id} draggableId={String(item.id)} index={index}>
                    {(provided) => (
                      <div
                        ref={provided.innerRef}
                        {...provided.draggableProps}
                        {...provided.dragHandleProps}
                        className="full-draggable"
                      >
                        <ContentRow
                          className={`${
                            selected?.some((selectedItem) => selectedItem.id === item.id)
                              ? 'checked'
                              : ''
                          }`}
                          popOutIds={popOutIds}
                          showDate={showDate}
                          showTime={showTime}
                          showSeries={showSeries}
                          simpleView={simpleView}
                          onClick={(e) => {
                            // Ensure the target is an Element and use .closest to check if the click was inside a checkbox (see comment below)
                            if (!(e.target instanceof Element) || !e.target.closest('.checkbox')) {
                              navigate(`/view/${item.id}`);
                            }
                          }}
                          selected={selected ?? []}
                          canDrag
                          item={item}
                          onCheckboxChange={handleCheckboxChange}
                          onRemove={onContentRemove}
                          filter={filter}
                          activeStream={activeStream}
                          setActiveStream={setActiveStream}
                          activeFileReference={activeFileReference}
                          setActiveFileReference={setActiveFileReference}
                        />
                      </div>
                    )}
                  </Draggable>
                ))}
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </Show>
    </styled.ContentList>
  );
};
