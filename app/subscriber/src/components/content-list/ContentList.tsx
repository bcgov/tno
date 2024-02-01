import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Checkbox, Col, IContentModel, Row, Show } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import * as styled from './styled';
import { determineToneIcon, groupContent, truncateTeaser } from './utils';

export interface IContentListProps {
  /** content is an array of content objects to be displayed and manipulated by the content list*/
  content: IContentSearchResult[];
  /** set content to parent component */
  setContent: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
  /** determine the selected content based on the checkbox */
  setSelected: React.Dispatch<React.SetStateAction<IContentModel[]>>;
  /** array of selected content */
  selected: IContentModel[];
}

export const ContentList: React.FC<IContentListProps> = ({ content, setSelected, selected }) => {
  const navigate = useNavigate();
  const { groupBy, viewOptions } = React.useContext(ContentListContext);
  const grouped = groupContent(groupBy, content);

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
                <Row>
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
                  {item.displayMedia && <p>okay</p>}
                  <Show visible={viewOptions.section}>
                    {item.section && <div className="section">{item.section}</div>}
                    {item.page && <div className="page-number">{item.page}</div>}
                  </Show>
                </Row>
                <Row>
                  {viewOptions.teaser && (
                    <div className="teaser">{truncateTeaser(item.body, 250)}</div>
                  )}
                </Row>
              </Col>
            ))}
          </div>
        </div>
      ))}
    </styled.ContentList>
  );
};
