import styled from 'styled-components';

export const ContentEditForm = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;

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
