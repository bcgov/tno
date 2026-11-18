import styled from 'styled-components';

export const ContentEditForm = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;

  > div:first-child {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;
    justify-content: space-between;

    button {
      padding: 0.15rem;

      svg {
        min-height: 12px;
        max-height: 12px;
        min-width: 12px;
        max-width: 12px;
      }
    }
  }

  > div {
    flex: 0;
  }

  > div:not(:last-child) {
    padding: 0 0.5rem;
  }

  h1 {
    margin: 0;
    color: ${(props) => props.theme.css.hPrimaryColor};
    font-size: 1.75rem;
  }

  .edit-content {
    flex: 1;
  }
`;
