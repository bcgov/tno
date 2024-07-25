import 'react-quill/dist/quill.snow.css';

import { getIn, useFormikContext } from 'formik';

import { Error, IWysiwygProps, Wysiwyg } from '../../form';

export interface IFormikWysiwygProps<T> extends Omit<IWysiwygProps, 'name' | 'value'> {
  /** the formik field that is being used within the WYSIWYG */
  name: keyof T;
}

/**
 * A Formik WYSIWYG component.
 * @param props Component props.
 * @returns A component.
 */
export const FormikWysiwyg = <T extends any>({
  name,
  className,
  onChange,
  onBlur,
  urlOptions,
  ...rest
}: IFormikWysiwygProps<T>) => {
  const { values, setFieldValue, errors, touched } = useFormikContext<T>();

  const text = getIn(values, name.toString());
  onChange =
    onChange ??
    ((text) => {
      setFieldValue(name.toString(), text);
    });
  return (
    <>
      <Wysiwyg
        name={name.toString()}
        value={text}
        urlOptions={urlOptions}
        onChange={onChange}
        className={`frm-in${className ? ` ${className}` : ''}`}
        {...rest}
      />
      <Error error={!!name && touched[name] ? (errors[name] as string) : ''} />
    </>
  );
};
