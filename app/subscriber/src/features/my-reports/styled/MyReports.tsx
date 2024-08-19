import styled from 'styled-components';

export const MyReports = styled.div`
  .section-label {
    font-size: 1.1rem;
  }
  .next-scheduled {
    margin-right: 1.5rem;
  }
  .report-tag {
    min-width: 3.5em;
    max-width: fit-content;
  }
  .media-analytics {
    svg {
      height: 1.25em;
      width: 1.25em;
      margin-right: 0.25em;
    }
  }
  .create-new-button {
    padding: 0.25rem 0.75rem;
    height: auto;
  }
  button:not(.create-new-button) {
    background-color: white;
    &:hover {
      box-shadow: 0 0 0 1px ${(props) => props.theme.css.linkPrimaryColor};
    }
    color: ${(props) => props.theme.css.linkPrimaryColor};
    svg {
      color: ${(props) => props.theme.css.linkPrimaryColor};
    }
    border: 1px solid ${(props) => props.theme.css.linkPrimaryColor};
  }
  display: flex;

  > div {
    flex: 1;
  }

  .txt {
    border-radius: 1.5rem;
    width: 25em;
  }

  .icon-close {
    height: 14px;
    min-height: 14px;
    max-height: 14px;
    width: 14px;
    min-width: 14px;
    max-width: 14px;
  }

  .create-new-button {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    svg {
      color: white;
      height: 16px;
      width: 16px;
    }
    .action {
      label {
        color: white;
      }
    }
  }

  .txt-filter {
    flex-direction: row;
    align-items: center;
    flex: 1;
    text-transform: uppercase;

    max-width: fit-content;
    margin-left: auto;
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

  .page-section {
    gap: 0.25rem;

    .my-reports-content {
      height: calc(100dvh - 260px);
      overflow-y: auto;
      position: relative;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;

      > div.report-card.active div.section-header {
        background: ${(props) => props.theme.css.highlightTertiary};
      }
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
          margin-bottom: 0.5rem;
        }
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
    position: relative;
    max-width: 50%;
    width: 50%;
    min-width: 50%;
    overflow: hidden;

    div.preview-body {
      overflow-x: auto;
      max-width: 100%;
      width: 100%;
      min-width: 100%;
      img {
        max-width: 100%;
      }
    }

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
