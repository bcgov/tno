import styled from 'styled-components';

export const SearchPage = styled.div`
  max-height: calc(100vh);
  overflow: none;
  .add-page {
    margin-left: auto;
  }
  .checkbox {
    height: 1.5em;
    width: 1.5em;
    margin-top: 0.25em;
  }

  .search-container {
    width: 100%;
  }

  .adv-search-container,
  .result-container {
    width: 50%;
  }
  .save-bar {
    background-color: rgb(233, 236, 239);
    padding-top: 0.5em;
    box-shadow: 0 0.5em 0.5em -0.4em rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(0, 0, 0, 0.02);
    padding-left: 1.5em;
    padding-right: 1.5em;
    padding-bottom: 0.5em;
    .folder-sub-menu {
      margin-left: auto;
    }
    .save-button {
      margin-top: 0.05em;

      height: 1.75em;
      width: 1.75em;
      color: #a5a4bf;
      &:hover {
        transform: scale(1.1);
        cursor: pointer;
        color: ${(props) => props.theme.css.sideBarIconHoverColor};
      }
    }
    input {
      height: 1.75em;
    }
    .label {
      margin-right: 0.25em;
      margin-top: 0.25em;
    }
  }
  .scroll {
    overflow-y: auto;
    max-height: calc(100vh - 8.5em);
    width: 100%;
  }

  .minimized {
    max-width: 59%;
  }

  .media-button {
    max-width: 14em;
    font-size: 0.6em;
    margin-left: auto;
    cursor: pointer;
    border-radius: 0.25em;
    border-width: 0.5px;
    svg {
      margin-top: 0.25em;
      margin-left: 0.5em;
    }
  }

  .player {
    width: 39%;
  }

  .show {
    color: #178d6a;
    background-color: #20c9971a;
    border-color: #178d6a;
  }

  .playing {
    background-color: white;
    color: #e2616e;
    border-color: #e2616e;
  }

  .tone-date {
    margin-left: auto;
    .date {
      color: #8f929d;
    }
    svg {
      margin-top: 0.15em;
      margin-right: 0.5em;
      margin-left: 0;
    }
  }

  .headline {
    color: #3847aa;
    font-size: 1.15em;
    text-decoration: underline;
    :hover {
      cursor: pointer;
    }
  }

  .summary {
    width: 100%;
  }

  .rows {
    .cols {
      width: 100%;
    }
    padding: 0.5em;
    background-color: white;
    &:nth-child(even) {
      background-color: rgb(233, 236, 239);
    }
  }
`;
