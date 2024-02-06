import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { FaPlayCircle } from 'react-icons/fa';
import { FaCopyright, FaEyeSlash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Checkbox, Col, IContentModel, IFileReferenceModel, Row, Show } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import * as styled from './styled';
import { determineToneIcon, groupContent, truncateTeaser } from './utils';

export interface IContentListProps {
  /** content is an array of content objects to be displayed and manipulated by the content list*/
  content: IContentSearchResult[];
  /** determine the selected content based on the checkbox */
  onContentSelected: (content: IContentModel[]) => void;
  /** array of selected content */
  selected: IContentModel[];
}

export const ContentList: React.FC<IContentListProps> = ({
  content,
  onContentSelected,
  selected,
}) => {
  const navigate = useNavigate();
  const { groupBy, viewOptions } = React.useContext(ContentListContext);
  const grouped = groupContent(groupBy, content);
  const [, { stream }] = useContent();

  const [activeFileReference, setActiveFileReference] = React.useState<IFileReferenceModel>();
  const [activeStream, setActiveStream] = React.useState<{ source: string; id: number }>();

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
      stream(activeFileReference.path).then((result) => {
        setActiveStream({ source: result, id: activeFileReference.contentId });
      });
    }
  }, [activeFileReference, stream, setActiveStream]);

  return (
    <styled.ContentList>
      {Object.keys(grouped).map((group) => (
        <div key={group}>
          <h2 className="group-title">{group}</h2>
          <div>
            {grouped[group].map((item) => (
              <Col
                className={`content-row ${
                  selected.some((selectedItem) => selectedItem.id === item.id) ? 'checked' : ''
                }`}
                key={item.id}
                onClick={(e) => {
                  // Ensure the target is an Element and use .closest to check if the click was inside a checkbox (see comment below)
                  if (!(e.target instanceof Element) || !e.target.closest('.checkbox')) {
                    navigate(`/view/${item.id}`);
                  }
                }}
              >
                <Row className="parent-row">
                  <Checkbox
                    className="checkbox"
                    checked={selected.some((selectedItem) => selectedItem.id === item.id)}
                    onChange={(e) => {
                      // TODO
                      // e.stopPropagation() does not work, so above we check if the click was inside a checkbox
                      handleCheckboxChange(item, e.target.checked);
                    }}
                  />
                  {viewOptions.sentiment && determineToneIcon(item.tonePools[0])}
                  {viewOptions.date && (
                    <div className="date">{moment(item.publishedOn).format('DD-MMM-YYYY')}</div>
                  )}
                  <button className="headline">{item.headline}</button>
                  <Show
                    visible={
                      !!item.fileReferences.length &&
                      !activeStream?.source &&
                      !item.fileReferences[0].contentType.includes('image')
                    }
                  >
                    <FaPlayCircle
                      className="play-icon"
                      onClick={(e) => {
                        e.stopPropagation();
                        setActiveFileReference(item.fileReferences[0]);
                      }}
                    />
                  </Show>
                  <Show visible={!!activeStream && item.id === activeStream.id}>
                    <FaEyeSlash
                      className="eye-slash"
                      onClick={(e) => {
                        e.stopPropagation();
                        setActiveStream({ source: '', id: 0 });
                        setActiveFileReference(undefined);
                      }}
                    />
                  </Show>
                  <Show visible={viewOptions.section}>
                    {item.section && <div className="section">{item.section}</div>}
                    {item.page && <div className="page-number">{item.page}</div>}
                  </Show>
                </Row>
                <Row>
                  {viewOptions.teaser && !!item.body && (
                    <div className="teaser">{truncateTeaser(item.body, 250)}</div>
                  )}
                  <Show visible={!!activeStream?.source && activeStream.id === item.id}>
                    <Col className="media-playback">
                      {activeFileReference?.contentType.includes('audio') && (
                        <audio controls src={activeStream?.source} />
                      )}
                      {activeFileReference?.contentType.includes('video') && (
                        <video controls src={activeStream?.source} />
                      )}
                      <div className="copyright-text">
                        <FaCopyright />
                        Copyright protected and owned by broadcaster. Your licence is limited to
                        internal, non-commercial, government use. All reproduction, broadcast,
                        transmission, or other use of this work is prohibited and subject to
                        licence.
                      </div>
                    </Col>
                  </Show>
                </Row>
              </Col>
            ))}
          </div>
        </div>
      ))}
    </styled.ContentList>
  );
};
