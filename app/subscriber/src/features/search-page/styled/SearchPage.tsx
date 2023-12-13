import styled from 'styled-components';

export const SearchPage = styled.div`
  .divider {
    margin-left: 0.5em;
    margin-right: 0.5em;
  }
  max-height: 100vh;
  overflow: none;

  .logout {
    margin-right: 1em;
  }

  /* RIBBON CONTAINING SEARCH NAME IF MODIFYING */
  .viewed-name {
    display: flex;
    border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    background-color: ${(props) => props.theme.css.bkWhite};
    padding-bottom: 0.5em;
    padding-top: 0.5em;
    margin-left: 0.5em;
    margin-right: 0.5em;
    svg {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    text-transform: uppercase;
    .filter-name {
      margin-left: 0.5em;
      font-weight: bold;
      font-size: 0.9em;
    }
  }

  .add-page {
    margin-left: auto;
  }
  .checkbox {
    height: 1.5em;
    width: 1.5em;
    margin-top: 0.25em;
  }

  .adv-search-container {
    width: 45%;
    overflow-y: auto;
  }

  .main-search-body {
    overflow-y: auto;
  }

  .result-container {
    width: 55%;
  }

  /** CONTAINED IN PAGE HEADER */
  .page-section-title {
    .title {
      margin-top: auto;
    }
  }

  .page-section {
    max-height: calc(100vh - 4em);
    overflow: hidden;
  }

  /** END HEADER */

  .folder-sub-menu {
    /* .row-item { */
    font-size: medium;
    /* } */
    margin-left: auto;
    svg {
      height: 1.5em;
    }
  }

  .share-sub-menu {
    /* .row-item { */
    font-size: medium;
    /* } */
    svg {
      height: 1.5em;
    }
  }

  .search-contents {
    padding-top: 0;
    max-height: calc(100vh - 12em);
  }

  .save-bar {
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
    max-width: fit-content;
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
    color: ${(props) => props.theme.css.fPrimaryColor};
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

  .padding-left {
    padding-left: 0.5em;
  }

  .helper-text {
    background-color: ${(props) => props.theme.css.bkWhite};
    padding: 0.5em;
    border-bottom-left-radius: 0.5em;
    border-bottom-right-radius: 0.5em;
  }
`;
