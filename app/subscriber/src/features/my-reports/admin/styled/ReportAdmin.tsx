import styled from 'styled-components';

export const ReportAdmin = styled.div`
  form {
    background: unset;
  }

  .page-section {
    margin-top: 0;
  }

  .tab-container {
    padding-top: 0.5rem;
  }

  .tab.report-name {
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0 1.5rem;
    background: ${(props) => props.theme.css.highlightPrimary};

    label {
      text-transform: uppercase;
    }
  }

  .section-bar {
    align-items: center;
    margin-bottom: 0.5rem;
    padding: 0.5rem;
    background: ${(props) => props.theme.css.bkTertiary};
    border: solid 1px ${(props) => props.theme.css.iconPrimaryColor};
    border-radius: 0.5rem;

    svg {
      min-height: 25px;
      min-width: 25px;
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    button {
      max-height: unset;
      padding: 0.75rem;
      background: ${(props) => props.theme.css.highlightSecondary};
      border: solid 1px ${(props) => props.theme.css.iconPrimaryColor};
      color: ${(props) => props.theme.css.iconPrimaryColor};

      > div {
        align-items: center;
      }

      text-transform: uppercase;
    }
  }

  .report-template {
    gap: 0.5rem;

    > div {
      border-left: solid 1px ${(props) => props.theme.css.highlightSecondary};
      border-right: solid 1px ${(props) => props.theme.css.highlightSecondary};
    }

    > div:not(:first-child) {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
  }

  .template-action-bar {
    justify-content: flex-end;
    gap: 1rem;
    margin-bottom: 0.5rem;
    padding: 0.25rem;
    background: ${(props) => props.theme.css.highlightSecondary};
  }

  .tab-container {
    overflow-y: auto;
    height: calc(100dvh - 280px);
  }

  .subscribers {
    padding: 1rem 0;
    gap: 0.25rem;

    .header {
      padding: 0.25rem;
      gap: 1rem;
      color: ${(props) => props.theme.css.fPrimaryColor};

      font-weight: 600;
      border-bottom: 2px solid ${(props) => props.theme.css.lineTertiaryColor};
    }

    .row {
      padding: 0.25rem;
      gap: 1rem;
      color: ${(props) => props.theme.css.fPrimaryColor};

      &:nth-child(even) {
        background: ${(props) => props.theme.css.highlightSecondary};
      }
    }
  }

  .charts {
    gap: 0.5rem;

    > div {
      border-radius: 0.25rem;

      > div:first-child {
        padding: 0.25rem 1rem;
        border-top-right-radius: 0.25rem;
        border-top-left-radius: 0.25rem;
        align-items: center;
        background: ${(props) => props.theme.css.highlightPrimary};
      }

      > div:not(first-child) {
        padding: 0.25rem 1rem;
      }
    }

    > div {
      background: ${(props) => props.theme.css.highlightSecondary};
    }
  }

  .section {
    border-left: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
    border-right: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
  }
`;
