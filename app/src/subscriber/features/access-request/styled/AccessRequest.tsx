import styled from 'styled-components';

export const AccessRequest = styled.div`
  position: relative;
  overflow-x: auto;
  overflow-y: hidden;
  background: ${(props) => props.theme.css.bkPrimary};
  width: 100%;
  height: 100dvh;

  .containing-row {
    overflow-x: auto;
    overflow-y: hidden;
    height: auto;

    justify-items: center;
    align-items: center;
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
      max-width: 30em;
      max-height: 15em;
    }
    max-width: 50em;
    max-height: 25em;

    @media (max-width: 1450px) {
      margin-left: auto;
      margin-right: auto;
    }
    margin-top: 2.5%;

    margin-bottom: 2.5%;
  }

  a {
    margin-bottom: 2%;
  }

  .containing-box {
    padding: 2%;
    display: flex;
    justify-content: center;
    max-height: fit-content;
    overflow: hidden;
    @media (max-width: 1550px) {
      display: inline-block;
      margin-left: 5em;
    }
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
    box-shadow: 0px 2px 6px #0000000a;
    background-color: #ffffff;
    max-height: fit-content;
    margin-right: auto;
    overflow: hidden;
    width: 110em;
    border-radius: 1rem;

    > p {
      margin-left: auto;
      margin-right: auto;
      padding: 1rem;
    }

    @media (min-width: 850px) {
      max-width: 70em;
    }
    @media (max-width: 1550px) {
      min-width: 50em;
      margin-left: 1%;
      margin-bottom: 1%;
    }
    @media (min-width: 1450px) {
      max-width: 130em;
    }
    .top-bar-box {
      margin: 0;
      background-color: ${(props) => props.theme.css.dialogBoxBkPrimary};
      color: white;
      padding: 1rem;
      font-size: 18px;
      font-weight: 400;
      line-height: 22px;
      letter-spacing: 0em;
      text-align: center;
    }
  }
`;
