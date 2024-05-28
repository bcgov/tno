import styled from 'styled-components';

export const MyFolders = styled.div`
  max-height: calc(100vh - 6.5em);
  /* option in tooltip */
  .option {
    &:hover {
      cursor: pointer;
      text-decoration: underline;
    }
  }
  .active-folder-row {
    border-left: 0.5em solid ${(props) => props.theme.css.highlightActive};
    padding-left: 0.15em;
  }
  .inactive-folder-row {
    padding-left: 0.65em;
  }

  .folder-name {
    height: 1.5em;
    margin-left: 0.5em;
    max-width: 100%;
  }
  .folder-text {
    display: flex;
    align-items: center;
  }
  .create-label {
    margin-left: 0.5em;
  }
  .create-new {
    flex-wrap: nowrap;
    max-width: fit-content;
    margin-left: auto;
    margin-right: auto;
    margin-bottom: 0.5em;
    margin-top: 1rem;

    .wand {
      margin-top: auto;
      margin-right: 0.25em;
      margin-bottom: auto;
    }
    .create-text {
      font-size: 1rem;
      font-weight: 800;
      align-self: center;
    }
  }
  .create-button {
    height: 2.5em;
    width: 2.5em;
    display: flex;
    border: none;
    &:focus {
      outline: none;
    }
    cursor: pointer;
    border-radius: 0.5em;
    svg {
      display: flex;
      align-self: center;
      margin-right: auto;
      margin-left: auto;
    }
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    color: ${(props) => props.theme.css.btnPrimaryColor};
    padding-bottom: 0.25em;
    &:hover {
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sidebarIconHoverColor};
    }
  }

  .re-name {
    input {
      outline: none;
      border: none;
      box-shadow: none;
      border-bottom: 1px solid ${(props) => props.theme.css.btnPkPrimary};
      border-radius: 0;
      font-size: 1em;
      padding: 0;
    }
  }
`;
