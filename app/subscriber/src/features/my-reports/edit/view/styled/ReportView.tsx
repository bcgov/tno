import styled from 'styled-components';

export const ReportView = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 1rem;

  /* The preview scrolls within its column, so the page does not scroll to read it. */
  position: sticky;
  top: 0;
  align-self: flex-start;
  height: calc(100dvh - 4.75rem);
  min-width: 0;

  .report-edit-headline-row {
    display: flex;
    align-items: first baseline;
    gap: 0.5em;

    > :nth-child(1) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    > :last-child {
      margin-left: auto;
      justify-content: flex-end;
    }
  }

  .preview-report {
    position: relative;
    flex: 1 1 auto;
    flex-wrap: nowrap;
    min-height: 0;
    overflow-y: auto;
    overflow-x: hidden;

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
