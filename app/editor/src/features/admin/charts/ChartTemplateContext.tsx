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
  /** ChartRequestForm state. */
  chartRequestForm: IChartRequestForm;
  /** Mutate chartRequestForm state. */
  setChartRequestForm: React.Dispatch<React.SetStateAction<IChartRequestForm>>;
  /** Formik values. */
  values: IChartTemplateModel;
  /** Formik setFieldValue function */
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void;
  /** Formik setValues function */
  setValues: (
    values: React.SetStateAction<IChartTemplateModel>,
    shouldValidate?: boolean | undefined,
  ) => void;
  /** Chart data to be sent to chartRequestForm. */
  chartData?: string;
  /** Change chart data. */
  setChartData: React.Dispatch<React.SetStateAction<string | undefined>>;
  /** Filter to fetch data. */
  filter: string;
  /** Set the filter value. */
  setFilter: React.Dispatch<React.SetStateAction<string>>;
}

/**
 * ChartTemplateContext provides a shared context for chart template components.
 */
export const ChartTemplateContext = React.createContext<IChartTemplateContext>({
  chartRequestForm: defaultChartRequestForm,
  setChartRequestForm: (value: React.SetStateAction<IChartRequestForm>) => {},
  values: defaultChartTemplate,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => {},
  setValues: (
    values: React.SetStateAction<IChartTemplateModel>,
    shouldValidate?: boolean | undefined,
  ) => {},
  setChartData: (value: React.SetStateAction<string | undefined>) => {},
  filter: '',
  setFilter: (value: React.SetStateAction<string>) => {},
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
  const { values, setFieldValue, setValues } = useFormikContext<IChartTemplateModel>();

  const [filter, setFilter] = React.useState('');
  const [chartRequestForm, setChartRequestForm] = React.useState<IChartRequestForm>(value);
  const [chartData, setChartData] = React.useState(
    chartRequestForm.chartData ? JSON.stringify(chartRequestForm.chartData, null, 2) : undefined,
  );

  React.useEffect(() => {
    setChartRequestForm((chartRequestForm) => ({ ...chartRequestForm, template: values.template }));
  }, [values.template]);

  React.useEffect(() => {
    setChartRequestForm((chartRequestForm) => ({
      ...chartRequestForm,
      settings: { ...chartRequestForm.settings, options: values.settings.options },
    }));
  }, [values.settings.options]);

  React.useEffect(() => {
    if (chartData) {
      try {
        const data = JSON.parse(chartData);
        setChartRequestForm((chartRequestForm) => ({ ...chartRequestForm, chartData: data }));
      } catch {}
    }
  }, [chartData, setChartRequestForm]);

  return (
    <ChartTemplateContext.Provider
      value={{
        chartRequestForm,
        setChartRequestForm,
        values,
        setFieldValue,
        setValues,
        filter,
        setFilter,
        chartData,
        setChartData,
      }}
    >
      {children}
    </ChartTemplateContext.Provider>
  );
};
