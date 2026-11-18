import styled from 'styled-components';

export const AddSectionBar = styled.div`
  display: flex;
  flex-direction: column;
  padding: 0.5rem;
  gap: 0.25rem;

  background: ${(props) => props.theme.css.bkTertiary};

  > div:nth-child(1) {
    justify-content: center;
    align-items: center;
    text-transform: uppercase;
    color: ${(props) => props.theme.css.iconPrimaryColor};
  }
  > div:nth-child(2) {
    justify-content: center;
    align-items: center;
    gap: 1rem;
  }

  svg {
    min-height: 25px;
    min-width: 25px;
    color: ${(props) => props.theme.css.iconPrimaryColor};
  }

  button {
    max-height: unset;
    padding: 0.5rem;
    background: ${(props) => props.theme.css.highlightSecondary};
    border: solid 1px ${(props) => props.theme.css.iconPrimaryColor};
    color: ${(props) => props.theme.css.iconPrimaryColor};

    > div {
      align-items: center;
    }

    text-transform: uppercase;
  }
`;
