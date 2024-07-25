import styled from 'styled-components';

export const BrowserLogin = styled.div`
  .system-message-box {
    height: 410px;
    margin-right: auto;
    width: 48em;
    @media (max-width: 1440px) {
      margin-top: 1em;
    }
  }
  .containing-row {
    margin-left: auto;
    margin-right: auto;
  }

  a {
    color: ${(props) => props.theme.css.fRedColor};
  }

  .modalOpen {
    margin-top: 8%;
    color: ${(props) => props.theme.css.fRedColor};
    cursor: pointer;
  }

  .idir-logo {
    padding: 0;
    background-image: url('/assets/Logos_IDIR.svg');
    background-position: center;
    background-repeat: no-repeat;
    background-size: cover;
  }

  .azure-logo {
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

  .login-box {
    display: inline-block;
    position: relative;
    border: 2px solid ${(props) => props.theme.css.btnRedColor};
    border-radius: 0.5rem;
    padding-top: 2%;
    text-align: center;
    height: fit-content;
    @media (max-width: 1450px) {
      min-width: 50em;
    }
    @media (max-width: 768px) {
      margin-bottom: 0.5em;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
    }
    width: 50em;
  }

  .buttons {
    margin-top: 10%;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }

  button {
    color: ${(props) => props.theme.css.btnRedColor};
    font-size: 28px;
    font-weight: 400;
    line-height: 38px;
    letter-spacing: 0em;
    margin-top: 3%;
    margin-bottom: 3%;
    min-width: 4em;
    border: none;
    display: flex;
    justify-content: center;
    border: 1px solid black;
    clear: both;
    &.white {
      background-color: ${(props) => props.theme.css.bkWhite};
      &:hover {
        border: 2px solid #a61c29;
        color: ${(props) => props.theme.css.hPrimaryColor};
      }
    }
  }

  .modal {
    position: fixed;
    z-index: 1;
    padding-top: 15%;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    background-color: rgb(0, 0, 0);
    background-color: rgba(0, 0, 0, 0.4);
  }

  .modal-content {
    background-color: #fefefe;
    margin: auto;
    padding-top: 2%;
    padding-left: 5%;
    padding-right: 5%;
    border: 2px solid #6c5d62;
    border-radius: 0.5rem;
    width: 50%;
    height: 35%;
    font-size: 16px;
    line-height: 22px;
    letter-spacing: 0em;
    text-align: left;
  }

  .close {
    color: black;
    float: right;
    font-size: 28px;
    font-weight: bold;
  }

  .close:hover,
  .close:focus {
    color: black;
    text-decoration: none;
    cursor: pointer;
  }
`;
