import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IConnectionModel } from 'hooks';

export const LocalVolumeConfiguration = () => {
  const { values } = useFormikContext<IConnectionModel>();

  return (
    <div>
      <FormikText label="Path" name="configuration.path" value={values.configuration?.path} />
    </div>
  );
};
