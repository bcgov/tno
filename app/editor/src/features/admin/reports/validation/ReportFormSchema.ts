import { toast } from 'react-toastify';
import { ContentTypeName } from 'tno-core';
import { array, date, number, object, string } from 'yup';

export const ReportFormSchema = object().shape(
  {
    name: string().required('Report should have a name.'),
    templateId: string().required('Report should have a template.'),
    settings: object({
      subject: object({ text: string().required('Email subject line should have a text.') }),
    }),
    // settings.subject.text:
    // name: string().required(() => {
    //   toast.error('value');
    // }),
    // name: string().when('tempSource', (value: string[]) => {
    //   if (value[0] === undefined) {
    //     // alert('name required');
    //     toast.error(`Name is missing on Report Tab.`);
    //     return string().required('Either source or other source is required.');
    //   }
    //   return string();
    // }),
  },
  [],
);
