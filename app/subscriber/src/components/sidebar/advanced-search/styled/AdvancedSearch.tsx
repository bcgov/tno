import styled from 'styled-components';
import { Row } from 'tno-core';

export const AdvancedSearch = styled(Row)`
  background-color: white;
  padding: 0.5em;

  .search-icon {
    &:hover {
      color: ${(props) => props.theme.css.subscriberPurple};
      cursor: pointer;
    }
  }
  .search-bar {
    background-color: ${(props) => props.theme.css.inputGrey};
    border-radius: 1.3em;
    padding: 0.3em;
    .frm-in {
      padding-bottom: 0;
      margin-left: 0.2em;
    }
    svg {
      margin: 0.2em;
      align-self: center;
      color: ${(props) => props.theme.css.searchIconColor};
    }
  }
  .search-input {
    border: none;
    background-color: ${(props) => props.theme.css.inputGrey};
    margin-top: auto;
    margin-bottom: auto;
    padding: 0;
    &:focus {
      outline: none;
      box-shadow: none !important;
    }
  }

  .use-text {
    margin-left: auto;
    color: red;
    &:hover {
      text-decoration: underline;
    }
    font-size: 0.8em;
    cursor: pointer;
  }

  .back-text {
    color: red;
    &:hover {
      text-decoration: underline;
    }
    font-size: 0.8em;
    cursor: pointer;
  }

  .space-top {
    margin-top: 0.5em;
  }

  .search-in-group {
    /* add bottom underline */
    border-bottom: 1px solid ${(props) => props.theme.css.bsGray500};
    padding-bottom: 0.5em;
    margin-top: 0.5em;
    width: 100%;
  }

  .date-navigator {
    margin-left: auto;
    margin-right: auto;
  }

  .narrow {
    margin-top: 1em;
  }

  .date-range-group,
  .media-group,
  .search-options-group,
  .story-options-group {
    padding: 0.5em;
    width: 100%;
    &:hover:not(.expanded) {
      background-color: ${(props) => props.theme.css.searchItemHover};
    }
    .drop-icon {
      margin-left: auto;
      cursor: pointer;
    }
    svg {
      margin-right: 0.5em;
      align-self: center;
    }
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
      /* important tag needed as it is fighting with the sidebar library */
      background-color: transparent !important;
      font-size: 0.8em;
      max-width: 100%;
      white-space: nowrap;
      &:hover {
        cursor: pointer;
        background-color: ${(props) => props.theme.css.searchItemHover};
      }
    }
  }

  .section,
  .story-options-group,
  .search-options-group {
    width: 100%;
  }

  .section {
    margin-top: 0.5em;
  }

  .search-button {
    margin-left: auto;
    margin-top: 2em;
    background-color: ${(props) => props.theme.css.subscriberPurple};
    border-radius: 2em;
    border: none;
  }

  .date-range {
    margin-top: 0.5em;
    justify-content: center;
    p {
      margin: 0.2em 0.35em;
    }
    .react-datepicker-wrapper {
      max-width: fit-content;
    }
    .date-picker {
      width: 5.5em;
      border-radius: 0.25em;
    }
  }

  .sub-container {
    max-height: 10em;
    max-width: 13.8em;
    overflow-y: auto;
    overflow-x: hidden;
  }
`;
