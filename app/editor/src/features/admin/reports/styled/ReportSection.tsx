import styled from 'styled-components';

export const ReportSection = styled.div`
  .description {
    .frm-in {
      background-color: white;
      padding-bottom: 0;
    }

    p {
      background-color: ${(props) => props.theme.css.stickyNoteColor};
      padding: 0.5rem;
      margin: 0 0.5rem 0 0;
      border-bottom-left-radius: 0.5rem;
      border-bottom-right-radius: 0.5rem;
    }
  }

  .section-type {
    p {
      padding: 0.5rem;
      border-radius: 0.5rem;
      background-color: ${(props) => props.theme.css.stickyNoteColor};
      width: auto;
      height: 100%;
    }
  }
`;
