import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Checkbox, Col, IContentModel, Row, Show } from 'tno-core';

import { IGroupByState, IToggleStates } from './interfaces';
import * as styled from './styled';
import { determineToneIcon, groupContent, truncateTeaser } from './utils';

export interface IContentListProps {
  /** content is an array of content objects to be displayed and manipulated by the content list*/
  content: IContentModel[];
  /** toggleStates is an object that contains the state of the toggleable content information */
  toggleStates: IToggleStates;
  /** groupBy is the key by which the content will be grouped */
  groupBy: IGroupByState;
  /** determine the selected content based on the checkbox */
  setSelected: React.Dispatch<React.SetStateAction<IContentModel[]>>;
}

export const ContentList: React.FC<IContentListProps> = ({
  content,
  toggleStates,
  groupBy = 'source',
  setSelected,
}) => {
  const navigate = useNavigate();
  const grouped = groupContent(groupBy, content);

  // State to keep track of checked items
  const [checkedItems, setCheckedItems] = React.useState<Set<number>>(new Set());

  const handleCheckboxChange = (item: IContentModel, isChecked: boolean) => {
    setCheckedItems((prevCheckedItems) => {
      const itemId = item.id;
      const newCheckedItems = new Set(prevCheckedItems);
      if (isChecked) {
        newCheckedItems.add(itemId);
        setSelected((prevSelected) => [...prevSelected, item]);
      } else {
        newCheckedItems.delete(itemId);
        setSelected((prevSelected) =>
          prevSelected.filter((selectedItem) => selectedItem.id !== item.id),
        );
      }
      return newCheckedItems;
    });
  };

  return (
    <styled.ContentList>
      {Object.keys(grouped).map((group) => (
        <div key={group}>
          <h2 className="group-title">{group}</h2>
          <div>
            {grouped[group].map((item) => (
              <Col
                className={`content-row ${checkedItems.has(item.id) ? 'checked' : ''}`}
                key={item.id}
              >
                <Row>
                  <Checkbox
                    className="checkbox"
                    onChange={(e) => {
                      handleCheckboxChange(item, e.target.checked);
                    }}
                  />
                  {toggleStates.sentiment && determineToneIcon(item.tonePools[0])}
                  {toggleStates.date && (
                    <div className="date">{moment(item.publishedOn).format('DD-MMM-YYYY')}</div>
                  )}
                  <button className="headline" onClick={() => navigate(`/view/${item.id}`)}>
                    {item.headline}
                  </button>
                  <Show visible={toggleStates.section}>
                    {item.section && <div className="section">{item.section}</div>}
                    {item.page && <div className="page-number">{item.page}</div>}
                  </Show>
                </Row>
                <Row>
                  {toggleStates.teaser && (
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
