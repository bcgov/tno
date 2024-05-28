import styled from 'styled-components';

export const SearchPage = styled.div<{ expanded: boolean }>`
  .content-list {
    padding: 0 1em 0 1em;
  }
  .divider {
    margin-left: 0.5em;
    margin-right: 0.5em;
  }
  max-height: 100vh;
  overflow: none;

  .header-col {
    width: 100%;
  }
  .header-row {
    width: 100%;
    .view-options {
      margin-left: auto;
    }
  }
  .result-total {
    font-size: 0.5em;
    margin: 0;
    text-transform: uppercase;
    color: ${(props) => props.theme.css.fPrimaryColor};
  }

  /* RIBBON CONTAINING SEARCH NAME IF MODIFYING */
  .viewed-name {
    display: flex;
    padding-left: 1rem;
    border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    background-color: ${(props) => props.theme.css.bkWhite};
    padding-bottom: 0.5em;
    padding-top: 0.5em;
    svg {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    text-transform: uppercase;
    .filter-name {
      margin-left: 0.5em;
      font-weight: bold;
      font-size: 0.9em;
    }
    .status {
      margin-left: 0.7em;

      &_check_mark {
        color: #19a337;
      }

      &_saved {
        color: #19a337;
        font-weight: bold;
        font-size: 0.9em;
      }

      &_save_changes {
        color: #964b00;
        background-color: #f5f5dc;
        border: none;
        padding: 0.1em 0.625em;
        cursor: pointer;
        font-size: 0.6875em;
      }
    }
  }

  .add-page {
    margin-left: auto;
  }

  .adv-search-container {
    width: 30%;
  }

  .main-search-body {
    overflow-y: auto;
  }

  .result-container {
    width: ${(props) => (props.expanded ? '70%' : '100%')};
    margin-left: ${(props) => (props.expanded ? '' : '1em')};
    margin-top: ${(props) => (props.expanded ? '0' : '1em')};
  }

  .results-only {
    padding: 0.5em;
    overflow-y: auto;
  }

  .result-container-full {
    width: 100%;
  }

  /** CONTAINED IN PAGE HEADER */
  .page-section-title {
    .title {
      margin-top: auto;
    }
  }

  .page-section {
    max-height: calc(100vh - 11em);
    overflow: hidden;
  }
  .adv-search-container > div {
    margin: 0em 1em;
    .page-section {
      margin: 0rem;
    }
  }
  .result-container > .page-section {
    margin: 0em 1em 0em 0em;
  }

  /** END HEADER */

  .folder-sub-menu {
    margin-left: auto;
    svg {
      height: 1.5em;
    }
  }

  .share-sub-menu {
    svg {
      height: 1.5em;
    }
  }

  .share-sub-menu {
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
`;
