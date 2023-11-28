import styled from 'styled-components';

export const Button = styled.button`
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  align-items: center;
  padding: 0.25rem 0.75rem;
  border-radius: 0.5rem;
  background: ${(props) => props.theme.css.btnBkPrimary};
  color: ${(props) => props.theme.css.btnPrimaryColor};
  border: none;
  outline: inherit;
  cursor: pointer;

  font-size: 1rem;
  line-height: unset;
  font-weight: 400;
  max-height: 26px;

  svg {
    flex-shrink: 0;
    height: 16px;
    max-height: 16px;
    min-height: 16px;
    width: 16px;
    max-width: 16px;
    min-width: 16px;
    color: ${(props) => props.theme.css.btnPrimaryColor};
  }

  &:hover {
    box-shadow: ${(props) => props.theme.css.boxShadow};
  }

  &:active {
    background: ${(props) => props.theme.css.linkPrimaryActiveColor};
    box-shadow: ${(props) => props.theme.css.boxShadow};
  }
`;
