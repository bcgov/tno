import styled from 'styled-components';

export const Link = styled.div`
  color: ${(props) => props.theme.css.activeColor};
  cursor: pointer;

  &:hover {
    color: ${(props) => props.theme.css.activeColor};
    text-decoration: underline;
  }

  &:visited {
    color: ${(props) => props.theme.css.secondaryVariantColor};
  }
`;
