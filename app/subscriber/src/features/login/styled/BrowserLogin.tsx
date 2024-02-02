import styled from 'styled-components';

export const BrowserLogin = styled.div`
  .containing-row {
    margin-left: auto;
    margin-right: auto;
  }

  a {
    color: ${(props) => props.theme.css.fRedColor};
  }

  .modalOpen {
    color: ${(props) => props.theme.css.fRedColor};
    cursor: pointer;
  }

  .bceid-logo {
    padding: 1%;
    background-image: url('/assets/bceid_default_logo.jpg');
    background-position: center;
    background-repeat: no-repeat;
    background-size: cover;
  }

  .idir-logo {
    padding: 1%;
    background-image: url('/assets/idir_logo_2.png');
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
    position: relative;
    margin-top: 2%;
    border: 2px solid #971d29;
    border-radius: 0.5rem;
    padding: 2%;
    text-align: center;
    height: 487px;
    display: flex;
    flex-direction: column;
    float: left;
    @media (max-width: 1450px) {
      min-width: 98%;
      margin-left: 1%;
    }
    @media (max-width: 768px) {
      margin-bottom: 0.5em;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
    }
    width: 50em;
    margin-right: 1%;
    .footer {
      padding: 2%;
      text-align: left;
      position: absolute;
      bottom: 0;
      left: 0;
      background-color: #ded9da;
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
    color: ${(props) => props.theme.css.redHeadingColor};
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
    padding-top: 8%;
    padding-left: 5%;
    padding-right: 5%;
    border: 1px solid #888;
    width: 80%;
    height: 45%;
    font-size: 16px;
    line-height: 22px;
    letter-spacing: 0em;
    text-align: left;
  }

  .close {
    color: #aaaaaa;
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
