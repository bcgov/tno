import styled from 'styled-components';

export const TranscriptionList = styled.div`
  width: 100%;

  .column {
    overflow: hidden;
  }

  .submitted {
    color: ${(props) => props.theme.css.submittedColor};
  }

  .completed {
    color: ${(props) => props.theme.css.completedColor};
  }

  .failed {
    color: ${(props) => props.theme.css.dangerColor};
  }

  .cancelled {
    color: ${(props) => props.theme.css.cancelledColor};
  }
`;
