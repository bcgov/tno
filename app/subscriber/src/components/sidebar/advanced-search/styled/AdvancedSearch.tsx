import styled from 'styled-components';
import { Row } from 'tno-core';

export const AdvancedSearch = styled(Row)`
  .page-section {
    width: 100%;
  }

  .search-for-row {
    margin-top: 0.5em;
  }

  /* BOTTOM SEARCH BAR */
  .adv-toolbar {
    .frm-in {
      width: 40%;
    }
    background-color: #e8e9f1;
    color: #847379;
    width: 100%;
    padding: 0.5em;
    align-items: center;
    border-bottom-left-radius: 0.75em;
    border-bottom-right-radius: 0.75em;
    .save-cloud {
      margin-right: 0.5em;
      max-height: 30px;
      background-color: white;
      border-color: #6750a4;
      border-width: 1px;
      border-radius: 0.25em;
      svg {
        height: 1.25em;
        width: 1.25em;
        margin-top: 0.15em;
        color: #6750a4;
      }
      &:hover {
        cursor: pointer;
      }
    }
    .label {
      font-size: 0.85em;
    }
    input {
      margin-top: 0.5em;
      max-height: 30px;
    }
  }
  .main-search-body {
    padding-left: 0.5em;
  }
  /* HEADER OF THE ADVANCED SEARCH COMPONENT */
  .top-bar {
    width: 100%;
    .reset {
      margin-left: auto;
      align-self: center;
      height: 1.25em;
      width: 1.25em;
      cursor: pointer;
      color: ${(props) => props.theme.css.btnBkPrimaryColor};
      &:hover {
        color: ${(props) => props.theme.css.subscriberPurple};
        transform: scale(1, 1.1);
      }
    }
  }

  .label {
    margin-right: 0.5em;
    font-size: 0.8em;
  }

  /* SENTIMENT SLIDER */
  .slider {
    .tone-icon {
      margin: 0;
    }
  }

  /* LABEL FOR THE TEXT AREA IN FIRST SECTION */
  .search-in-label {
    margin-right: 0.5em;
    font-size: 0.8em;
    align-self: center;
  }

  /* CHECKBOX OPTIONS PASSED IN THE SEARCH IN GROUP SECTION */
  .options {
    label {
      margin-right: 0.5em;
    }
    accent-color: ${(props) => props.theme.css.btnBkPrimary};
    width: 100%;
    margin-bottom: 1em;
    margin-top: 0.5em;
  }
  .search-in-group {
    .options {
      label {
        align-self: center;
        margin-left: 0.5em;
      }
      font-size: 0.8em;
    }
  }

  /* TEXT AREA IN SEARCH IN GROUP */
  .text-area-container {
    width: 100%;
    .text-area {
      resize: vertical;
    }
  }

  /* EXPANDABLE ROW IN THE ADVANCED SEARCH */
  .option-row {
    padding: 0.5em 0 0.5em 0.5em;

    &:hover {
      cursor: pointer;
    }
    .drop-icon {
      margin-left: auto;
      cursor: pointer;
    }
    svg {
      margin-right: 0.5em;
      align-self: center;
    }
  }

  .option-children {
    .frm-in {
      width: 100%;
    }
  }

  /* FOR THE DATE PICKER SECTION */
  .date-navigator {
    margin-left: auto;
    margin-right: auto;
  }

  .date-range-toggle {
    padding: 0;
    margin-top: auto;
  }

  .date-range {
    @media (max-width: 1300px) {
      max-width: 15em;
    }
    margin-left: auto;
    margin-right: auto;
  }

  .date-picker {
    height: 1.75em;
    box-shadow: rgba(0, 0, 0, 0.2) 0px 1px 2px;
    font-size: 0.75em;
    padding: 0.25em 0.5em;
  }

  /* GENERIC CLASS TO ADD SPACE AT TOP OF NODE */
  .top-spacer {
    margin-top: 0.5em;
  }

  .more-options {
    font-size: 0.8em;
    margin-left: 0.5em;
  }

  .sub-group-title {
    &:hover {
      cursor: pointer;
    }
  }

  .paper-attributes-container {
    .frm-in {
      max-width: fit-content;
    }
  }
  .paper-attributes-container,
  .content-types-container {
    margin-top: 0.5em;
  }

  .date-range-group,
  .media-group,
  .search-in-group,
  .sentiment-group,
  .search-options-group,
  .story-options-group,
  .paper-attributes,
  .expandable-section {
    width: 100%;
    border-bottom: 1px solid ${(props) => props.theme.css.bsGray500};
  }
  .toggles {
    margin-left: 1em;
  }
  .sub-group {
    &:hover {
      background-color: ${(props) => props.theme.css.searchItemHover};
    }
    padding: 0.1em;
    margin-left: 0.1em;
    border-bottom: 1px solid ${(props) => props.theme.css.bsGray500};

    .sub-options {
      font-size: 0.8em;
      max-width: 100%;
      white-space: nowrap;
      &:hover {
        cursor: pointer;
        background-color: ${(props) => props.theme.css.searchItemHover};
      }
    }
  }

  .story-options-group,
  .search-options-group {
    width: 100%;
  }

  .section {
    width: 100%;
    .drop-icon {
      margin-left: auto;
      cursor: pointer;
    }
    svg {
      margin-right: 0.5em;
      align-self: center;
    }
  }

  .rs__control {
    margin-top: 0.25em;
    margin-left: auto;
    min-width: 100%;
  }

  .search-button {
    max-height: 30px;
    margin-left: auto;
    margin-right: 1.5em;
    font-weight: 200;
    font-size: 0.8em;
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border-radius: 0.5em;
    border: none;
    svg {
      align-self: center;
      margin-left: 0.5em;
    }
  }

  .date-range {
    padding: 0.25em;
    .picker {
      margin-right: 0.5em;
      @media (max-width: 1300px) {
        margin-bottom: 0.5em;
        margin-left: 0.5em;
      }
    }
    p {
      margin: 0.35em 0.35em;
    }
    .react-datepicker-wrapper {
      max-width: fit-content;
      margin-top: 0.25em;
    }
    .date-picker {
      width: 5.5em;
      border-radius: 0.25em;
      border: 1px solid #a8aab3;
    }
    .clear {
      margin-left: 0.25em;
      &:hover {
        cursor: pointer;
        transform: scale(1.1, 1.1);
      }
    }
  }

  .sub-group-title {
    max-height: 100%;
    min-width: 15em;
  }
  .sub-container {
    max-height: 10em;
    max-width: 100%;
    overflow-y: auto;
    overflow-x: hidden;
  }
`;
