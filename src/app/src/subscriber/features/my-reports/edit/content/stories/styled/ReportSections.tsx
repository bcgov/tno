import styled from 'styled-components';

export const ReportSections = styled.div`
  .section {
    .section-header {
      gap: 0.5rem;

      .section-header-label {
        > span:nth-child(2) {
          text-transform: uppercase;
          margin-right: 1rem;
        }
        > span.section-total {
          flex: 1;
          text-align: right;
        }
      }
    }

    .section-body {
      padding: 0.25rem;

      .add-story {
        border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
        justify-content: space-between;
        padding-bottom: 0.15rem;
        margin-bottom: 0.25rem;

        > div {
          align-items: center;
        }

        > div:last-child {
          align-items: flex-end;
        }
      }

      > div {
        > div {
          display: flex;
          flex-direction: column;
          gap: 0.15rem;

          > div {
            /* Rows */
            border-bottom: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
            > div {
              > div {
                /* Content Rows */
                gap: 0.5rem;

                > div {
                  /* Columns */
                  justify-content: center;
                  align-content: flex-start;

                  .grip-bar {
                    color: ${(props) => props.theme.css.iconGrayColor};
                    width: 16px;
                    height: 16px;
                  }

                  .link {
                    cursor: pointer;
                    color: ${(props) => props.theme.css.linkPrimaryColor};

                    &:hover {
                      color: ${(props) => props.theme.css.linkPrimaryHoverColor};
                    }
                  }
                }

                > div:nth-child(2) {
                  /** Sentiment */
                  min-width: 12px;
                  align-content: center;
                  font-weight: 600;

                  > svg {
                    margin: 0;
                  }
                }

                .story-details {
                  text-transform: uppercase;
                }

                .story-sortOrder {
                  .frm-in {
                    padding: 0 0 0.15rem 0;

                    input {
                      padding: 0 0.15rem;
                    }
                  }
                }

                .remove-link {
                  > svg {
                    height: 14px;
                    min-height: 14px;
                    max-height: 14px;
                    width: 14px;
                    min-width: 14px;
                    max-width: 14px;
                    color: ${(props) => props.theme.css.fRedColor};
                  }
                }
              }
            }
          }
        }
      }
    }

    .icon-refresh svg {
      color: #04814d !important;
      transition: color 0.3s ease;
    }
    .icon-refresh:hover svg {
      transform: rotate(-90deg);
    }
    .icon-refresh:active svg {
      color: #26e194 !important;
    }
  }
`;
