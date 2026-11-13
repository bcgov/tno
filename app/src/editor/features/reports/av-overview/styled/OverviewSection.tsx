import styled from 'styled-components';

export const OverviewSection = styled.div`
  margin-bottom: 1em;
  border: 1px solid #003366;
  border-radius: 0.25rem;

  > div:first-child {
    margin-bottom: unset;
  }

  .section-title {
    font-weight: bold;
    align-self: center;
  }

  .disabled {
    border: none;
  }

  .edit-button {
    height: 2em;
  }

  .title-edit {
    margin-bottom: 1em;
  }

  .buttons {
    padding: 0.25rem;
    display: flex;
    gap: 0.5rem;
    flex-direction: row;
    align-items: stretch;

    > div:first-child {
      gap: 0.5rem;
    }
  }
`;
