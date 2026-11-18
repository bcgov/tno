import styled from 'styled-components';

export const PressGallery = styled.div`
  .tool-bar {
    margin-left: auto;
    margin-right: auto;
    width: fit-content;
    .reset {
      margin-top: auto;
      margin-bottom: auto;
      height: 20px;
      width: 20px;
      margin-left: 1em;
      color: ${({ theme }) => theme.css.btnBkPrimary};
      &:hover {
        cursor: pointer;
        transform: scale(1.1);
      }
    }
  }
`;
