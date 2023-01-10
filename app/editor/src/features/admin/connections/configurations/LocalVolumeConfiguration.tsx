import { useFormikContext } from 'formik';
import { IConnectionModel } from 'hooks';
import { FormikText } from 'tno-core';

export const LocalVolumeConfiguration = () => {
  const { values } = useFormikContext<IConnectionModel>();

  return (
    <div>
      <FormikText label="Path" name="configuration.path" value={values.configuration?.path} />
    </div>
  );
};
