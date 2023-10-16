import { toast } from 'react-toastify';
import { ContentTypeName } from 'tno-core';
import { array, date, number, object, string } from 'yup';

export const ReportFormSchema = object().shape(
  {
    name: string().required(() => {
      toast.error('value');
    }),
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
