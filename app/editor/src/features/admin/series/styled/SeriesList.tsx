import styled from 'styled-components';

export const SeriesList = styled.div`
  width: auto;
  min-height: 100%;
  display: flex;
  justify-content: center;
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
`;
