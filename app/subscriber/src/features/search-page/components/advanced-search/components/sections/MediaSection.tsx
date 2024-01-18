import { useFilterOptions } from 'components/navbar/hooks';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { useContent } from 'store/hooks';
import { Button, ButtonVariant, Checkbox, Col, Row, Select, Show } from 'tno-core';

import { SubMediaGroups } from '../../constants';
import { ISubMediaGroupExpanded } from '../../interfaces';
import { IFilterDisplayProps } from './IFilterDisplayProps';
import { determineSelectedMedia, sortableMediaOptions } from './utils';

export interface IMediaSectionProps extends IFilterDisplayProps {
  /** the object that contains the expansion states of the media subgroups  */
  mediaGroupExpandedStates: ISubMediaGroupExpanded;
  /** function that controls the expanded state of the media sub-menu items */
  setMediaGroupExpandedStates: (expanded: ISubMediaGroupExpanded) => void;
}

/** Component that contains the media sources for various different media types. Used to filter in the advanced search bar */
export const MediaSection: React.FC<IMediaSectionProps> = ({
  displayFiltersAsDropdown,
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
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

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
          <Row className="sub-container" justifyContent="center">
            <Show visible={!!mediaGroup.options.length && mediaGroupExpandedStates[mediaGroup.key]}>
              <Show visible={!displayFiltersAsDropdown}>
                <div className="check-box-list">
                  <div className="chk-box-container chk-source select-all">
                    <Button
                      variant={ButtonVariant.link}
                      onClick={() =>
                        storeFilter({
                          ...filter,
                          sourceIds: sortableMediaOptions(mediaGroup.options).map((m) => +m.value!),
                        })
                      }
                    >
                      Select All
                    </Button>
                    /
                    <Button
                      variant={ButtonVariant.link}
                      onClick={() => {
                        let subsetSourceIds = sortableMediaOptions(mediaGroup.options).map(
                          (s) => +s.value,
                        );
                        let filteredSourceIds = filter.sourceIds ?? [];
                        subsetSourceIds.forEach(
                          (s) => (filteredSourceIds = filteredSourceIds.filter((f) => f !== s)),
                        );
                        storeFilter({ ...filter, sourceIds: filteredSourceIds });
                      }}
                    >
                      Deselect All
                    </Button>
                  </div>
                  {sortableMediaOptions(mediaGroup.options).map((item, index) => (
                    <div key={`chk-source-${index}-${item.value}`} className="chk-box-container chk-source">
                      <Checkbox
                        id={`chk-source-${index}-${item.value}`}
                        label={item.label}
                        checked={filter.sourceIds?.includes(+item.value!)}
                        value={item.value}
                        onChange={(e) => {
                          storeFilter({
                            ...filter,
                            sourceIds: e.target.checked
                              ? [...(filter.sourceIds ?? []), +e.target.value] // add it
                              : filter.sourceIds?.filter((i) => i !== +e.target.value), // remove it
                          });
                        }}
                      />
                    </div>
                  ))}
                </div>
              </Show>
              <Show visible={displayFiltersAsDropdown}>
                <Select
                  isMulti
                  key={filter.sourceIds?.join(',')}
                  defaultValue={determineSelectedMedia(filter, mediaGroup.options)}
                  options={sortableMediaOptions(mediaGroup.options)}
                  name="opts"
                  onChange={(newValues) => {
                    Array.isArray(newValues) &&
                      storeFilter({
                        ...filter,
                        sourceIds: newValues.map((v) => v.value),
                      });
                  }}
                />
              </Show>
            </Show>
          </Row>
        </Col>
      ))}
    </Col>
  );
};
