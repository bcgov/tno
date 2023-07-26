import { useFilterOptions } from 'components/sidebar/hooks';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { Col, Row, Show } from 'tno-core';

import { SubMediaGroups } from '../constants';
import { IAdvancedSearchFilter, ISubMediaGroupExpanded } from '../interfaces';

export interface IMediaSectionProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  mediaExpanded: boolean;
  /** the object that contains the expansion states of the media subgroups  */
  mediaGroupExpandedStates: ISubMediaGroupExpanded;
  /** function that controls the expanded state of the media sub-menu items */
  setmediaGroupExpandedStates: (expanded: ISubMediaGroupExpanded) => void;
  /** change the state of the advanced search */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** use the current state of advanced search */
  advancedSearch: IAdvancedSearchFilter;
}

/** Componenet that contains the media sources for various different media types. Used to filter in the advanced search bar */
export const MediaSection: React.FC<IMediaSectionProps> = ({
  mediaExpanded,
  mediaGroupExpandedStates,
  setmediaGroupExpandedStates,
  setAdvancedSearch,
  advancedSearch,
}) => {
  const { dailyPapers, sources } = useFilterOptions();
  return (
    <Show visible={mediaExpanded}>
      <Col className="expanded media-section space-top">
        {SubMediaGroups(dailyPapers, sources, []).map((mediaGroup, index) => (
          <Col key={`${mediaGroup.key}-${index}`} className="sub-group">
            <Row>
              {`${mediaGroup.label} (${mediaGroup.options.length})`}
              {!mediaGroupExpandedStates[mediaGroup.key] ? (
                <IoIosArrowDroprightCircle
                  className="drop-icon"
                  onClick={() =>
                    setmediaGroupExpandedStates({
                      ...mediaGroupExpandedStates,
                      [mediaGroup.key]: true,
                    })
                  }
                />
              ) : (
                <IoIosArrowDropdownCircle
                  className="drop-icon"
                  onClick={() =>
                    setmediaGroupExpandedStates({
                      ...mediaGroupExpandedStates,
                      [mediaGroup.key]: false,
                    })
                  }
                />
              )}
            </Row>
            <div className="sub-container">
              <Show
                visible={!!mediaGroup.options.length && mediaGroupExpandedStates[mediaGroup.key]}
              >
                {mediaGroup.options.map((option, index) => (
                  <Row key={`${option.name}-${index}`} className="sub-options">
                    <input
                      type="checkbox"
                      onChange={(e) => {
                        if (e.target.checked) {
                          setAdvancedSearch({
                            ...advancedSearch,
                            sourceIds: [...(advancedSearch?.sourceIds ?? []), option.id],
                          });
                        } else {
                          setAdvancedSearch({
                            ...advancedSearch,
                            sourceIds: advancedSearch?.sourceIds?.filter((id) => id !== option.id),
                          });
                        }
                      }}
                    />
                    {option.name.length < 28 ? option.name : option.name.slice(0, 28) + '...'}
                  </Row>
                ))}
              </Show>
            </div>
          </Col>
        ))}
      </Col>
    </Show>
  );
};
