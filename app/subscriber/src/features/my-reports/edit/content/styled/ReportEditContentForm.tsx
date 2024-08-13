import styled from 'styled-components';

export const ReportEditContentForm = styled.div`
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 0.25rem;
  padding: 0.25rem 1rem;

  .section-actions {
    display: flex;
    flex-direction: row;
    justify-content: center;

    > div {
      flex: 1;
    }

    > div:nth-child(2) {
      display: flex;
      flex-direction: row;
      align-content: center;
      justify-content: center;
    }

    > div:last-child {
      display: flex;
      flex-direction: row;
      align-content: center;
      justify-content: flex-end;
      gap: 0.5rem;
    }
  }

  .sections {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;

    .section {
      .section-header {
        background: ${(props) => props.theme.css.sectionHeader};
        color: ${(props) => props.theme.css.sectionHeaderText};

        .section-sort {
          padding: 0;
          margin: 0;
          input {
            max-height: 20px;
            padding: 0 0.25rem;
            margin: 0;
            text-align: right;
          }
        }

        .action {
          svg {
            color: ${(props) => props.theme.css.sectionHeaderText};
          }
        }

        .section-header-label {
          > svg {
            color: ${(props) => props.theme.css.iconSecondaryColor};
          }
          > span:nth-child(2) {
            text-transform: uppercase;
            padding-right: 1rem;
            color: ${(props) => props.theme.css.iconSecondaryColor};
          }
          > span:nth-child(3) {
            font-weight: 600;
          }
        }
      }

      .section-icon {
        > svg {
          color: ${(props) => props.theme.css.sectionHeaderText};
          height: 20px;
          min-height: 20px;
          max-height: 20px;
          width: 20px;
          min-width: 20px;
          max-width: 20px;
        }
      }
    }
  }

  .excel-icon {
    height: 1.3em;
    padding: 0.15em;
    border-style: solid;
    border-width: 1px;
    border-color: #8084b1;
    border-radius: 0.2em;
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
`;
