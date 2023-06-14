import styled from 'styled-components';

export const SearchPage = styled.div`
  .search-items {
    padding-left: 1em;
    padding-right: 1em;
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
