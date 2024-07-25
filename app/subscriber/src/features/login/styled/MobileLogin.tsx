import styled from 'styled-components';

export const MobileLogin = styled.div`
  .mobile-view {
    .mobile-title {
      min-width: 100%;
    }
    .app-logo {
      padding: 1%;
      width: 40vmax;
      height: 25vmin;
      background-image: url('/assets/MMinsights_logo_dark_text.svg');
      background-position: center;
      background-repeat: no-repeat;
      background-size: contain;
    }
    .login-box {
      margin-top: 2%;
      margin-bottom: 2%;
      border: 2px solid ${(props) => props.theme.css.btnRedColor};
      border-radius: 0.5rem;
      text-align: center;
      max-height: 91%;
      flex-direction: row;
      padding: 1em;
    }

    .main-box {
      box-shadow: 0px 2px 6px #0000000a;
      background-color: #ffffff;
      margin-right: 1em;
      width: auto;
      @media (max-width: 1550px) {
        margin-left: 1%;
        margin-bottom: 1%;
        min-width: auto;
      }
    }

    .containing-box {
      padding: 2%;
      display: flex;
      max-height: fit-content;
      overflow: hidden;
      @media (max-width: 1550px) {
        display: inline-block;
        margin-left: 1px;
      }
    }

    .buttons {
      margin-top: 5.5%;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
    }

    button {
      color: ${(props) => props.theme.css.btnRedColor};
      font-family: Noto Sans;
      font-size: 28px;
      font-weight: 400;
      line-height: 38px;
      letter-spacing: 0em;
      margin-bottom: 5%;
      min-width: 5em;
      border: none;
      display: flex;
      justify-content: center;
      border: 1px solid black;
      clear: both;
      &.white {
        background-color: ${(props) => props.theme.css.bkWhite};
        &:hover {
          border: 2px solid #a61c29;
          color: ${(props) => props.theme.css.btnRedColor};
        }
      }
    }

    .containing-row {
      margin-left: auto;
      margin-right: auto;
    }

    a {
      color: ${(props) => props.theme.css.fRedColor};
    }

    .idir-logo {
      padding: 1%;
      background-position: center;
      background-repeat: no-repeat;
      background-size: cover;
    }

    .login-content {
      margin-top: 2%;
      display: flex;
      justify-content: space-between;
    }
  }
`;
