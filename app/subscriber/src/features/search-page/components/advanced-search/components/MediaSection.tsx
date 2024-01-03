import { useFilterOptions } from 'components/sidebar/hooks';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { Col, Row, Show } from 'tno-core';

import { SubMediaGroups } from '../constants';
import { ISubMediaGroupExpanded } from '../interfaces';

export interface IMediaSectionProps {
  /** variable that keeps track of whether the sub-menu is expanded or not */
  mediaExpanded: boolean;
  /** the object that contains the expansion states of the media subgroups  */
  mediaGroupExpandedStates: ISubMediaGroupExpanded;
  /** function that controls the expanded state of the media sub-menu items */
  setMediaGroupExpandedStates: (expanded: ISubMediaGroupExpanded) => void;
}

/** Component that contains the media sources for various different media types. Used to filter in the advanced search bar */
export const MediaSection: React.FC<IMediaSectionProps> = ({
  mediaExpanded,
  mediaGroupExpandedStates,
  setMediaGroupExpandedStates,
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
    <Show visible={mediaExpanded}>
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
          </Col>
        ))}
      </Col>
    </Show>
  );
};
