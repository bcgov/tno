import { useFormikContext } from 'formik';
import React from 'react';
import { IReportModel, IReportPreviewModel } from 'tno-core';

import { defaultReport } from './constants';

/*******************************************************************
 * All related report context code is in a single file because it will never be used separately.
 *******************************************************************/

/**
 * IReportTemplateContext interface for the context.
 */
export interface IReportTemplateContext {
  /** Preview state. */
  preview?: IReportPreviewModel;
  /** Mutate preview state. */
  setPreview: React.Dispatch<React.SetStateAction<IReportPreviewModel | undefined>>;
  /** Formik values. */
  values: IReportModel;
  /** Formik setFieldValue function */
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void;
}

/**
 * ReportTemplateContext provides a shared context for report components.
 */
export const ReportTemplateContext = React.createContext<IReportTemplateContext>({
  preview: undefined,
  setPreview: (value: React.SetStateAction<IReportPreviewModel | undefined>) => {},
  values: defaultReport,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => {},
});

/**
 * Provides a way for child components to interact with shared state.
 * @returns The context state.
 */
export const useReportTemplateContext = () => {
  return React.useContext(ReportTemplateContext);
};

/**
 * IReportTemplateContextProviderProps interface for the component.
 */
export interface IReportTemplateContextProviderProps {
  /** The initial value of state. */
  value?: IReportPreviewModel;
  /** Children elements to display within this context provider. */
  children?: React.ReactNode;
}

/**
 * ReportTemplateContextProvider provides a component to wrap elements that need access to shared state.
 * @param param0 Component properties.
 * @returns Component context provider.
 */
export const ReportTemplateContextProvider: React.FC<IReportTemplateContextProviderProps> = ({
  value,
  children,
}) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const [preview, setPreview] = React.useState<IReportPreviewModel | undefined>(value);

  return (
    <ReportTemplateContext.Provider value={{ preview, setPreview, values, setFieldValue }}>
      {children}
    </ReportTemplateContext.Provider>
  );
};
