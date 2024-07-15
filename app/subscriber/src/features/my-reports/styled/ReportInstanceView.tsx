import styled from 'styled-components';

export const ReportInstanceView = styled.div`
  padding: 1rem;

  .preview-subject {
    padding: 0.25rem;
    margin-bottom: 1rem;
    font-weight: 600;
    background: ${(props) => props.theme.css.highlightPrimary};
  }

  p {
    /* Need this to stop the paragraph from making the div wider than it should be */
    width: 0;
    min-width: 100%;
  }

  .article {
    /* Need this to stop the paragraph from making the div wider than it should be */
    width: 0;
    min-width: 100%;
  }
`;
