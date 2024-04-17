import styled from 'styled-components';

export const Link = styled.div`
  color: ${(props) => props.theme.css.activeColor};
  cursor: pointer;
  display: flex;
  align-items: center;
  padding: 8px;
  text-decoration: underline;

  &:hover {
    color: ${(props) => props.theme.css.activeColor};
    text-decoration: underline;
  }

  &:visited {
    color: ${(props) => props.theme.css.secondaryVariantColor};
  }

  & > *:not(:last-child) {
    margin-right: 10px;
  }

  & svg {
    margin-right: 8px;
    fill: currentColor;
  }
`;
