import styled from 'styled-components';

export const TranscriptionRequest = styled.div`
  position: relative;
  overflow-x: auto;
  overflow-y: auto;
  background: ${(props) => props.theme.css.bkPrimary};
  width: 100%;
  height: 100%;

  hr {
    width: 100%;
  }

  .containing-col {
    width: 100%;
    display: flow;
  }

  .containing-row {
    overflow-x: auto;
    overflow-y: hidden;
    height: auto;
  }

  .app-logo {
    padding: 1%;
    @media (min-width: 1450px) {
      margin-left: 5%;
    }
    @media (min-width: 850px) {
      max-width: 50em;
      max-height: 25em;
    }
    @media (max-width: 850px) {
      max-width: 25em;
      max-height: 15em;
    }
    max-width: 50em;
    max-height: 25em;
    margin-top: 2.5%;

    margin-bottom: 2.5%;
  }

  a {
    margin-bottom: 2%;
  }

  .containing-box {
    display: flex;
    max-height: fit-content;
    overflow: hidden;
  }

  .mm-logo {
    height: 8%;
    @media (max-width: 768px) {
      height: 6%;
      right: 1%;
    }
    position: fixed;
    right: 5%;
    bottom: 1%;
    z-index: 100;
  }
  .logo-row {
    justify-content: flex-end;
    img {
    }
  }

  .main-box {
    margin-left: auto;
    max-height: fit-content;
    margin-right: auto;
    overflow: hidden;
    width: 110em;
    .top-bar-box {
      margin: 0;
      padding: 0.5em;
      font-size: 18px;
      line-height: 22px;
      letter-spacing: 0em;
      font-weight: bold;
    }
    .message-box {
      padding: 0.5em;
      margin: 0;
      font-size: 18px;
      font-weight: 400;
      line-height: 22px;
      letter-spacing: 0em;
    }
  }
`;
