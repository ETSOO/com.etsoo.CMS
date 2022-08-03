import React from 'react';
import { CommonPage, MUGlobal } from '@etsoo/react';
import { app } from '../../app/MyApp';
import { useNavigate } from 'react-router-dom';

function Dashboard() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'registerIncome',
    'registerExpense',
    'writeOff',
    'leaderApproval',
    'income',
    'expense'
  );

  // User context
  const Context = app.userState.context;

  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Load data
  const reloadData = async () => {};

  React.useEffect(() => {
    // Page title
    app.setPageKey('menuHome');
  }, []);

  return <CommonPage onUpdateAll={reloadData} paddings={paddings}></CommonPage>;
}

export default Dashboard;
