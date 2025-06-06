import styled from 'styled-components';
import { Row } from 'tno-core';

export const AdvancedSearch = styled(Row)<{ expanded: boolean }>`
  position: relative;
  .react-datepicker {
    margin-left: 7em;
  }
  .page-section {
    width: 100%;
    height: 90vh;
  }

  .elastic-info {
    margin-left: auto;
  }

  .toggle-item {
    font-size: 0.9em;
  }

  .radio-group {
    width: 8rem;
  }

  input[type='radio'] {
    margin-top: 0.3rem;
    margin-right: 0.3rem;
    height: 0.8em;
    width: 0.8em;
  }

  .narrow-filter-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .toggle-group-container {
    display: flex;
    align-items: inherit;
    .toggle-item svg {
      margin-right: unset;
    }
  }
  .check-box-list {
    display: flex;
    flex-wrap: wrap;
    padding-bottom: 0.5em;
    .select-all {
      button {
        margin: 0 0.375rem;
        height: unset;
        padding: unset;
        font-size: small;
        line-height: inherit;
        font-family: unset;
        border: unset;
      }
    }
    .chk-tag {
      width: 20rem;
    }
    .chk-source {
      width: 16.5rem;
    }
    .chk-series,
    .chk-contributor,
    .chk-media-type,
    .chk-content-type {
      width: 12rem;
    }
    .chk-box-container {
      font-size: small;
      div {
        align-items: center;
        margin: 1px 0;
        input {
          height: auto;
        }
      }
    }
  }

  .search-for-row {
    margin-top: 0.5em;
  }

  /* BOTTOM SEARCH BAR */
  .adv-toolbar {
    position: sticky;
    .frm-in {
      width: 40%;
    }
    background-color: ${(props) => props.theme.css.bkQuaternary};
    width: 100%;
    padding: 0.5em;
    align-items: center;
    .save-cloud {
      margin-right: 0.5em;
      max-height: 30px;
      background-color: white;
      border-color: ${(props) => props.theme.css.btnBkPrimary};
      border-width: 1px;
      border-radius: 0.25em;
      svg {
        height: 1.25em;
        width: 1.25em;
        margin-top: 0.15em;
        color: ${(props) => props.theme.css.btnBkPrimary};
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
    padding: 0em 1em;
    .viewed-name {
      margin: 0 -1em;
      padding-left: 1em;
    }
  }
  /* HEADER OF THE ADVANCED SEARCH COMPONENT */
  .page-section-title {
    border-bottom: ${(props) => (!props.expanded ? 'none' : '')};
    margin: ${(props) => (!props.expanded ? '0.25em' : '')};
  }
  .top-bar {
    .tools {
      margin-left: auto;
      font-size: 0.65em;
      .back-text {
        cursor: pointer;
        text-transform: uppercase;
        font-size: 0.75em;
        margin-right: 0.5em;
        &:hover {
          text-decoration: underline;
        }
      }
      .minimize,
      .back-text,
      .expand {
        align-self: center;
      }
      .minimize,
      .expand {
        color: ${(props) => props.theme.css.iconPrimaryColor};
        fill: ${(props) => props.theme.css.iconPrimaryColor};
        cursor: pointer;
        margin-right: 0.5em;
      }
    }
    width: 100%;
    .reset {
      align-self: center;
      height: 1.25em;
      width: 1.25em;
      cursor: pointer;
      color: ${(props) => props.theme.css.btnBkPrimaryColor};
      &:hover {
        color: ${(props) => props.theme.css.btnBkPrimary};
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
    font-weight: 700;
    margin-right: 0.5em;
  }
  .query-title-label {
    font-weight: 550;
    color: #41393b;
  }

  /* CHECKBOX OPTIONS PASSED IN THE SEARCH IN GROUP SECTION */
  .options {
    label {
      margin-right: 0.5em;
    }
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
    .frm-in {
      padding-right: 0;
      padding-bottom: 0;
    }
    .text-area-editor {
      width: auto;
      resize: vertical;
      padding: 0.3em;
      border: 1px solid rgb(96, 96, 96);
      border-radius: 0.3em;
      height: 2.5em;
      overflow-y: scroll;
      resize: vertical;
      textarea:focus {
        outline: none;
        border: none;
      }
      textarea::-moz-selection {
        color: black;
      }
      textarea::selection {
        color: black;
      }
    }
  }
  .query-text-red {
    color: crimson;
  }
  .query-icon-red {
    display: inline-block;
    color: crimson;
    width: 1em;
    height: 1em;
    margin-left: 0.4em;
  }
  .query-icon-green {
    display: inline-block;
    color: green;
    width: 1em;
    height: 1em;
    margin-left: 0.4em;
  }
  .query-title-container {
    display: flex;
    flex-direction: row;
    justify-content: left;
  }
  .query-validate-row {
    margin-top: 0.5em;
    margin-left: 0.5em;
  }

  /* SEARCH VALIDATION */
  .text-container-validate-div {
    padding: 0;
    width: 100%;
    height: 3em;
    font-size: 0.9em;
    margin: 0;
    overflow-wrap: break-word;
    word-break: break-all;
    overflow-y: scroll;
    resize: vertical;
    .text-area-text {
      display: inline-block;
      overflow-wrap: break-word;
      word-break: break-all;
    }
    .text-area-red {
      color: crimson;
      display: inline-block;
      overflow-wrap: break-word;
      word-break: break-all;
    }
    .text-area-green {
      color: green;
      display: inline-block;
      overflow-wrap: break-word;
      word-break: break-all;
    }
    .text-area-underline {
      color: crimson;
      display: inline-block;
      text-decoration: underline;
      overflow-wrap: break-word;
      word-break: break-all;
    }
  }

  /* EXPANDABLE ROW IN THE ADVANCED SEARCH */
  .option-row {
    display: flex;
    justify-content: center;
    cursor: pointer;
    align-items: center;
    padding: 0.5em 0.5em 0.5em 0.5em;
    color: ${(props) => props.theme.css.btnSecondaryColor};
    font-weight: 650;

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
    margin-left: 2em;
    color: ${(props) => props.theme.css.btnSecondaryColor};
  }

  /* FOR THE DATE PICKER SECTION */
  .date-navigator {
    margin-left: auto;
    margin-right: auto;
  }

  .date-range-toggle {
    padding: 0;
    margin-top: auto;
    max-width: 20em;
    display: flex;
    flex-direction: row;

    > button {
      flex: 1;
      white-space: nowrap;
      > div {
        > div {
          justify-content: center;
        }
      }
    }
  }

  .date-range {
    @media (max-width: 1300px) {
      max-width: 15em;
    }
    display: flex;
    flex-direction: column;
    align-items: center;
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
    font-size: 1em;
    margin-left: 0.5em;
  }

  .sub-group-title {
    &:hover {
      cursor: pointer;
    }
    .drop-icon {
      margin-left: auto;
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
    border-bottom: 1px solid ${(props) => props.theme.css.lineTertiaryColor};
  }
  .toggles {
    margin-left: 1em;
  }
  .sub-group {
    &:hover {
      background-color: ${(props) => props.theme.css.highlightPrimary};
    }
    padding: 0.1em;
    margin-left: 0.1em;
    border-bottom: 1px solid ${(props) => props.theme.css.lineTertiaryColor};
  }

  .story-options-group,
  .search-options-group {
    width: 100%;
  }

  .section {
    width: 100%;
    .action-icons {
      .active-filter-icon {
        color: ${(props) => props.theme.css.iconGreen};
      }
      margin-left: auto;
      flex-direction: row;
      .drop-icon {
        cursor: pointer;
      }
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
    font-weight: 400;
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
    padding-bottom: 0.5em;
    .picker {
      margin-right: 0.5em;
      margin-bottom: 0.5em;
      display: flex;
      justify-content: center;
      align-items: center;
      width: 100%;
      @media (max-width: 1300px) {
        margin-bottom: 0.5em;
        margin-left: 0.5em;
      }
    }
    p {
      margin: 0.35em 0.35em;
      text-align: center;
    }
    .react-datepicker-wrapper {
      max-width: fit-content;
      margin-top: 0.25em;
    }
    .date-picker {
      width: 5.5em;
      border-radius: 0.25em;
      border: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    }
    .clear {
      margin-left: 0.25em;
      &:hover {
        cursor: pointer;
        transform: scale(1.1, 1.1);
      }
    }
  }

  .react-datepicker__day--keyboard-selected,
  .react-datepicker__day--in-selecting-range {
    background-color: ${(props) => props.theme.css.btnLightRedColor};
  }

  .react-datepicker__day--in-range,
  .react-datepicker__day--selected {
    background-color: ${(props) => props.theme.css.btnRedColor};
  }

  .react-datepicker__day--keyboard-selected,
  .react-datepicker__month-text--keyboard-selected,
  .react-datepicker__quarter-text--keyboard-selected,
  .react-datepicker__year-text--keyboard-selected {
    &:hover {
      background-color: ${(props) => props.theme.css.btnRedColor};
    }
  }
  .sub-group-title {
    max-height: 100%;
    min-width: 15em;
  }
`;
