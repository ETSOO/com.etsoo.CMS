import { CommonPage } from '@etsoo/react';
import React from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { app } from '../../app/MyApp';
import { alpha, LinearProgress, Theme, useTheme } from '@mui/material';
import { BusinessUtils } from '@etsoo/appscript';

// https://www.chartjs.org/docs/latest/getting-started/
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface ReportItem {
  rowIndex: number;
  income?: number;
  income0?: number;
  expense?: number;
  expense0?: number;
}

function parseData(rawData: ReportItem[], theme: Theme) {
  // Start month
  const startMonth = new Date().getMonth() + 1;

  // Month labels
  const months = BusinessUtils.getMonths(
    app.get<string[]>('months') ?? [],
    startMonth
  );

  // Labels
  const labels = app.getLabels(
    'expenseYTD',
    'expenseMOM',
    'incomeYTD',
    'incomeMOM'
  );

  // Datasets
  const datasets = [
    {
      label: labels.expenseYTD,
      data: rawData.map((d) => d.expense),
      borderColor: theme.palette.primary.main,
      backgroundColor: theme.palette.primary.main
    },
    {
      label: labels.expenseMOM,
      data: rawData.map((d) => d.expense0),
      borderColor: alpha(theme.palette.primary.main, 0.1),
      backgroundColor: alpha(theme.palette.primary.main, 0.1)
    },
    {
      label: labels.incomeYTD,
      data: rawData.map((d) => d.income),
      borderColor: theme.palette.error.main,
      backgroundColor: theme.palette.error.main
    },
    {
      label: labels.incomeMOM,
      data: rawData.map((d) => d.income0),
      borderColor: alpha(theme.palette.error.main, 0.1),
      backgroundColor: alpha(theme.palette.error.main, 0.1)
    }
  ];

  return {
    labels: months.map((m) => m.label),
    datasets
  };
}

function Reports() {
  // Theme
  const theme = useTheme();

  // State
  const [rawData, setRawData] = React.useState<ReportItem[]>();
  const isMounted = React.useRef(true);

  React.useEffect(() => {
    // Page title
    app.setPageKey('reports');

    // Load data
    app.serviceApi
      .get<ReportItem[]>('AccountLine/ReportDefault')
      .then((rawData) => {
        if (rawData == null) return;
        setRawData(rawData);
      });

    return () => {
      isMounted.current = false;
      app.pageExit();
    };
  }, []);

  return (
    <CommonPage>
      {rawData == null ? (
        <LinearProgress />
      ) : (
        <Line
          options={{
            responsive: true,
            plugins: {
              legend: {
                position: 'top' as const
              },
              title: {
                display: false,
                text: 'Line Chart'
              }
            }
          }}
          data={parseData(rawData, theme)}
        />
      )}
    </CommonPage>
  );
}

export default Reports;
