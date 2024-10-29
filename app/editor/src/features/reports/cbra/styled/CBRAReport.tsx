import styled from 'styled-components';

export const CBRAReport = styled.div`
  & > div:first-child {
    display: inline-block;
    width: 15em;
  }

  .dates {
    display: flex;
    flex-direction: row;
  }

  .buttons {
    display: flex;
    flex-direction: row;
  }

  .report-form {
    margin-left: 1.5rem;
  }
`;
