import styled from 'styled-components';

export interface IToneSelectorProps {
  required?: boolean;
  active?: boolean;
}

export const ToneSelector = styled.div.attrs<IToneSelectorProps>(({ required }) => ({
  required,
}))<IToneSelectorProps>`
  display: flex;
  flex-direction: column;

  img {
    height: 20px;
    width: 20px;
    align-self: center;
  }

  button {
    background-color: #f2f2f2;
    border-radius: 15%;
    border-color: #f2f2f2;
    margin: 0 0.25rem;
    margin-top: 0.5rem;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
    min-width: 2rem;
    min-height: 2rem;
    align-self: center;
    &:hover {
      cursor: pointer;
    }
  }

  .blank {
    height: 20px;
    width: 20px;
  }

  .active {
    background-color: #007af5;
    color: white;
    border: none;
    min-height: 2rem;
    min-width: 2rem;
  }

  label {
    align-self: flex-start;
    font-weight: 600;
    :after {
      content: '${(props) => (props.required ? ' *' : '')}';
      color: red;
    }
  }

  .tone-icon {
    font-size: 1.4rem;
    margin-bottom: 0rem;
    margin-left: 0.5rem;
  }
`;
