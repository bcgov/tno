import styled from 'styled-components';

export const ManageFolder = styled.div`
  .manage-title {
    margin-left: 0.5em;
  }
  .remove-icon {
    margin-top: 0.15em;
    margin-left: 0.5em;
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sideBarIconHoverColor};
    }
    color: ${(props) => props.theme.css.sideBarIconColor};
  }
  .title {
    margin-left: 0.1em;
  }
  .header-col {
    width: 100%;
  }
  .header-row {
    width: 100%;
    .close-button {
      margin-left: auto;
      margin-bottom: 0.67em;
      cursor: pointer;
      color: ${({ theme }) => theme.css.iconGrayColor};
      height: 1.2rem;
      width: 1.2rem;
    }
  }
`;
