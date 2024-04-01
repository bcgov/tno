import { useFormikContext } from 'formik';
import React from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances, useReports } from 'store/hooks';
import { IReportInstanceModel } from 'tno-core';

import { defaultReport } from '../constants';
import { IReportForm, IReportInstanceContentForm } from '../interfaces';
import { toForm } from '../utils';
import {
  ReportContentMenuOption,
  ReportMainMenuOption,
  ReportSettingsMenuOption,
  ReportViewMenuOption,
} from './constants';

/*******************************************************************
 * All related chart template context code is in a single file because it will never be used separately.
 *******************************************************************/

/**
 * IReportEditContext interface for the context.
 */
export interface IReportEditContext {
  /** The active report page */
  active?: string;
  /** Change the active report page. */
  setActive: React.Dispatch<React.SetStateAction<string | undefined>>;
  /** The active content row. */
  activeRow?: IReportInstanceContentForm;
  /** Change the active content row. */
  setActiveRow: React.Dispatch<React.SetStateAction<IReportInstanceContentForm | undefined>>;
  /** Formik values. */
  values: IReportForm;
  /** Formik setFieldValue function */
  setFieldValue: (field: string, value: any, shouldValidate?: boolean | undefined) => void;
  /** Formik setValues function */
  setValues: (
    values: React.SetStateAction<IReportForm>,
    shouldValidate?: boolean | undefined,
  ) => void;
  isSubmitting: boolean;
  setSubmitting: (isSubmitting: boolean) => void;
  submitForm: (() => Promise<void>) & (() => Promise<any>);
  onNavigate: (instance: IReportInstanceModel | undefined, action: 'previous' | 'next') => void;
  onExport: (report: IReportForm) => void;
  onRegenerate: (values: IReportForm, regenerate: boolean) => Promise<IReportForm | undefined>;
}

/**
 * ReportEditContext provides a shared context for chart template components.
 */
export const ReportEditContext = React.createContext<IReportEditContext>({
  setActive: () => {},
  setActiveRow: () => {},
  values: defaultReport(0, 0),
  setFieldValue: () => {},
  setValues: () => {},
  isSubmitting: false,
  setSubmitting: () => {},
  submitForm: () => Promise.resolve(),
  onNavigate: () => {},
  onExport: () => {},
  onRegenerate: () => Promise.resolve(undefined),
});

/**
 * Provides a way for child components to interact with shared state.
 * @returns The context state.
 */
export const useReportEditContext = () => {
  return React.useContext(ReportEditContext);
};

/**
 * IReportEditContextProviderProps interface for the component.
 */
export interface IReportEditContextProviderProps {
  /** Children elements to display within this context provider. */
  children?: React.ReactNode;
}

/**
 * ReportEditContextProvider provides a component to wrap elements that need access to shared state.
 * @param param0 Component properties.
 * @returns Component context provider.
 */
export const ReportEditContextProvider: React.FC<IReportEditContextProviderProps> = ({
  children,
}) => {
  const { values, setFieldValue, setValues, isSubmitting, setSubmitting, submitForm } =
    useFormikContext<IReportForm>();
  const { path1, path2 } = useParams();
  const [{ exportReport }] = useReportInstances();
  const [, { generateReport }] = useReports();

  const [active, setActive] = React.useState<string>();
  const [activeRow, setActiveRow] = React.useState<IReportInstanceContentForm>();

  React.useEffect(() => {
    // Set the active form based on the route.
    const main = path1?.toLocaleLowerCase();
    const secondary = path2?.toLocaleLowerCase();
    const path = `${main ?? ''}${secondary ? `/${secondary}` : ''}`;
    if (path === ReportMainMenuOption.Settings) setActive(ReportMainMenuOption.Settings);
    else if (path === ReportSettingsMenuOption.Info) setActive(ReportSettingsMenuOption.Info);
    else if (path === ReportSettingsMenuOption.Sections)
      setActive(ReportSettingsMenuOption.Sections);
    else if (path === ReportSettingsMenuOption.DataSources)
      setActive(ReportSettingsMenuOption.DataSources);
    else if (path === ReportSettingsMenuOption.Preferences)
      setActive(ReportSettingsMenuOption.Preferences);
    else if (path === ReportMainMenuOption.Content) setActive(ReportMainMenuOption.Content);
    else if (path === ReportContentMenuOption.Content) setActive(ReportContentMenuOption.Content);
    else if (path === ReportContentMenuOption.Sort) setActive(ReportContentMenuOption.Sort);
    else if (path === ReportContentMenuOption.Summary) setActive(ReportContentMenuOption.Summary);
    else if (path === ReportMainMenuOption.View) setActive(ReportMainMenuOption.View);
    else if (path === ReportViewMenuOption.View) setActive(ReportViewMenuOption.View);
    else if (path === ReportMainMenuOption.Send) setActive(ReportMainMenuOption.Send);
    else if (path === ReportSettingsMenuOption.Send) setActive(ReportSettingsMenuOption.Send);
    else setActive(ReportMainMenuOption.Settings);
  }, [path1, path2]);

  const onNavigate = React.useCallback(
    (instance: IReportInstanceModel | undefined, action: 'previous' | 'next') => {
      if (activeRow && instance && instance.content.length) {
        let index = activeRow.originalIndex;

        if (action === 'previous') index = index > 0 ? index - 1 : instance.content.length - 1;
        else if (action === 'next') index = index < instance.content.length - 1 ? index + 1 : 0;
        setActiveRow({
          ...instance.content[index],
          originalIndex: index,
        });
      }
    },
    [activeRow],
  );

  const onExport = React.useCallback(
    async (report: IReportForm) => {
      try {
        if (report?.id) {
          const instance = report.instances.length ? report.instances[0] : 0;
          if (instance) {
            const filename = report.name.replace(/[^a-zA-Z0-9 ]/g, '');
            await toast.promise(exportReport(instance.id, filename), {
              pending: 'Downloading file',
              success: 'Download complete',
              error: 'Download failed',
            });
          } else {
            toast.error(`The report has not been generated.`);
          }
        }
      } catch {}
    },
    [exportReport],
  );

  const onRegenerate = React.useCallback(
    async (values: IReportForm, regenerate: boolean) => {
      try {
        const report = await generateReport(values.id, regenerate);
        const form = toForm(report, true);
        setValues(form);
        if (regenerate) toast.success('Report has been regenerated');
        return form;
      } catch {}
    },
    [generateReport, setValues],
  );

  return (
    <ReportEditContext.Provider
      value={{
        active,
        setActive,
        activeRow,
        setActiveRow,
        values,
        setFieldValue,
        setValues,
        isSubmitting,
        setSubmitting,
        submitForm,
        onNavigate,
        onExport,
        onRegenerate,
      }}
    >
      {children}
    </ReportEditContext.Provider>
  );
};
