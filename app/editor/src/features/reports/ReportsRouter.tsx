import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import { CBRAReport } from '.';

export const ReportsRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="cbra" />} />
      <Route path="cbra" element={<CBRAReport />} />
    </Routes>
  );
};
