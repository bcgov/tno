import styled from 'styled-components';

export const ScheduleForm = styled.div`
  .timing {
    p {
      padding-left: 0.5em;
      padding-right: 1em;
    }
  }

  label {
    font-weight: bold;
  }

  .actions {
    background-color: ${(props) => props.theme.css.filterBackgroundColor};
    padding: 1em;
  }
`;
