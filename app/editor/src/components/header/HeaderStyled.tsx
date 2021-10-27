import styled from 'styled-components';

export const Header = styled.header`
  display: flex;
  min-width: 768px;
  background-color: #003366;
  color: #ffffff;
  padding: 10px 10px 10px 50px;

  border-bottom: 2px solid #fcba19;

  img {
    width: 175px;
  }

  div {
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
  }
`;

export default Header;
