import styled from 'styled-components';

export const Home = styled.div`
  /* table styling */
  .table-container {
    background-color: white;
  }
  .headline {
    /* link color */
    color: #3847aa;
  }

  .show-media-label {
    font-weight: bold;
    margin-right: 1em;
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
