import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaIcons, FaNewspaper, FaPlay, FaRegSmile, FaTag, FaTv, FaUsers } from 'react-icons/fa';
import { FaToggleOff, FaToggleOn } from 'react-icons/fa6';
import { useLookup } from 'store/hooks';
import { Col, ToggleButton } from 'tno-core';

import { useSearchSettings } from '../SearchSettingsContext';
import { ExpandableRow } from './ExpandableRow';
import {
  ContentTypeSection,
  ContributorSection,
  DateSection,
  MediaSection,
  MediaTypeSection,
  PaperSection,
  SentimentSection,
  SeriesSection,
  TagSection,
} from './sections';

export interface IAdvancedSearchSectionsProps {
  /** Whether the lists of values are dropdowns rather than checkboxes. */
  displayFiltersAsDropdown: boolean;
  /** Whether the sections only administrators use are shown. */
  isAdmin: boolean;
}

/**
 * The sections that narrow a search: dates, sources, sentiment, paper attributes, columnists,
 * media types, programs, and for administrators content types and tags. They read and change
 * the settings from the nearest SearchSettingsContext, or the search filter in the store.
 */
export const AdvancedSearchSections: React.FC<IAdvancedSearchSectionsProps> = ({
  displayFiltersAsDropdown,
  isAdmin,
}) => {
  const [search, storeSearchFilter] = useSearchSettings();
  const [{ sources, series, mediaTypes }] = useLookup();

  return (
    <>
      {/* DATE RANGE */}
      <Col className={`date-range-group space-top`}>
        <ExpandableRow
          icon={<BsCalendarEvent />}
          title="Date range"
          hasValues={!!search.startDate || !!search.dateOffset}
        >
          <DateSection />
        </ExpandableRow>
      </Col>
      {/* MEDIA SOURCE SECTION */}
      <Col className={`media-group`}>
        <ExpandableRow
          icon={<BsSun />}
          title="Media source"
          hasValues={!!search.sourceIds?.length}
          headerActions={
            <ToggleButton
              on={<FaToggleOn />}
              off={<FaToggleOff />}
              onClick={() =>
                storeSearchFilter({ ...search, sourceIdsNegate: !search.sourceIdsNegate })
              }
              value={!!search.sourceIdsNegate}
              width="25px"
              height="25px"
              label="NOT"
            />
          }
        >
          <MediaSection
            displayFiltersAsDropdown={displayFiltersAsDropdown}
            sources={sources.filter((s) => s.isEnabled)}
            mediaTypes={mediaTypes.filter((s) => s.isEnabled)}
            series={series.filter((s) => s.isEnabled)}
          />
        </ExpandableRow>
      </Col>
      {/* SENTIMENT SECTION */}
      <Col className={`sentiment-group`}>
        <ExpandableRow
          icon={<FaRegSmile />}
          title="Sentiment"
          hasValues={!!search.sentiment?.length}
        >
          <SentimentSection />
        </ExpandableRow>
      </Col>
      {/* PAPER SECTION */}
      <Col className="paper-attributes">
        <ExpandableRow
          icon={<FaNewspaper />}
          title="Paper attributes"
          hasValues={!!search.section || !!search.page || !!search.edition}
        >
          <PaperSection />
        </ExpandableRow>
      </Col>
      {/* CONTRIBUTOR SECTION */}
      <Col className="expandable-section">
        <ExpandableRow
          icon={<FaUsers />}
          title="Columnists/Anchors"
          hasValues={!!search.contributorIds?.length}
        >
          <ContributorSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
        </ExpandableRow>
      </Col>
      {/* MEDIA TYPES SECTION */}
      <Col className="expandable-section">
        <ExpandableRow
          icon={<FaIcons />}
          title="Media Types"
          hasValues={!!search.mediaTypeIds?.length}
        >
          <MediaTypeSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
        </ExpandableRow>
      </Col>
      {/* SHOW/PROGRAM SECTION */}
      <Col className="expandable-section">
        <ExpandableRow
          icon={<FaPlay />}
          title="Show/Program"
          hasValues={!!search.seriesIds?.length}
        >
          <SeriesSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
        </ExpandableRow>
      </Col>
      {isAdmin && (
        <>
          {/* CONTENT TYPE SECTION */}
          <Col className="expandable-section">
            <ExpandableRow
              icon={<FaTv />}
              title="Content Type"
              hasValues={!!search.contentTypes?.length}
            >
              <ContentTypeSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
            </ExpandableRow>
          </Col>
          {/* TAG SECTION */}
          <Col className="expandable-section">
            <ExpandableRow icon={<FaTag />} title="Tags" hasValues={!!search.tags?.length}>
              <TagSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
            </ExpandableRow>
          </Col>
        </>
      )}
    </>
  );
};
