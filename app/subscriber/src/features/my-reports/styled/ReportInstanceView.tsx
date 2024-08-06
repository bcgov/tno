import styled from 'styled-components';

export const ReportInstanceView = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;

  .preview-subject {
    padding: 0.25rem;
    margin-bottom: 1rem;
    font-weight: 600;
    background: ${(props) => props.theme.css.highlightPrimary};
  }

  p {
    width: 100%;
  }

  .article {
    width: 100%;
  }

  .preview-body {
    word-wrap: break-word;
    width: 100%;
  }
  img {
    max-width: 100%;
    height: auto;
  }
`;
