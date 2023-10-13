import styled from 'styled-components';

export const Home = styled.div`
  .folder-sub-menu {
    margin-left: auto;
  }
  /* table styling */
  .view-options {
    background-color: white;
    color: black;
    opacity: 1;
    /* add box shadow */
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    z-index: 999;
  }
  .more-options {
    margin-left: 1em;
    margin-top: 0.25em;
    color: #3847aa;
    &:hover {
      cursor: pointer;
    }
  }
  .table {
    width: 100%;
    .group {
      background-color: #f9f9f9 !important;
      font-family: ${(props) => props.theme.css?.bcSans};
    }
    .header {
      background-color: #f5f6fa;
      font-family: 'Roboto', sans-serif;
      font-size: 0.8em;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;
      color: #7c7e8a;

      .column {
        background-color: #f5f6fa;
      }
    }
    .rows {
      cursor: pointer;
    }
  }
  .headline {
    /* link color */
    color: #3847aa;
  }

  .show-media-label {
    font-weight: bold;
    margin-right: 1em;
    font-family: 'Roboto', sans-serif;
    font-size: 0.85em;
    margin-top: 0.5em;
  }
  .filter-buttons {
  }
  .date-navigator {
    .calendar {
      color: #3847aa;
    }
    svg {
      align-self: center;
      height: 1.5em;
      width: 1.5em;
      &:hover {
        cursor: pointer;
      }
    }
    margin-bottom: 1em;
  }
`;
