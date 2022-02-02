import React from 'react';

export const Home = () => {
  return (
    <div>
      <p>Welcome to TNO</p>
      <p>{process.env.REACT_APP_NOT_SECRET_CODE}</p>
    </div>
  );
};
