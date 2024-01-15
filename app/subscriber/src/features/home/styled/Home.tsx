import styled from 'styled-components';

export const Home = styled.div`
  .folder-sub-menu {
    margin-left: auto;
  }
  /* table styling */
  .view-options {
    .option {
      margin-bottom: 0.5em;
      label {
        margin-left: 0.25em;
      }
    }
    b {
      margin-bottom: 0.25em;
    }
    background-color: white;
    color: black;
    opacity: 1;
    /* add box shadow */
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    input[type='radio'] {
      accent-color: #6750a4;
    }
    z-index: 999;
  }
  .more-options {
    margin-left: 1em;
    margin-top: 0.25em;
    color: #6750a4;
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
    }
  }
  .table {
    width: 100%;
    .group {
      background-color: #f9f9f9 !important;
    }
    .header {
      background-color: #f5f6fa;
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
  // tone column
  .column.col-1 {
    width: 2.5rem;
    flex: unset;
    div {
      width: 100%;
      justify-content: center;
      svg.tone-icon {
        margin-left: unset;
      }
    }
  }

  .tableHeadline {
    width: 100%;
    table-layout: fixed;
  }

  .dateColumn {
    width: 20%;
  }

  .headlineColumn {
    width: 80%;
  }

  .td-date {
    white-space: nowrap;
  }

  .headline {
    color: ${(props) => props.theme.css.btnBkPrimary};
    font-size: 16px;
    font-style: normal;
    font-weight: 400;
    line-height: 20px;
    letter-spacing: 0.25px;
  }

  .date {
    color: ${(props) => props.theme.css.fPrimaryColor};
    font-style: normal;
    font-weight: 400;
    line-height: 20px;
    letter-spacing: 0.25px;
    text-transform: uppercase;
  }

  .teaser {
    background: ${(props) => props.theme.css.teaserBackgroundColor};
    border-radius: 8px;
    display: flex;
    height: 46px;
    padding: 7px 12px;
    justify-content: space-between;
    color: ${(props) => props.theme.css.fPrimaryColor};
    leading-trim: both;
    text-edge: cap;
    font-size: 14px;
    font-style: normal;
    font-weight: 400;
    line-height: 18px;
  }

  .show-media-label {
    font-weight: bold;
    margin-right: 1em;
    font-size: 0.85em;
    margin-top: 0.5em;
  }
  .filter-buttons {
    button {
      min-width: 5rem;
      border: none;
      display: flex;
      justify-content: center;
      &.active {
        background-color: ${(props) => props.theme.css.defaultRed};
        color: white;
      }
      &.inactive {
        background-color: ${(props) => props.theme.css.lightInactiveButton};
        color: #7a7978;
      }
    }
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
