import { Formik, useFormikContext } from 'formik';
import React from 'react';
import { type IFilterAIManualFilterProps, type IFilterSettingsModel } from 'tno-core';

import { FilterSettingsForm } from './FilterSettingsForm';
import * as styled from './styled';

interface ISettingsObserverProps {
  onChange: (settings: IFilterSettingsModel) => void;
}

/** Reports every change to the settings, including fields that only write to Formik. */
const SettingsObserver: React.FC<ISettingsObserverProps> = ({ onChange }) => {
  const { values } = useFormikContext<{ settings: IFilterSettingsModel }>();
  const last = React.useRef(JSON.stringify(values.settings));

  React.useEffect(() => {
    const json = JSON.stringify(values.settings);
    if (json === last.current) return;
    last.current = json;
    onChange(values.settings);
  }, [values.settings, onChange]);

  return null;
};

/**
 * The filter settings form in its own Formik, for the filter prompt workbench's manual tab.
 * It starts from the settings it is given and reports each change; remount it to reset it.
 */
export const ManualFilterForm: React.FC<IFilterAIManualFilterProps> = ({ settings, onChange }) => {
  return (
    <styled.ManualFilterForm className="manual-filter-form">
      <Formik initialValues={{ settings }} onSubmit={() => {}}>
        <>
          <FilterSettingsForm path="settings" />
          <SettingsObserver onChange={onChange} />
        </>
      </Formik>
    </styled.ManualFilterForm>
  );
};
