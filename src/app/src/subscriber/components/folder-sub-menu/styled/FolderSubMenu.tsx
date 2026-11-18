import styled from 'styled-components';

export const FolderSubMenu = styled.div`
  /* add to folder button */
  .add-folder {
    height: 2em;
    width: 2em;
    color: ${(props) => props.theme.css.btnBkPrimary};
    &:hover {
      cursor: pointer;
    }
    color: ${(props) => props.theme.css.sideBarIconColor};
  }
`;
