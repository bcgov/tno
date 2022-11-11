import styled from 'styled-components';

export const Header = styled.header`
  display: flex;
  min-width: 768px;
  background-color: #003366;
  color: #ffffff;
  padding: 10px 00px 10px 3px;

  border-bottom: 2px solid #fcba19;

  & > div:first-child {
    margin-left: 5em;
    align-self: flex-end;

    img {
      width: 175px;
    }
  }

  & > div:last-child {
    display: flex;
    flex-direction: row;
    align-items: stretch;
    margin-left: 2em;
    width: 100%;

    .title {
      flex-grow: 1;
      font-size: 1.25rem;
      font-family: ${(props) => props.theme.css?.bcSans};
      justify-content: flex-start;
      margin-top: auto;
      margin-bottom: auto;
    }

    .options {
      flex-shrink: 1;
      margin-right: 5em;
      margin-top: auto;
      margin-bottom: auto;
      text-align: center;
    }
  }
`;
