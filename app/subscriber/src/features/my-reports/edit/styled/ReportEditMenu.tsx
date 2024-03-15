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
    font-weight: 800;
    font-size: 1.25rem;
  }

  > div {
    /* Menu rows */
    display: flex;
    gap: 0.5rem;
    padding: 0.25rem;
    border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
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

  > div:nth-child(2) {
    /* Secondary menu */
    display: flex;
    flex-direction: row;
    justify-content: flex-end;

    > div {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-items: center;
    }
  }
`;
