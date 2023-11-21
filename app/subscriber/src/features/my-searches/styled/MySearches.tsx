import styled from 'styled-components';
import { Col } from 'tno-core';

export const MySearches = styled(Col)`
  max-height: calc(100vh - 6.5em);
  .react-tooltip {
    z-index: 999;
  }
  .options {
    box-shadow: 0 0 0.5rem #c7c7c7;
    opacity: 1;
    padding-left: 1.5em;
    padding-right: 1.5em;
    .option {
      &:hover {
        text-decoration: underline;
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        cursor: pointer;
      }
    }
  }
  .filter-name {
    height: 1.5em;
    margin-left: 0.5em;
  }
  /* table styling */
  .table {
    width: 100%;
    overflow: hidden;
    .bookmark-icon {
      margin-right: 0.25em;
      height: 1.25em;
      width: 1.25em;
      color: #6750a4;
    }

    .search-row-options {
      svg {
        height: 1.25em;
        width: 1.25em;
        margin-right: 0.25em;
      }
    }
    .row {
      font-size: 1em;
      font-weight: bold;
      color: #646293;
      &:hover {
        cursor: pointer;
      }
    }
    .re-name {
      height: 1.5em;
      padding: 0;
      &:focus {
        box-shadow: none;
      }
    }
    .header {
      background-color: white;
      font-family: 'Roboto', sans-serif;
      color: #231F20
      font-size: 1em;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;

      }
    }
    .binocs, .gear, .trash {
      &:hover {
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        transform: scale(1.1);
      }
    }
  }
  .filter-add {
    &:focus {
      outline: none;
    }
    cursor: pointer;
    align-self: center;
    margin-bottom: 0.5rem;
    height: 1.6rem;
    width: 2.5rem;
    color: ${(props) => props.theme.css.sidebarIconColor};
    padding-bottom: 0.25em;
    &:hover {
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sidebarIconHoverColor};
    }
  }
`;
