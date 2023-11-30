import { Button } from 'components/button';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTelegramPlane } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useReportInstances } from 'store/hooks';
import { Row, Text } from 'tno-core';

export const ReportSendForm: React.FC = () => {
  const { values, isSubmitting } = useFormikContext<IReportForm>();
  const [{ sendReportInstance }] = useReportInstances();

  const [to, setTo] = React.useState('');

  const instanceId = values.instances.length ? values.instances[0].id : undefined;

  const handleSend = React.useCallback(
    async (id: number, to: string) => {
      try {
        await sendReportInstance(id, to);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [sendReportInstance],
  );

  return (
    <Row>
      <label>Send to:</label>
      <Text name="email" value={to} onChange={(e) => setTo(e.target.value)}></Text>
      <Button
        disabled={isSubmitting || !to.length}
        title="Send now"
        onClick={() => !!instanceId && handleSend(instanceId, to)}
      >
        <FaTelegramPlane />
      </Button>
    </Row>
  );
};
