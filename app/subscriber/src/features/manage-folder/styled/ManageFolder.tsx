import styled from 'styled-components';

export const ManageFolder = styled.div`
  .main-manage {
    overflow-y: auto;
    overflow-x: clip;
    max-height: calc(100vh - 4em);
  }
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
    margin-left: auto;
  }

  .full-draggable {
    &:nth-child(even) {
      background-color: rgb(233, 236, 239);
    }
  }
  .item-draggable {
    padding: 0.25em;
    padding-left: 0.5em;
    padding-right: 0.5em;
    .tone-date {
      margin-left: auto;
      .date {
        color: #8f929d;
      }
      svg {
        margin-top: 0.5em;
        margin-right: 0.5em;
        margin-left: 0;
      }
    }
    .checkbox {
      height: 1.5em;
      width: 1.5em;
      margin-top: 0.25em;
    }
    .item-headline {
      color: #3847aa;
      font-size: 1.15em;
      text-decoration: underline;
      :hover {
        cursor: pointer;
      }
    }
    .grip-lines {
      margin-top: 0.5em;
      margin-left: auto;
    }
  }
`;
