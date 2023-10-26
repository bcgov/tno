import styled from 'styled-components';

export const Dashboard = styled.div`
  display: flex;
  flex-direction: column;

  .form-page {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;

    .filter {
      background: ${(props) => props.theme.css.filterBackgroundColor};
      border-radius: 0.5rem;
      padding: 0.5rem;
      gap: 0.5rem;
      align-items: center;

      > .frm-in {
        padding: 0;
      }
    }

    .ingest {
      height: 100px;
      width: 200px;
      display: flex;
      flex-direction: column;
      padding: 0;

      & div:first-child {
        flex: 1 1 100%;
        overflow: hidden;

        h3 {
          flex: 1 1 100%;
          padding: 0.25rem;
          overflow: auto;
        }

        .icon {
          padding: 0.15rem 0.25rem;
          color: rgba(0, 0, 0, 0.5);
          background: rgba(255, 255, 255, 0.5);
          border-bottom-right-radius: 0.5rem;
        }

        svg:last-child {
          cursor: pointer;
          background-color: white;
          box-shadow: 0 3px 3px grey;
          padding: 0.25rem;
          border-radius: 0.25rem;
          margin: 0.25rem;

          &:hover {
            color: ${(props) => props.theme.css.actionButtonColor};
          }
        }
      }

      .lastRan {
        align-items: center;
        background: rgba(255, 255, 255, 0.75);
      }

      > div:last-child {
        background-color: white;
        border-bottom-right-radius: 0.15rem;
        border-bottom-left-radius: 0.15rem;
        padding: 0.25rem;
        text-align: center;
      }

      &.running {
        background: ${(props) => props.theme.css.completedColor};
      }

      &.warning {
        background: ${(props) => props.theme.css.accentColor};
      }

      &.error {
        background: ${(props) => props.theme.css.dangerColor};
      }

      &.disabled {
        background-color: ${(props) => props.theme.css.cancelledColor};
      }
    }
  }
`;
