import styled from 'styled-components';

export const MyReports = styled.div`
  display: flex;
  flex-direction: row;

  > div {
    flex: 1;
  }

  .icon-close {
    height: 14px;
    min-height: 14px;
    max-height: 14px;
    width: 14px;
    min-width: 14px;
    max-width: 14px;
  }

  .txt-filter {
    flex-direction: row;
    align-items: center;
    flex: 1;
    text-transform: uppercase;

    @media only screen and (max-width: 500px) {
      flex-direction: column;
      align-items: flex-start;
    }

    label {
      color: ${(props) => props.theme.css.fPrimaryColor};
      font-weight: 400;
    }

    > div {
      flex: 1;
      flex-wrap: nowrap;
      gap: 1rem;

      input {
        min-width: 200px;
      }
    }
  }

  .my-reports-content {
    height: calc(100dvh - 260px);
    overflow-y: auto;
    position: relative;

    > div.report-card.active div.section-header {
      background: ${(props) => props.theme.css.highlightTertiary};
    }
  }

  .report-schedule {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;

    > div:first-child {
      display: flex;
      flex-direction: row;
      justify-content: center;
      width: 50px;

      svg {
        color: ${(props) => props.theme.css.iconSecondaryColor};
        height: 40px;
        max-height: 40px;
        min-height: 40px;
        width: 40px;
        max-width: 40px;
        min-width: 40px;
      }
    }

    & label {
      text-transform: uppercase;
    }
  }

  .report-instance {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;

    > div:first-child {
      display: flex;
      flex-direction: row;
      justify-content: center;
      width: 50px;

      svg {
        color: ${(props) => props.theme.css.iconSecondaryColor};
        height: 30px;
        max-height: 30px;
        min-height: 30px;
        width: 30px;
        max-width: 30px;
        min-width: 30px;
      }
    }

    & label {
      text-transform: uppercase;
    }
  }

  .loading {
    min-height: 100px;
    background: ${(props) => props.theme.css.bkPrimary25};
  }

  .section.report-card {
    .section-header {
      .section-open {
        padding: 0;
      }
    }
    .section-body {
      h3 {
        &.upper {
          text-transform: uppercase;
        }
        margin-bottom: 0;
      }

      > div {
        > div:first-child {
          p {
            /* Need this to stop the paragraph from making the div wider than it should be */
            width: 0;
            min-width: 100%;
          }
          > div:not(:first-child) {
            > div:nth-child(3) {
              margin-top: 0.5rem;
            }

            > div {
              > div:first-child {
                min-width: 100px;
              }
            }
          }
        }

        > div:nth-child(2) {
          border-left: solid 1px black;

          > div h3 {
            display: inline;
            margin-right: 1rem;
          }

          > div:not(:last-child) {
            padding: 0 2rem;
          }

          > div:not(:first-child):not(:last-child) {
            > div:nth-child(2) {
              border-top: solid 1px ${(props) => props.theme.css.linePrimaryColor};
            }
          }

          > div:not(:first-child) {
            > div {
              padding-top: 0.5rem;
            }
          }
        }
      }

      .report-card-schedule {
        justify-content: space-between;
      }

      .report-schedule-enabled {
        color: ${(props) => props.theme.css.tonePositive};
      }

      .report-schedule-disabled {
        color: ${(props) => props.theme.css.toneNegative};
      }
    }
  }

  .report-preview {
    min-width: min-content;
    .report-title {
      display: flex;
      flex-direction: row;
      gap: 1rem;
      align-items: center;

      h2 {
        margin: 0.5rem;
      }
    }
    .bar {
      > div:last-child {
        flex: 1;
        display: flex;
        flex-direction: row;
        // TODO: Need to fix right panel layout so this button can float right.
        /* justify-content: flex-end; */
      }
    }
    .preview-section {
      position: relative;

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
    }
  }
`;
