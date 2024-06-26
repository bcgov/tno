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

  .grid-table:nth-child(2) {
    min-height: 100px;
    max-height: calc(-450px + 100vh);
    overflow: auto;
    margin-right: -17px;

    .grid-column {
      > .clickable {
        cursor: pointer;
      }
      > div {
        width: 100%;
      }
    }
  }

  .grid {
    width: 100%;
  }
`;
