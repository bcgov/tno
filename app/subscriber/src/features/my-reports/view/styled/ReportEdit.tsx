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
      border-bottom: solid 1px ${(props) => props.theme.css.lineSecondaryColor};

      .frm-in {
        max-height: 25px;
        padding: 0;
        > div {
          max-height: 25px;
          > input {
            max-height: 25px;
          }
        }
      }

      .frm-select {
        max-height: 25px;
        padding: 0;
        > div {
          max-height: 25px;
          min-height: unset;
          > div {
            align-content: center;
            max-height: 25px;
          }
        }
      }

      > div {
        &:nth-child(2) {
          padding-right: 0.5rem;
        }
        &:nth-child(3) {
          padding-right: 0.5rem;
          text-transform: uppercase;
        }
        &:nth-child(8) {
          svg {
            width: 15px;
            height: 15px;
            min-height: 15px;
            max-height: 15px;
          }
        }
      }
    }

    .section {
      border-left: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
      border-right: solid 1px ${(props) => props.theme.css.lineTertiaryColor};

      .section-header {
        background-color: ${(props) => props.theme.css.sectionHeader};
        color: ${(props) => props.theme.css.sectionHeaderText};

        .frm-in {
          max-height: 25px;
          padding: 0;
          > div {
            max-height: 25px;
            > input {
              max-height: 25px;
            }
          }
        }

        .section-label {
          span:nth-child(1) {
            min-width: 50px;
            text-align: center;
          }

          svg:first-of-type {
            margin-right: 1rem;
          }
        }
      }

      .active-content {
        background-color: ${(props) => props.theme.css.highlightPrimary};
        border-radius: 0.5rem;
      }
      .active-row {
        background-color: ${(props) => props.theme.css.highlightTertiary};
      }
    }
  }

  .preview-section {
    position: relative;
    min-height: 200px;
  }

  .preview-report {
    border: solid 2px ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
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

  .excel-icon {
    height: 1.3em;
    padding: 0.15em;
    border-style: solid;
    border-width: 1px;
    border-color: #8084b1;
    border-radius: 0.2em;
  }

  .edit-content {
    > div:not(.content-bar) {
      padding: 0.5rem 0.5rem 0 0.5rem;
      margin: 0;
    }
    padding-bottom: 0.5rem;
  }
`;
