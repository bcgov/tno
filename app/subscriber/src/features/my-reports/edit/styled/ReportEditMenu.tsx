import styled from 'styled-components';

export const ReportEditMenu = styled.div`
  display: flex;
  flex-direction: column;

  .icon-exit {
    transform: rotate(180deg);
  }

  .icon-green {
    color: ${(props) => props.theme.css.iconGreen};
  }

  .report-name {
    flex: 1;
    display: flex;
    flex-direction: row;
    gap: 1rem;
    font-weight: 800;
    font-size: 1.25rem;
    align-items: center;
  }

  > div {
    /* Menu rows */
    display: flex;
    gap: 0.5rem;
    padding: 0.25rem 2rem 0.25rem 0.25rem;
    border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};

    .caret {
      color: ${(props) => props.theme.css.linePrimaryColor};
    }
  }

  > div:nth-child(1) {
    /* Main menu */
    flex-direction: row;

    > div:first-child {
      display: flex;
      flex-direction: row;
      align-items: center;
    }

    > div:nth-child(2) {
      flex: 1;
    }

    > div:not(:first-child) {
      display: flex;
      flex-direction: row;
      align-items: flex-end;
    }
  }
  .report-secondary-menu {
    background: ${(props) => props.theme.css.bkTertiary};
  }

  .error {
    background: ${(props) => props.theme.css.bkError};
    color: ${(props) => props.theme.css.fRedColor};
  }
`;
