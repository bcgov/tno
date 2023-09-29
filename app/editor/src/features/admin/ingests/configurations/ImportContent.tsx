import { useFormikContext } from 'formik';
import { FormikCheckbox, IIngestModel } from 'tno-core';

export const ImportContent: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  return (
    <>
      <FormikCheckbox
        label="Post to Kafka"
        name="configuration.post"
        tooltip="Only post to Kafka if the content can be added to MMI."
        onChange={(e) => {
          setFieldValue('configuration', {
            ...values.configuration,
            post: e.target.checked,
            import: e.target.checked ? values.configuration.import : false,
          });
        }}
      />
      <FormikCheckbox
        label="Add content to database"
        name="configuration.import"
        disabled={!values.configuration.post}
      />
      <hr />
    </>
  );
};
