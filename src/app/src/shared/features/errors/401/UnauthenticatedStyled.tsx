import styled from 'styled-components';

export const Unauthenticated = styled.div`
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100%;

  & > div {
    display: flex;
    flex-direction: row;
    height: 300px;

    & > div {
      display: flex;
      flex-direction: column;
      margin-left: 50px;
    }
  }

  svg {
    g {
      path:first-child {
        fill: ${(props) => props.theme.css.primaryColor};
      }
      path:last-child {
        fill: ${(props) => props.theme.css.primaryVariantColor};
      }
    }
  }

  h1 {
    color: ${(props) => props.theme.css.dangerColor};
  }
`;

export default Unauthenticated;
