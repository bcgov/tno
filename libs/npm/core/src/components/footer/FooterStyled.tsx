import styled from 'styled-components';

export const Footer = styled.footer`
  display: flex;
  min-width: 768px;
  background-color: #003366;
  color: #ffffff;
  padding: 5px 5px 5px 50px;

  border-top: 2px solid #fcba19;

  div {
    display: flex;
    flex-grow: 1;

    justify-content: flex-start;
    align-self: center;
  }

  a {
    padding: 5px 10px 5px 10px;
  }

  a:not(:last-child) {
    border-right: 1px solid #4b5e73;
  }

  a:link {
    color: #fff;
  }

  a:visited {
    color: #fff;
  }

  a:hover {
    color: #fff;
    opacity: 0.75;
  }
`;

export default Footer;
