import styled from 'styled-components';

export const SearchForm = styled.div`
  max-height: calc(100vh - 6.5em);
  .checkboxes {
    margin-left: 0.5em;
    input {
      margin-left: 0.25em;
      margin-right: 0.25em;
    }
  }
  .search-in {
    margin-top: 1em;
    margin-bottom: 1em;
  }
  /* padding: 0.5em; */
  overflow: none;
  .main {
    overflow-y: auto;
    .header {
      background-color: ${(props) => props.theme.css.darkHeaderColor};
      padding: 0.5em;
      .title {
        margin-left: auto;
      }
      font-size: 1.75em;
      color: white;
      font-family: 'Source Sans Pro', sans-serif;
      .back-arrow {
        margin-top: 0.15em;
        &:hover {
          &:hover {
            cursor: pointer;
            transform: scale(1.1);
            color: ${(props) => props.theme.css.sideBarIconHoverColor};
          }
        }
        color: #a5a4bf;
      }
    }
    .form-container {
      padding: 1em;
    }
  }
`;
