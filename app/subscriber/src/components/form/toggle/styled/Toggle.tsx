import styled from 'styled-components';

export const Toggle = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;

  label {
    display: flex;
    flex-direction: row;
    align-items: center;
    font-weight: 800;
    text-transform: uppercase;
    white-space: nowrap;
  }

  > div.options {
    display: grid;
    grid-auto-flow: column;
    grid-auto-columns: 1fr;

    > div:first-child {
      border-top-left-radius: 0.5rem;
      border-bottom-left-radius: 0.5rem;
      border-left: solid 1px ${(props) => props.theme.css.linePrimaryColor};
    }

    > div:last-child {
      border-top-right-radius: 0.5rem;
      border-bottom-right-radius: 0.5rem;
    }

    > div {
      display: flex;
      flex-direction: row;
      gap: 0.25rem;
      justify-content: center;
      align-items: center;
      padding: 0.25rem 0.5rem;
      border-top: solid 1px ${(props) => props.theme.css.linePrimaryColor};
      border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
      border-right: solid 1px ${(props) => props.theme.css.linePrimaryColor};
      text-transform: uppercase;
      color: ${(props) => props.theme.css.fPrimaryColor};

      &.active {
        background: ${(props) => props.theme.css.btnBkPrimary};
        color: ${(props) => props.theme.css.btnPrimaryColor};
      }

      &:hover {
        background: ${(props) => props.theme.css.btnBkSecondary};
        color: ${(props) => props.theme.css.btnSecondaryColor};
        cursor: pointer;
      }

      > svg {
        color: ${(props) => props.theme.css.iconPurpleColor};
        min-width: 14px;
        max-width: 14px;
        min-height: 14px;
        max-height: 14px;
      }

      > div {
        /* flex: 0 0 auto; */
      }

      &.disabled {
        cursor: default;
        background: ${(props) => props.theme.css.btnBkDisabled};
        color: ${(props) => props.theme.css.btnDisabledColor};

        &.active {
          background: ${(props) => props.theme.css.btnBkSecondary};
          color: ${(props) => props.theme.css.btnSecondaryColor};
        }
      }
    }
  }
`;
