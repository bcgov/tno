import styled from 'styled-components';

export const ClipDirectoryTable = styled.div`
  width: 100%;

  .file-table {
    width: 100%;
  }

  .file-actions {
    & > div:not(:first-child) {
      margin-left: 0.5em;
    }

    svg {
      color: ${(props) => props.theme.css.primaryColor};
      cursor: pointer;
    }

    .delete {
      color: ${(props) => props.theme.css.dangerColor};
    }
  }

  .hidden {
    display: none;
  }

  .ft-row {
    padding-top: 4px;
  }
`;
