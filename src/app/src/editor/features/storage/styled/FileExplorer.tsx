import styled from 'styled-components';

export const FileExplorer = styled.div`
  width: 100%;

  .file-table {
    width: 100%;
  }

  .file-actions {
    & > div:not(:first-child) {
      margin-left: 0.5em;
    }

    & > div {
      width: 100%;
    }

    svg {
      color: ${(props) => props.theme.css.primaryColor};
      cursor: pointer;
    }

    .delete {
      color: ${(props) => props.theme.css.dangerColor};
    }
  }

  .link {
    cursor: pointer;
    color: ${(props) => props.theme.css.actionButtonColor};

    &:hover {
      color: ${(props) => props.theme.css.activeColor};
      text-decoration: underline;
    }
  }

  .hidden {
    display: none;
  }

  .ft-row {
    padding-top: 4px;
  }
`;
