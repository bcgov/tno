import { useFilterOptions } from 'components/sidebar/hooks';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { Col, Row, Select, Show } from 'tno-core';

import { SubMediaGroups } from '../../constants';
import { IAdvancedSearchFilter, ISubMediaGroupExpanded } from '../../interfaces';

export interface IMediaSectionProps {
  /** the object that contains the expansion states of the media subgroups  */
  mediaGroupExpandedStates: ISubMediaGroupExpanded;
  /** function that controls the expanded state of the media sub-menu items */
  setMediaGroupExpandedStates: (expanded: ISubMediaGroupExpanded) => void;
  /** change the state of the advanced search */
  setAdvancedSearch: (advancedSearch: IAdvancedSearchFilter) => void;
  /** use the current state of advanced search */
  advancedSearch: IAdvancedSearchFilter;
}

/** Component that contains the media sources for various different media types. Used to filter in the advanced search bar */
export const MediaSection: React.FC<IMediaSectionProps> = ({
  mediaGroupExpandedStates,
  setMediaGroupExpandedStates,
  setAdvancedSearch,
  advancedSearch,
}) => {
  const {
    dailyPrint,
    sources,
    weeklyPrint,
    cpWire,
    talkRadio,
    onlinePrint,
    television,
    newsRadio,
  } = useFilterOptions();

  return (
    <Col className="expanded media-section space-top">
      {SubMediaGroups(
        dailyPrint,
        weeklyPrint,
        cpWire,
        talkRadio,
        onlinePrint,
        newsRadio,
        television,
        sources,
      ).map((mediaGroup, index) => (
        <Col key={`${mediaGroup.key}-${index}`} className="sub-group">
          <Row
            className="sub-group-title"
            onClick={() => {
              setMediaGroupExpandedStates({
                ...mediaGroupExpandedStates,
                [mediaGroup.key]: !mediaGroupExpandedStates[mediaGroup.key],
              });
            }}
          >
            {`${mediaGroup.label} (${mediaGroup.options.length})`}
            {!mediaGroupExpandedStates[mediaGroup.key] ? (
              <IoIosArrowDroprightCircle
                className="drop-icon"
                onClick={() =>
                  setMediaGroupExpandedStates({
                    ...mediaGroupExpandedStates,
                    [mediaGroup.key]: true,
                  })
                }
              />
            ) : (
              <IoIosArrowDropdownCircle
                className="drop-icon"
                onClick={() =>
                  setMediaGroupExpandedStates({
                    ...mediaGroupExpandedStates,
                    [mediaGroup.key]: false,
                  })
                }
              />
            )}
          </Row>
          <div className="sub-container">
            <Show visible={!!mediaGroup.options.length && mediaGroupExpandedStates[mediaGroup.key]}>
              <Select
                isMulti
                options={mediaGroup.options
                  .map((option) => ({
                    label: option.name,
                    value: option.id,
                    sortOrder: option.sortOrder,
                  }))
                  .sort((a, b) => a.sortOrder - b.sortOrder)}
                name="opts"
                onChange={(newValues) => {
                  Array.isArray(newValues) &&
                    setAdvancedSearch({
                      ...advancedSearch,
                      sourceIds: newValues.map((v) => v.value),
                    });
                }}
                menuPortalTarget={document.body}
                menuPosition={'fixed'}
              />
            </Show>
          </div>
        </Col>
      ))}
    </Col>
  );
};
