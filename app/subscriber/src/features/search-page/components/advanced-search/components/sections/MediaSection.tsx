import { useSubMediaGroups } from 'features/hooks';
import React from 'react';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { useContent } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  IMediaTypeModel,
  ISeriesModel,
  ISourceModel,
  Row,
  Select,
  Show,
} from 'tno-core';

import { ISubMediaGroupExpanded } from '../../interfaces';
import { IFilterDisplayProps } from './IFilterDisplayProps';
import { determineSelectedMedia, sortableMediaOptions } from './utils';

export interface IMediaSectionProps extends IFilterDisplayProps {
  sources: ISourceModel[];
  series: ISeriesModel[];
  mediaTypes: IMediaTypeModel[];
}

/** Component that contains the media sources for various different media types. Used to filter in the advanced search bar */
export const MediaSection: React.FC<IMediaSectionProps> = ({
  displayFiltersAsDropdown,
  sources,
  series,
  mediaTypes,
}) => {
  const { subMediaGroups } = useSubMediaGroups(sources, series, mediaTypes);
  const [mediaGroupExpanded, setMediaGroupExpanded] = React.useState<ISubMediaGroupExpanded>({});

  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  return (
    <Col className="expanded media-section space-top">
      {subMediaGroups?.map((mediaGroup, index) => (
        <Col key={`${mediaGroup.key}-${index}`} className="sub-group">
          <Row
            className="sub-group-title"
            onClick={() => {
              setMediaGroupExpanded({
                ...mediaGroupExpanded,
                [mediaGroup.key]: !mediaGroupExpanded![mediaGroup.key],
              });
            }}
          >
            {`${mediaGroup.label} (${mediaGroup.options.length})`}
            {!mediaGroupExpanded![mediaGroup.key] ? (
              <IoIosArrowDroprightCircle
                className="drop-icon"
                onClick={() =>
                  setMediaGroupExpanded({
                    ...mediaGroupExpanded,
                    [mediaGroup.key]: true,
                  })
                }
              />
            ) : (
              <IoIosArrowDropdownCircle
                className="drop-icon"
                onClick={() => {
                  setMediaGroupExpanded({
                    ...mediaGroupExpanded,
                    [mediaGroup.key]: false,
                  });
                }}
              />
            )}
          </Row>
          <Row className="sub-container" justifyContent="center">
            <Show visible={!!mediaGroup.options.length && mediaGroupExpanded![mediaGroup.key]}>
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
                    <div
                      key={`chk-source-${index}-${item.value}`}
                      className="chk-box-container chk-source"
                    >
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
