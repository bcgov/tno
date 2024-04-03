import { useFormikContext } from 'formik';
import React from 'react';
import { IReportModel, IReportResultModel } from 'tno-core';

import { defaultReport } from './constants';
import { openPreviewInNewTab } from './utils/openPreviewInNewTab';

/*******************************************************************
 * All related report context code is in a single file because it will never be used separately.
 *******************************************************************/

/**
 * IReportTemplateContext interface for the context.
 */
export interface IReportTemplateContext {
  /** Preview state. */
  preview?: IReportResultModel;
  /** Mutate preview state. */
  setPreview: React.Dispatch<React.SetStateAction<IReportResultModel | undefined>>;
  /** Formik values. */
  values: IReportModel;
  /** Formik setFieldValue function */
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void;
  /** Open the preview in a new tab. */
  openPreview: () => void;
}

/**
 * ReportTemplateContext provides a shared context for report components.
 */
export const ReportTemplateContext = React.createContext<IReportTemplateContext>({
  preview: undefined,
  setPreview: (value: React.SetStateAction<IReportResultModel | undefined>) => {},
  values: defaultReport,
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => {},
  openPreview: () => {},
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
  value?: IReportResultModel;
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

  const [preview, setPreview] = React.useState<IReportResultModel | undefined>(value);

  const openPreview = () => preview && openPreviewInNewTab(preview);

  return (
    <ReportTemplateContext.Provider
      value={{ preview, setPreview, values, setFieldValue, openPreview }}
    >
      {children}
    </ReportTemplateContext.Provider>
  );
};
