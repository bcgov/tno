import styled from 'styled-components';

export const ReportHistoryView = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;

  > div:first-child {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;
    justify-content: space-between;

    button {
      padding: 0.15rem;

      svg {
        min-height: 12px;
        max-height: 12px;
        min-width: 12px;
        max-width: 12px;
      }
    }
  }

  .preview-report {
    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
      img {
        max-width: 100%;
      }
    }
  }
`;
