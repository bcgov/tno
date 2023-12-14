import styled from 'styled-components';

import { ITabStyled } from '../interfaces';

export const Tabs = styled.div<ITabStyled>`
  display: flex;
  flex-direction: column;

  .tabs-header {
    display: flex;
    flex-direction: row;
    align-items: stretch;
    min-height: 33px;
    background: ${(props) => props.theme.css.highlightTertiary};

    > div:last-child {
      border-top-right-radius: 0.5rem;
      border-bottom-right-radius: 0.5rem;
      margin-right: 3px;
    }

    .tab {
      flex: 1;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: center;
      margin: 2px 0;
      background: ${(props) => props.theme.css.highlightSecondary};

      & div {
        display: flex;
        flex-direction: row;
      }

      &.active {
        label {
          font-weight: 600;
        }
      }
      &.error {
        color: ${(props) => props.theme.css.fRedColor};
        background: ${(props) => props.theme.css.bkError};

        label {
          color: ${(props) => props.theme.css.fRedColor};
        }
      }
    }
  }

  .tab-container {
    margin: 0 1rem 1rem 1rem;
  }
`;
