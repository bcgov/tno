import styled from 'styled-components';

export const ReportEditSortForm = styled.div`
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 0.15rem;
  padding: 1rem;

  .content-row {
    padding: 0.15rem;
    border-bottom: solid 1px ${(props) => props.theme.css.lineTertiaryColor};
    gap: 0.5rem;
    flex-wrap: nowrap;

    > div {
      justify-content: center;
    }

    > div:first-child {
      svg {
        color: ${(props) => props.theme.css.iconSecondaryColor};
      }
    }

    > div:nth-child(2) {
      min-width: 20px;
      align-items: center;
    }

    > div:nth-child(3) {
      overflow: hidden;
      white-space: nowrap;

      .link {
        cursor: pointer;
        color: ${(props) => props.theme.css.linkPrimaryColor};

        &:hover {
          color: ${(props) => props.theme.css.linkPrimaryHoverColor};
        }
      }
    }

    > div:last-child {
      div.action {
        svg {
          color: ${(props) => props.theme.css.fRedColor};
          height: 14px;
          min-height: 14px;
          max-height: 14px;
          width: 14px;
          min-height: 14px;
          max-height: 14px;
        }
      }
    }

    input {
      padding: 0 0.15rem;
      margin: 0;
    }

    .frm-in {
      margin: 0;
      padding: 0;

      .frm-select {
        margin: 0;
        padding: 0;

        .rs__control {
          min-height: unset;

          .rs__value-container {
            padding: 0;
            margin: 0;

            .rs__single-value {
            }

            .rs__input-container {
              padding: 0;
              margin: 0;
              min-width: 20ch;
            }
          }

          .rs__indicator {
            padding: 0;
            margin: 0;
          }
        }
      }
    }
  }

  .section-empty {
    background-color: ${(props) => props.theme.css.tableEvenRow};
    padding: 1rem;
  }
`;
