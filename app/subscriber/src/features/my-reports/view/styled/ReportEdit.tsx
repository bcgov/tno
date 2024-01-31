import styled from 'styled-components';

export const ReportEdit = styled.div`
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

  .sections {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    padding-top: 1rem;

    .content-row {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 0.25rem;
      padding: 0.5rem 0;
      border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};

      .frm-in {
        padding-bottom: 0;
      }
    }

    .section {
      border-left: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
      border-right: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
    }
  }

  .preview-section {
    position: relative;
    min-height: 200px;
  }

  .preview-report {
    border: solid 2px ${(props) => props.theme.css.primaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.primaryLightColor};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
    }
  }

  .report-send {
    margin: 1rem;
    gap: 1rem;

    > div:last-child {
      button {
        margin-top: 1.5rem;
      }
    }
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
      align-items: center;

      &:nth-child(even) {
        background: ${(props) => props.theme.css.highlightSecondary};
      }
    }
  }

  .link {
    cursor: pointer;
    color: ${(props) => props.theme.css.linkPrimaryColor};

    &:hover {
      color: ${(props) => props.theme.css.linkPrimaryHoverColor};
      font-weight: 600;
    }
  }

  .remove-link svg {
    color: ${(props) => props.theme.css.fRedColor};
  }

  .sub-title {
    color: ${(props) => props.theme.css.fPrimaryColor};
    padding: 0;

    label.h2 {
      font-size: 14pt;
    }

    p {
      font-size: 14pt;
      font-weight: 100;
      margin: 0;
    }
  }

  .content-bar {
    .byline {
      font-weight: 600;
      text-transform: uppercase;
    }
    .source {
      text-transform: uppercase;
    }
  }

  .add-story {
    justify-content: center;
    align-items: center;
    border-bottom: solid 1px ${(props) => props.theme.css.fPrimaryColor};
    padding: 0 0 0.5rem 0;
  }
`;
