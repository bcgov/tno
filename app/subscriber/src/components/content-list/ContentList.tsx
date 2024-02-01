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
  setSelected: React.Dispatch<React.SetStateAction<IContentModel[]>>;
  /** array of selected content */
  selected: IContentModel[];
}

export const ContentList: React.FC<IContentListProps> = ({ content, setSelected, selected }) => {
  const navigate = useNavigate();
  const { groupBy, viewOptions } = React.useContext(ContentListContext);
  const grouped = groupContent(groupBy, content);
  const [, { stream }] = useContent();

  const [activeFileReference, setActiveFileReference] = React.useState<IFileReferenceModel>();
  const [activeStream, setActiveStream] = React.useState<string>();

  const handleCheckboxChange = React.useCallback(
    (item: IContentModel, isChecked: boolean) => {
      if (isChecked) {
        setSelected((prevSelected) => [...prevSelected, item]);
      } else {
        setSelected((prevSelected) =>
          prevSelected.filter((selectedItem) => selectedItem.id !== item.id),
        );
      }
    },
    [setSelected],
  );

  React.useEffect(() => {
    if (activeFileReference) {
      stream(activeFileReference.path).then((result) => {
        setActiveStream(result);
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
              >
                <Row className="parent-row">
                  <Checkbox
                    className="checkbox"
                    checked={selected.some((selectedItem) => selectedItem.id === item.id)}
                    onChange={(e) => {
                      handleCheckboxChange(item, e.target.checked);
                    }}
                  />
                  {viewOptions.sentiment && determineToneIcon(item.tonePools[0])}
                  {viewOptions.date && (
                    <div className="date">{moment(item.publishedOn).format('DD-MMM-YYYY')}</div>
                  )}
                  <button className="headline" onClick={() => navigate(`/view/${item.id}`)}>
                    {item.headline}
                  </button>
                  <Show visible={!!item.fileReferences.length && !activeStream}>
                    <FaPlayCircle
                      className="play-icon"
                      onClick={() => setActiveFileReference(item.fileReferences[0])}
                    />
                  </Show>
                  <Show visible={!!activeStream}>
                    <FaEyeSlash className="eye-slash" onClick={() => setActiveStream('')} />
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
                  <Show visible={!!activeStream}>
                    <Col className="media-playback">
                      <video controls src={activeStream} />
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
