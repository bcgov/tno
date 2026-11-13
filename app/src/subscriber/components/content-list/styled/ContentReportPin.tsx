import styled from 'styled-components';

export const ContentReportPin = styled.div`
  > svg {
    color: ${(props) => props.theme.css.btnRedColor};

    &:hover {
      cursor: pointer;
    }
  }

  .content-report-pin-tooltip {
    border: solid 1px ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 0.5rem;
    padding: 0.5rem;
    margin: 0.25rem;
    gap: 0.5rem;
    background: ${(props) => props.theme.css.bkPrimary};

    > div svg {
      color: ${(props) => props.theme.css.btnRedColor};
    }
    .report-link {
      transition:
        background-color 0.3s ease,
        border-radius 0.3s ease;
      padding: 0.2rem;
    }

    .report-link:hover {
      background-color: ${(props) => props.theme.css.stickyNoteColor};
      border-radius: 0.5rem;
    }
  }
`;
