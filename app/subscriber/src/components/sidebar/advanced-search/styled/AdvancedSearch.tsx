import styled from 'styled-components';
import { Row } from 'tno-core';

export const AdvancedSearch = styled(Row)`
  background-color: white;
  padding: 0.5em;
  .search-bar {
    background-color: rgb(236 231 235);
    padding: 0.3em;
    border-radius: 0.8em;
    svg {
      align-self: center;
      color: rgb(73 69 78);
    }
  }
  .search-input {
    border: none;
    background-color: rgb(236 231 235);
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
  .search-in-group {
    /* add bottom underline */
    border-bottom: 1px solid rgb(202 196 207);
    padding-bottom: 0.5em;
    width: 100%;
  }
  .expanded {
    margin-top: 0.75em;
  }

  .calendar {
    cursor: pointer;
    /* light blue */
    color: rgb(0 123 255);
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
    margin-bottom: 0.75em;
    &:hover:not(.expanded) {
      background-color: rgb(236 231 235);
    }
    .drop-icon {
      margin-left: auto;
      cursor: pointer;
    }
    svg {
      margin-right: 0.5em;
      align-self: center;
    }
    border-bottom: 1px solid rgb(202 196 207);
  }
  .toggles {
    margin-left: 1em;
  }
  .sub-group {
    &:hover {
      background-color: rgb(236 231 235);
    }
    margin-left: 1.5em;
    border-bottom: 1px solid rgb(202 196 207);
  }

  .section,
  .story-options-group,
  .search-options-group {
    width: 100%;
  }

  .search-button {
    margin-left: auto;
    margin-top: 2em;
    background-color: rgb(103 80 164);
    border-radius: 2em;
    border: none;
    &:hover {
      background-color: rgb(103 80 164);
    }
  }
`;
