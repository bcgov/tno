import styled from 'styled-components';

export const CBRAReport = styled.div`
  display: inline-block;

  & > div:first-child {
    display: inline-block;
    width: 15em;
  }

  .dates {
    display: flex;
    flex-direction: row;

    span {
      margin-right: 1em;
      width: 20em;
    }
  }

  .buttons {
    display: flex;
    flex-direction: row;
  }
`;
