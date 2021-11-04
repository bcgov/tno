import styled from 'styled-components';

export const Header = styled.header`
  display: flex;
  min-width: 768px;
  background-color: #003366;
  color: #ffffff;
  padding: 10px 00px 10px 3px;

  border-bottom: 2px solid #fcba19;

  div:first-child {
    align-self: flex-end;
  }

  div:nth-child(2):nth-last-child(2) {
    margin-left: 20px;
  }

  img {
    width: 175px;
  }

  div:last-child {
    display: flex;
    flex-grow: 1;
    justify-content: flex-end;
    align-self: center;

    div.title {
      font-size: 1.25rem;
      font-family: BCSans, 'Noto Sans', Verdana, Arial, sans serif;
      justify-content: flex-start;
      margin-left: 50px;
    }

    div div:last-child {
      margin-right: 10px;
    }
  }
`;

export default Header;
