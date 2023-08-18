import styled from 'styled-components';

export const OverviewSection = styled.div`
  margin-bottom: 3em;
  border: 1px solid #003366;
  border-radius: 0.25px;

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
    padding: 1rem;
    display: flex;
    flex-direction: row;
    align-items: stretch;
  }
`;
