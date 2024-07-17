import styled from 'styled-components';

export const FilterList = styled.div`
  display: flex;

  .filter-bar {
    display: flex;
    align-items: center;
    input {
      margin-top: 3.5%;
    }
    button {
      background-color: white;
    }
    background-color: #f5f5f5;
  }

  div.row {
    cursor: pointer;

    div.column {
      overflow: hidden;
    }
  }

  .keyword-cell {
    display: flex;
    align-items: center;
    position: relative;

    .clipboard-icon {
      opacity: 0;
      transition: opacity 0.1s ease;
      cursor: pointer;
      margin-left: 0.5rem;
    }

    &:hover .clipboard-icon {
      opacity: 1;
    }
  }

  .grid {
    min-height: 200px;

    .grid-column {
      overflow: hidden;

      &:nth-child(5n + 5) {
        justify-content: center;
      }
    }
  }
`;
