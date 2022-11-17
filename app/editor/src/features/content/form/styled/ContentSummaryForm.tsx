import styled from 'styled-components';

export const ContentSummaryForm = styled.div`
  height: 90%;

  .row-margins {
    margin-top: 2%;
  }

  .top-spacer {
    margin-top: 1.16em;
  }

  .show-player {
    margin-left: 10px;
  }

  .add-time {
    margin-right: 2%;
  }

  .textarea {
    height: 50%;

    div {
      height: 100%;
    }
  }

  textarea[name='summary'] {
    height: 80%;
  }

  textarea[name='body'] {
    height: 80%;
  }

  .object-fit {
    object-fit: contain;
  }
`;
