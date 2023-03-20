import { useFormikContext } from 'formik';
import { FormikText, IConnectionModel } from 'tno-core';

export const LocalVolumeConfiguration = () => {
  const { values } = useFormikContext<IConnectionModel>();

  return (
    <div>
      <FormikText label="Path" name="configuration.path" value={values.configuration?.path} />
    </div>
  );
};
