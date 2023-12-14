import { FormikErrors, useFormikContext } from 'formik';
import { set, transform } from 'lodash';
import React from 'react';
import { ValidationError } from 'yup';

import { IFormikTab } from './interfaces';
import { ITabsProps, Tabs } from './Tabs';

export interface IFormikTabsProps<T extends unknown> extends ITabsProps {
  tabs: IFormikTab<T>[];
  validateForm?: (values?: any) => Promise<FormikErrors<T>>;
}

export const FormikTabs = <T extends unknown>({
  tabs: initTabs,
  onChange = () => Promise.resolve(true),
  validateForm,
  ...rest
}: IFormikTabsProps<T>) => {
  const { values, setStatus, setTouched, isSubmitting, errors, touched } = useFormikContext<T>();

  const [tabs, setTabs] = React.useState<IFormikTab<T>[]>(initTabs);

  React.useEffect(() => {
    setTabs(initTabs);
  }, [initTabs]);

  const validateTab = React.useCallback(
    async (tab: IFormikTab<T>) => {
      if (tab.validateOnChange && tab.validationSchema) {
        try {
          await tab.validationSchema.validate(values);
          tab.className = `${tab.className}`.trim().replace('error', '');
          tab.touched = undefined;
          tab.errors = undefined;
        } catch (error) {
          const vError = error as ValidationError;
          const result = transform(
            { [vError.path ?? '_']: vError.errors },
            (result, value, key) => {
              set(result, key, value);
            },
            {},
          );
          tab.className = `${tab.className}`.trim().replace('error', '').trim() + ' error';
          tab.touched = result;
          tab.errors = result;
        }
      }
      return tab;
    },
    [values],
  );

  const handleChange = React.useCallback(
    async (tab: IFormikTab<T>, active?: IFormikTab<T>) => {
      await onChange?.(tab, active);
      if (active) {
        const result = await validateTab(active);
        setTabs((tabs) => tabs.map((t) => (t.key === tab.key ? tab : t)));
        setTouched({ ...touched, ...(result.touched ?? {}) });
        setStatus({ ...errors, ...result.errors });
      }
      return true;
    },
    [errors, onChange, setStatus, setTouched, touched, validateTab],
  );

  React.useEffect(() => {
    if (isSubmitting) {
      const results = tabs.map(async (tab) => {
        return await validateTab(tab);
      });
      Promise.all(results).then((tabs) => {
        tabs.forEach((tab) => {
          setTouched({ ...touched, ...(tab.touched ?? {}) });
          setStatus({ ...errors, ...tab.errors });
        });
        setTabs(tabs);
      });
    }
    // Only validate when submitting.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isSubmitting]);

  return <Tabs tabs={tabs} onChange={handleChange} {...rest} />;
};
