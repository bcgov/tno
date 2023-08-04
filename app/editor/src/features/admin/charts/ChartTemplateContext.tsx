import { useFormikContext } from 'formik';
import React from 'react';
import { IChartTemplateModel } from 'tno-core';

import { defaultChartRequestForm, defaultChartTemplate } from './constants';
import { IChartRequestForm } from './interfaces';

/*******************************************************************
 * All related chart template context code is in a single file because it will never be used separately.
 *******************************************************************/

/**
 * IChartTemplateContext interface for the context.
 */
export interface IChartTemplateContext {
  /** Preview state. */
  preview: IChartRequestForm;
  /** Mutate preview state. */
  setPreview: React.Dispatch<React.SetStateAction<IChartRequestForm>>;
  /** Formik values. */
  values: IChartTemplateModel;
  /** Formik setFieldValue function */
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void;
}

/**
 * ChartTemplateContext provides a shared context for chart template components.
 */
export const ChartTemplateContext = React.createContext<IChartTemplateContext>({
  preview: defaultChartRequestForm,
  setPreview: (value: React.SetStateAction<IChartRequestForm>) => {},
  values: defaultChartTemplate,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => {},
});

/**
 * Provides a way for child components to interact with shared state.
 * @returns The context state.
 */
export const useChartTemplateContext = () => {
  return React.useContext(ChartTemplateContext);
};

/**
 * IChartTemplateContextProviderProps interface for the component.
 */
export interface IChartTemplateContextProviderProps {
  /** The initial value of state. */
  value?: IChartRequestForm;
  /** Children elements to display within this context provider. */
  children?: React.ReactNode;
}

/**
 * ChartTemplateContextProvider provides a component to wrap elements that need access to shared state.
 * @param param0 Component properties.
 * @returns Component context provider.
 */
export const ChartTemplateContextProvider: React.FC<IChartTemplateContextProviderProps> = ({
  value = defaultChartRequestForm,
  children,
}) => {
  const { values, setFieldValue } = useFormikContext<IChartTemplateModel>();

  const [preview, setPreview] = React.useState<IChartRequestForm>(value);

  React.useEffect(() => {
    setPreview((preview) => ({ ...preview, template: values.template }));
  }, [values.template]);

  React.useEffect(() => {
    setPreview((preview) => ({
      ...preview,
      settings: { ...preview.settings, options: values.settings.options },
    }));
  }, [values.settings.options]);

  return (
    <ChartTemplateContext.Provider value={{ preview, setPreview, values, setFieldValue }}>
      {children}
    </ChartTemplateContext.Provider>
  );
};
