import { ISpinnerProps, SpinnerVariant } from 'components';
import styled from 'styled-components';

export const Spinner = styled.div<ISpinnerProps>`
  display: inline-block;
  width: ${(props) => props.size};
  height: ${(props) => props.size};
  vertical-align: -0.125em;
  border: 0.25em solid;
  border-right: 0.25em solid transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;

  @keyframes spinner-border {
    100% {
      transform: rotate(1turn);
    }
  }

  color: ${(props) => {
    switch (props.variant) {
      case SpinnerVariant.secondary:
        return `rgba(${props.theme.css.secondaryRgb}, ${props.theme.css.textOpacity})`; // 'rgba(var(' + props.theme.bs-secondary-rgb + '),var(--bs-text-opacity))';
      case SpinnerVariant.success:
        return `rgba(${props.theme.css.successRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.info:
        return `rgba(${props.theme.css.infoRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.warning:
        return `rgba(${props.theme.css.warningRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.danger:
        return `rgba(${props.theme.css.dangerRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.light:
        return `rgba(${props.theme.css.lightRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.dark:
        return `rgba(${props.theme.css.darkRgb}, ${props.theme.css.textOpacity})`;
      case SpinnerVariant.primary:
      default:
        return `rgba(${props.theme.css.primaryRgb}, ${props.theme.css.textOpacity})`;
    }
  }};
`;

export default Spinner;
