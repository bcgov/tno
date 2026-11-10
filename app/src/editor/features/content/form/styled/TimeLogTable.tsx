import styled from 'styled-components';

export const TimeLogTable = styled.div`
  .rows .row {
    /* Compact rows (the content form pads every .row, which FlexboxTable rows also use). */
    padding: 0;
    margin: 0;
  }

  .fa-trash:hover {
    color: red;
    cursor: pointer;
  }

  .total-text {
    font-weight: 700;
    margin-top: 1%;
    margin-left: 1%;
  }
`;
