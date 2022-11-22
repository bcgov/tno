import styled from 'styled-components';

export const MorningReport = styled.div`
  width: 100%;
  margin: 1em;

  .content-list {
    div[role='rowgroup'] {
      max-height: calc(100vh - 450px);
      overflow: scroll;
    }
  }
`;
