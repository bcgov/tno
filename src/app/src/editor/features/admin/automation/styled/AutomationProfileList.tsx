import styled from 'styled-components';

export const AutomationProfileList = styled.div`
  width: 100%;
  height: 100%;
  min-height: 100%;
  display: flex;
  justify-content: center;

  div.row {
    cursor: pointer;

    div.column {
      overflow: hidden;
    }
  }

  .table {
    max-height: calc(100% - 120px);
    min-height: 200px;
  }

  .actions {
    margin-top: 0.5rem;
    display: flex;
    gap: 0.5rem;
    justify-content: flex-end;
  }
`;
