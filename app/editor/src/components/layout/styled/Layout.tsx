import styled from 'styled-components';

export const Layout = styled.div`
  display: flex;
  flex-direction: column;
  align-items: stretch;
  height: 100vh;

  .main-window {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    flex-grow: 1;
    overflow: hidden;

    & > .navbar:first-child {
      border-bottom: solid 1px #65799e;
    }
  }

  main {
    background-color: #f2f2f2;
    flex-grow: 1;
    padding: 10px;
    position: relative;
    overflow-y: auto;
    height: calc(100% - 100px);
    margin: 0px;
    padding: 0;
    display: flex;
    justify-content: center;
  }

  & > header {
    background-color: ${(props) => {
      switch (process.env.NODE_ENV) {
        case 'development':
          return props.theme.css.developmentBackgroundColor;
        case 'test':
          return props.theme.css.testBackgroundColor;
        default:
          return props.theme.css.productionBackgroundColor;
      }
    }};
  }

  .navbar {
    & > div {
      margin-left: 5em;
    }
  }

  .error-boundary {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 1em;

    & > h1 {
      color: ${(props) => props.theme.css.primaryColor};
    }

    & > div {
      background-color: ${(props) => props.theme.css.darkerBackgroundColor};
      padding: 5em;
      border-radius: 1em;
      box-shadow: 1em 1em 1em ${(props) => props.theme.css.lightVariantColor};
    }

    .error {
      color: red;
    }
  }
`;
