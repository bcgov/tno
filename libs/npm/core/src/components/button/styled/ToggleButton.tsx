import styled from 'styled-components';

import { IToggleButtonProps } from '../ToggleButton';

export const ToggleButton = styled.div<Partial<IToggleButtonProps>>`
  display: flex;
  color: ${(props) =>
    props.disabled ? props.theme.css.linkGrayColor : props.theme.css.linkPrimaryColor};

  > div {
    display: flex;
    flex-direction: ${(props) =>
      props.labelPosition === 'left'
        ? 'row-reverse'
        : props.labelPosition === 'right'
        ? 'row'
        : props.labelPosition === 'top'
        ? 'column-reverse'
        : 'column'};
    gap: 0.5rem;
    align-items: center;

    &:hover {
      color: ${(props) =>
        props.disabled ? props.theme.css.linkGrayColor : props.theme.css.linkPrimaryHoverColor};
      cursor: ${(props) => (props.disabled ? 'unset' : 'pointer')};
    }

    > svg {
      width: ${(props) => props.width ?? 'unset'};
      height: ${(props) => props.height ?? 'unset'};
    }

    > label {
      cursor: ${(props) => (props.disabled ? 'unset' : 'pointer')};
    }
  }
`;
