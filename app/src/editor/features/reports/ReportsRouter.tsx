import React from 'react';
import { Navigate, Route, Routes } from 'react-router';

import { CBRAReport } from '.';

const ReportsRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="cbra" />} />
      <Route path="cbra" element={<CBRAReport />} />
    </Routes>
  );
};

export default ReportsRouter;
