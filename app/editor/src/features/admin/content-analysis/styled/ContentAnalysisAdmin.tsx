import styled from 'styled-components';

export const ContentAnalysisAdmin = styled.div`
  width: 100%;

  h2 {
    font-size: 1.1rem;
    margin: 0;
  }

  .page-description {
    margin-bottom: 1rem;
  }

  .hint {
    color: ${(props) => props.theme.css.lightVariantColor};
    font-size: 0.9rem;
  }

  .panel {
    padding: 1rem 0;
  }

  .counts {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
  }

  // Fields beside buttons: drop the form padding so they share a bottom edge.
  .field-row .frm-in {
    padding-bottom: 0;
  }

  .failures {
    max-height: 50vh;
    overflow-y: auto;
  }

  .failure,
  .backfill {
    border-bottom: 1px solid ${(props) => props.theme.css.tableOddRowColor};
    padding: 0.25rem 0;
  }

  .error {
    color: ${(props) => props.theme.css.dangerColor};
  }
`;
