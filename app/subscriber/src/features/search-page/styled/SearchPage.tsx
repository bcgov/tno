import styled from 'styled-components';

export const SearchPage = styled.div`
  .search-with-logout {
    margin-bottom: 0.5%;
  }
  .scroll {
    overflow-y: auto;
    max-height: calc(100vh - 6.5em);
    padding-left: 1em;
    padding-right: 1em;
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
      margin-top: 0.65em;
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
    .date {
      margin-left: auto;
      color: #8f929d;
    }
    svg {
      margin-top: 0.5em;
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
